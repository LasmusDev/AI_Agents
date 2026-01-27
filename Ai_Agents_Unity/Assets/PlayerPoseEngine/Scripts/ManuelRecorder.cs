using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlayerPoseEngine.Scripts
{
    [RequireComponent(typeof(AudioSource))]
    public class ManuelRecorder : MonoBehaviour
    {
        [Header("Debug Controls")]
        [Tooltip("Click here to start recording without keyboard")]
        public bool startRecordingNow = false; 

        [Header("Settings")]
        public string saveFileName = "MyNewLevel"; 
        public float bpm = 120f; 

        [Header("Pose Mappings")]
        public List<KeyToPose> mappings;

        [Header("Status")]
        public bool isRecording = false;
        public int recordedCount = 0;

        private List<BeatToPose> recordedNotes = new List<BeatToPose>();
        private AudioSource audioSource;
        private float secPerBeat;

        [System.Serializable]
        public struct KeyToPose
        {
            public Key key; 
            public PlayerPose poseAsset; 
        }

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            
           
            if(audioSource.clip == null) 
            {
                Debug.LogError("WARNING: AudioSource has no AudioClip! Please assign a song.");
            }

            secPerBeat = 60f / bpm;
            recordedNotes.Clear();
        }

        void Update()
        {
            
            if (startRecordingNow)
            {
                startRecordingNow = false; 
                ToggleRecording();
            }

            
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    ToggleRecording();
                }

                if (isRecording)
                {
                    
                    float currentBeat = audioSource.time / secPerBeat;

             
                    foreach (var map in mappings)
                    {
                        if (map.key == Key.None) continue;

                        if (Keyboard.current[map.key].wasPressedThisFrame)
                        {
                            RecordHit(currentBeat, map.poseAsset);
                        }
                    }
                }
            }
        }

        void RecordHit(float beat, PlayerPose pose)
        {
            if (pose == null) return;

            BeatToPose newNote = new BeatToPose();
            newNote.beat = beat;
            newNote.pose = pose;

            recordedNotes.Add(newNote);
            recordedCount++;
            
            Debug.Log($"<color=cyan>Recorded:</color> {pose.name} at beat {beat:F2}");
        }

        void ToggleRecording()
        {
            if (audioSource.clip == null)
            {
                Debug.LogError("Cannot start: No song in the AudioSource!");
                return;
            }

            if (isRecording)
            {
                isRecording = false;
                audioSource.Pause();
                Debug.Log("Recording PAUSED.");
            }
            else
            {
                isRecording = true;
                if (!audioSource.isPlaying) audioSource.Play();
                Debug.Log("Recording RUNNING! Press keys!");
            }
        }

#if UNITY_EDITOR
        [ContextMenu("SAVE TO POSEMAP ASSET")]
        public void SaveToAsset()
        {
            if (recordedNotes.Count == 0)
            {
                Debug.LogWarning("Nothing recorded!");
                return;
            }

            Posemap newMap = ScriptableObject.CreateInstance<Posemap>();
            newMap.song = audioSource.clip;
            newMap.bpm = (int)bpm; 
            newMap.poses = recordedNotes.ToArray(); 

            string folderPath = "Assets/DancingGameLevels";
            string fullPath = folderPath + "/" + saveFileName + ".asset";
            

            AssetDatabase.CreateAsset(newMap, fullPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"<color=green>SUCCESS! Saved to: {fullPath}</color>");
            Selection.activeObject = newMap;
        }
#endif
    }
}