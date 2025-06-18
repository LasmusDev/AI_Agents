using System;
using System.Collections;
using UnityEngine;
using Normal.Realtime.Native;
using Normal.Utility;
using UnityEngine.XR;
#if !UNITY_EDITOR && UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace NormcoreAvatars
{
#if UNITY_EDITOR || !UNITY_WEBGL
    [DefaultExecutionOrder(-1)] // Make sure our Update() runs before the default to ensure _microphoneDbLevel has been calculated for CalculateVoiceVolume()
    public class CustomRealtimeAvatarVoice : Normal.Realtime.RealtimeComponent<CustomRealtimeAvatarVoiceModel> {
        public  float voiceVolume { get; private set; }

        private bool _mute = false;

        public bool mute {
            get => _mute;
            set => SetMute(value);
        }

        // If other components need access to the microphone data being transmitted
        public event Action<float[]> voiceData;

        private bool _hasMicrophone => _oculusMicrophoneDevice != null || _nativeMicrophoneDevice != null || _unityMicrophoneDevice != null;

        private Normal.Realtime.OculusMicrophoneDevice _oculusMicrophoneDevice;
        private Normal.Realtime.Native.Microphone      _nativeMicrophoneDevice;
        private Normal.Realtime.MicrophoneDevice       _unityMicrophoneDevice;
        private Normal.Realtime.AudioDeviceDataReader  _unityMicrophoneDeviceDataReader;
        private int                    _microphoneSampleRate;
        private int                    _microphoneChannels;
        private int                    _microphoneFrameSize;
        private float[]                _microphoneFrameData;
        private AudioInputStream       _microphoneStream;

        private AudioPreprocessor                 _audioPreprocessor;
        private Normal.Realtime.AudioPreprocessorPlaybackListener _audioPreprocessorPlaybackListener;
        private float _microphoneDbLevel = -42.0f;

        private Normal.Realtime.AudioOutput _audioOutput;

        /// <summary>
        /// True if we need to rebuild the audio stream because the model changed. This is cached as a bool so we can
        /// handle the audio stream connection during Update instead of the model change event handlers.
        /// </summary>
        private bool _rebuildAudioStream;

#if !UNITY_EDITOR && UNITY_ANDROID
        private Coroutine _microphonePermissionCheckTask;
#endif

        void Update() {
            // Reconnect the audio stream if the client or stream ID changed.
            if (_rebuildAudioStream) ConnectAudioStream();

            // Send microphone data if needed
            SendMicrophoneData();

            // Calculate voice volume level
            CalculateVoiceVolume();
        }

        void OnDestroy() {
            if (model != null) {
                model.clientIDDidChange -= ClientIDUpdated;
                model.streamIDDidChange -= StreamIDUpdated;
            }

            DisconnectAudioStream();

            // Remove audio output
            if (_audioOutput != null) {
                _audioOutput.Stop();
                Destroy(_audioOutput);
                _audioOutput = null;
            }
        }

        private void CalculateVoiceVolume() {
            float averageDbSample = -42.0f;

            if (_hasMicrophone) {
                averageDbSample = _microphoneDbLevel;
            } else if (_audioOutput != null) {
                averageDbSample = _audioOutput.dbLevel;
            }

            // These are arbitrary values I picked from my own testing.
            float volumeMinDb = -42.0f;
            float volumeMaxDb = -5.0f;
            float volumeRange = volumeMaxDb - volumeMinDb;

            float normalizedVolume = (averageDbSample - volumeMinDb) / volumeRange;
            if (normalizedVolume < 0.0f)
                normalizedVolume = 0.0f;
            if (normalizedVolume > 1.0f)
                normalizedVolume = 1.0f;

            voiceVolume = normalizedVolume;
        }

        protected override void OnRealtimeModelReplaced(CustomRealtimeAvatarVoiceModel previousModel, CustomRealtimeAvatarVoiceModel currentModel) {
            if (previousModel != null) {
                previousModel.clientIDDidChange -= ClientIDUpdated;
                previousModel.streamIDDidChange -= StreamIDUpdated;
            }

            if (currentModel != null) {
                currentModel.clientIDDidChange += ClientIDUpdated;
                currentModel.streamIDDidChange += StreamIDUpdated;
            }

            _rebuildAudioStream = true;
        }

        private void ClientIDUpdated(CustomRealtimeAvatarVoiceModel model, int clientID) {
            if (isOwnedLocallyInHierarchy) return;
            _rebuildAudioStream = true;
        }

        private void StreamIDUpdated(CustomRealtimeAvatarVoiceModel model, int streamID) {
            if (isOwnedLocallyInHierarchy) return;
            _rebuildAudioStream = true;
        }

#region Connect Audio Stream

        private void ConnectAudioStream() {
            DisconnectAudioStream();

            if (model == null) return;

            if (isOwnedLocallyInHierarchy) {
                RequestMicrophonePermissionAndConnectLocalAudioStream();
            } else {
                ConnectRemoteAudioStream();
            }

            _rebuildAudioStream = false; // prevent rebuilding the audio stream until the model changes
        }

        private void RequestMicrophonePermissionAndConnectLocalAudioStream() {
#if !UNITY_EDITOR && UNITY_ANDROID
            // Check for microphone permission

            // If there's already a task polling for microphone permission, let that one run so we don't display multiple permissions dialogs.
            if (_microphonePermissionCheckTask != null)
                return;

            _microphonePermissionCheckTask = StartCoroutine(RequestMicrophonePermissionAndConnectLocalAudioStreamTask());
#else
            // No permission needed, connect the local microphone stream.
            ConnectLocalAudioStream();
#endif
        }

#if !UNITY_EDITOR && UNITY_ANDROID
        private IEnumerator RequestMicrophonePermissionAndConnectLocalAudioStreamTask() {
            // Check if we have permission to use the microphone
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone)) {
                Debug.Log("RealtimeAvatarVoice: User has not granted microphone permissions. Will request.");
                Permission.RequestUserPermission(Permission.Microphone);

                // Yield until we get permission or 20 seconds pass
                float startTime = Time.realtimeSinceStartup;
                while (true) {
                    // Note: Android technically pauses Unity, which prevents rescheduling after this await while the dialog is up.
                    //       The whole loop only runs once or twice, but there's no way to be certain the permission dialog will
                    //       pause the app before the first yield return null call, so I loop until we either get permission or
                    //       we run out of time.
                    yield return null;

                    // Break from the loop if we have permission.
                    if (Permission.HasUserAuthorizedPermission(Permission.Microphone)) {
                        Debug.Log("RealtimeAvatarVoice: User has granted microphone permissions!");
                        break;
                    }

                    // Bail if too much time has passed.
                    float elapsed = Time.realtimeSinceStartup - startTime;
                    if (elapsed > 20.0f) {
                        Debug.LogError("RealtimeAvatarVoice: Waited 20 seconds. User has not granted microphone permissions. Unable to start microphone device.");
                        yield break;
                    }
                }
            }

            // Connect the local audio stream
            ConnectLocalAudioStream();

            // Clear the task
            _microphonePermissionCheckTask = null;
        }
#endif

        private void ConnectLocalAudioStream() {
            // Local player, create microphone stream

            // First check if this platform supports our native microphone wrapper (lower latency + native echo cancellation if available)
            _microphoneSampleRate = 48000;
            _microphoneChannels   = 1;

            // Check for Oculus native microphone device API
            bool foundOculusMicrophoneDevice = false;
            if (Normal.Realtime.OculusMicrophoneDevice.IsOculusPlatformAvailable()) {
                foundOculusMicrophoneDevice = Normal.Realtime.OculusMicrophoneDevice.IsOculusPlatformInitialized();
                if (!foundOculusMicrophoneDevice && Application.platform == RuntimePlatform.Android)
                    Debug.LogWarning("Normcore: Oculus Platform SDK found, but it's not initialized. Oculus Quest native echo cancellation will be unavailable.");
            }

            if (foundOculusMicrophoneDevice) {
                // Create Oculus microphone device
                _oculusMicrophoneDevice = new Normal.Realtime.OculusMicrophoneDevice();
                _oculusMicrophoneDevice.Start();
                _microphoneSampleRate = 48000;
                _microphoneChannels   = 1;
            } else if (Normal.Realtime.Native.Microphone.PlatformSupported()) {
                _nativeMicrophoneDevice = new Normal.Realtime.Native.Microphone();

                // If we failed to connect to the local microphone, bail.
                if (!_nativeMicrophoneDevice.Start()) {
                    Debug.LogError("Failed to connect to default microphone device. Make sure it is plugged in and functioning properly.");
                    _nativeMicrophoneDevice.Dispose();
                    _nativeMicrophoneDevice = null;
                    return;
                }

                _microphoneSampleRate = _nativeMicrophoneDevice.SampleRate();
                _microphoneChannels   = _nativeMicrophoneDevice.Channels();
            } else {
                // Create a microphone device
                _unityMicrophoneDevice = Normal.Realtime.MicrophoneDevice.Start("");

                // If we failed to connect to the local microphone, bail.
                if (_unityMicrophoneDevice == null) {
                    Debug.LogError("Failed to connect to default microphone device. Make sure it is plugged in and functioning properly.");
                    return;
                }

                _unityMicrophoneDeviceDataReader = new Normal.Realtime.AudioDeviceDataReader(_unityMicrophoneDevice);
                _microphoneSampleRate = _unityMicrophoneDevice.sampleRate;
                _microphoneChannels   = _unityMicrophoneDevice.numberOfChannels;
            }

            // Compute frame size with the sample rate of the microphone we received
            _microphoneFrameSize = _microphoneSampleRate / 100;

            // Preallocate the microphone frame buffer used in SendMicrophoneData
            _microphoneFrameData = new float[_microphoneFrameSize];

            // Create microphone stream with this sample rate (stream will automatically resample to 48000 before encoding with OPUS)
            _microphoneStream = room.CreateAudioInputStream(true, _microphoneSampleRate, _microphoneChannels);

            // Audio Preprocessor
            // Create it for all platforms except iOS and visionOS, as they have nice built-in ones.
#if (UNITY_IOS || UNITY_VISIONOS) && !UNITY_EDITOR
            bool platformHasHardwarePreprocessor = true;
#else
            bool platformHasHardwarePreprocessor = false;
#endif

            if (platformHasHardwarePreprocessor == false) {
                bool   isMobilePlatform = Application.isMobilePlatform;
                string loadedDeviceName = XRSettings.loadedDeviceName.ToLower();

                // Typically "Oculus", "oculus display", "OpenXR Display", "wvr display provider" (HTC VIVE), depending on Unity / platform SDK version.
                bool   isXRPlatform     = loadedDeviceName.Contains("oculus") || loadedDeviceName.Contains("pico") || loadedDeviceName.Contains("xr") || loadedDeviceName.Contains("vr");

                // Turn on software echo cancellation for mobile platforms without native echo cancellation (aka non-iOS with the exception of Oculus/XR Android platforms)
                bool echoCancellation = isMobilePlatform && !isXRPlatform;
                _audioPreprocessor = new AudioPreprocessor(_microphoneSampleRate, _microphoneFrameSize, // Input stream
                    true,                                                                               // Automatic gain control
                    true,                                                                               // Noise suppression
                    true,                                                                               // Reverb suppression
                    echoCancellation, AudioSettings.outputSampleRate, 2, 0.28f);                        // Echo cancellation
                if (echoCancellation) {
                    // Find the audio listener in the scene so we can perform echo cancellation with it
                    AudioListener[] audioListeners = FindObjectsOfType<AudioListener>();
                    if (audioListeners.Length <= 0) {
                        Debug.LogWarning("RealtimeAvatarVoice: Unable to find any AudioListeners in the scene. RealtimeAvatarVoice will not be able to perform echo cancellation.");
                    } else {
                        AudioListener audioListener = audioListeners[0];
                        if (audioListeners.Length > 1)
                            Debug.LogWarning("RealtimeAvatarVoice: Multiple AudioListeners found in the scene. Performing echo cancellation with the first one: " + audioListener.gameObject.name);

                        _audioPreprocessorPlaybackListener = audioListener.gameObject.AddComponent<Normal.Realtime.AudioPreprocessorPlaybackListener>();
                        _audioPreprocessorPlaybackListener.audioPreprocessor = _audioPreprocessor;
                    }
                }
            }
        }

        private void ConnectRemoteAudioStream() {
            // Remote player, lookup audio stream and create audio output
            int clientID = model.clientID;
            int streamID = model.streamID;

            // Ignore invalid state model state
            if (clientID < 0 || streamID < 0) return;

            // Find AudioOutputStream
            AudioOutputStream audioOutputStream = room.GetAudioOutputStream(clientID, streamID);
            if (audioOutputStream != null) {
                _audioOutput = gameObject.AddComponentIfNeeded<Normal.Realtime.AudioOutput>(out _);
                _audioOutput.mute = mute;
                _audioOutput.StartWithAudioOutputStream(audioOutputStream);
            } else {
                Debug.LogError($"RealtimeAvatarVoice: Unable to find matching audio stream for avatar (clientID: {clientID}, streamID: {streamID}).");
            }
        }

#endregion

        private void DisconnectAudioStream() {
            if (_microphoneStream != null) {
                // Destroy AudioPreprocessorPlaybackListener
                if (_audioPreprocessorPlaybackListener != null) {
                    Destroy(_audioPreprocessorPlaybackListener);
                    _audioPreprocessorPlaybackListener = null;
                }

                // Dispose of audio preprocessor
                if (_audioPreprocessor != null) {
                    _audioPreprocessor.Dispose();
                    _audioPreprocessor = null;
                }

                // Close microphone stream
                _microphoneStream.Close();

                // Dispose microphone device
                if (_oculusMicrophoneDevice != null) {
                    _oculusMicrophoneDevice.Stop();
                    _oculusMicrophoneDevice.Dispose();
                    _oculusMicrophoneDevice = null;
                }
                if (_nativeMicrophoneDevice != null) {
                    _nativeMicrophoneDevice.Stop();
                    _nativeMicrophoneDevice.Dispose();
                    _nativeMicrophoneDevice = null;
                }
                if (_unityMicrophoneDevice != null) {
                    _unityMicrophoneDevice.Dispose();
                    _unityMicrophoneDevice = null;
                }

                // Clean up
                _unityMicrophoneDeviceDataReader = null;
                _microphoneStream = null;
            }
        }

        void SendMicrophoneData() {
            if (_microphoneStream == null)
                return;

            // Store the client ID / stream ID so remote clients can find the corresponding AudioOutputStream
            model.clientID = _microphoneStream.ClientID();
            model.streamID = _microphoneStream.StreamID();

            // Check if AudioInputStream is valid.
            if (model.clientID < 0 || model.streamID < 0)
                return;

            // Clear the previous microphone frame data
            Array.Clear(_microphoneFrameData, 0, _microphoneFrameData.Length);

            // Send audio data in _microphoneFrameSize chunks until we're out of microphone data to send
            bool didGetAudioData = false;
            while (GetMicrophoneAudioData(_microphoneFrameData)) {
                // If we have an _audioPreprocessor, preprocess the microphone data to remove noise and echo
                if (_audioPreprocessor != null)
                    _audioPreprocessor.ProcessRecordSamples(_microphoneFrameData);

                // Note that even when muted audio still needs to run through the audio processor to make sure echo cancellation works properly when mute is turned back off.
                if (_mute)
                    Array.Clear(_microphoneFrameData, 0, _microphoneFrameData.Length);

                // If anyone is interested in the mic data, pass it to them
                try {
                    voiceData?.Invoke(_microphoneFrameData);
                } catch (Exception exception) {
                    Debug.LogException(exception);
                }

                // Send out microphone data
                _microphoneStream.SendRawAudioData(_microphoneFrameData);

                didGetAudioData = true;
            }

            // Normcore queues audio packets locally so they can go out in a single packet. This sends them off.
            if (didGetAudioData) {
                _microphoneStream.SendQueuedMessages();
            }

            // If we got audio data, update the current microphone level.
            // Note: I moved this here so that we do our volume level calculations on microphone audio that has run through the AudioPreprocessor.
            if (didGetAudioData) {
                int firstFrame = _microphoneFrameData.Length - 256;
                if (firstFrame < 0)
                    firstFrame = 0;
                int firstSample = firstFrame * _microphoneChannels;
                _microphoneDbLevel = StaticFunctions.CalculateAverageDbForAudioBuffer(_microphoneFrameData, firstSample);
            }
        }

        private bool GetMicrophoneAudioData(float[] audioData) {
            if (_oculusMicrophoneDevice != null)
                return _oculusMicrophoneDevice.GetAudioData(audioData);
            else if (_nativeMicrophoneDevice != null)
                return _nativeMicrophoneDevice.GetAudioData(audioData);
            else if (_unityMicrophoneDeviceDataReader != null)
                return _unityMicrophoneDeviceDataReader.GetData(audioData);

            return false;
        }

        void SetMute(bool mute) {
            if (mute == _mute)
                return;

            if (_audioOutput != null)
                _audioOutput.mute = mute;

            _mute = mute;
        }
    }
#else
    // Normcore WebGL + Audio requires access to the WebGL Beta branch (help@normalvr.com)
    [DefaultExecutionOrder(-1)] // Make sure our Update() runs before the default to ensure _microphoneDbLevel has been calculated for CalculateVoiceVolume()
    public class RealtimeAvatarVoice : RealtimeComponent<RealtimeAvatarVoiceModel> {
        public  float voiceVolume { get; private set; }

        public bool mute {
            get => true;
            set { }
        }
    }
#endif
}
