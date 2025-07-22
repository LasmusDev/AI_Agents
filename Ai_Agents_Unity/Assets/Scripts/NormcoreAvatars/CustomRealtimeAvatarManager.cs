using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Serialization;

namespace NormcoreAvatars
{
    [RequireComponent(typeof(Normal.Realtime.Realtime))]
    public class CustomRealtimeAvatarManager : MonoBehaviour {
#pragma warning disable 0649 // Disable variable is never assigned to warning.
        [FormerlySerializedAs("_avatarPrefab")]
        [SerializeField] private GameObject _localAvatarPrefab;

        [SerializeField] private CustomRealtimeAvatar.LocalPlayer _localPlayer;
#pragma warning restore 0649

        public GameObject localAvatarPrefab {
            get => _localAvatarPrefab;
            set => SetLocalAvatarPrefab(value);
        }

        public CustomRealtimeAvatar                  localAvatar { get; private set; }
        public Dictionary<int, CustomRealtimeAvatar> avatars     { get; private set; }

        public delegate void AvatarCreatedDestroyed(CustomRealtimeAvatarManager avatarManager, CustomRealtimeAvatar avatar, bool isLocalAvatar);
        public event AvatarCreatedDestroyed avatarCreated;
        public event AvatarCreatedDestroyed avatarDestroyed;

        private Normal.Realtime.Realtime _realtime;

        void Awake() {
            _realtime = GetComponent<Normal.Realtime.Realtime>();
            _realtime.didConnectToRoom += DidConnectToRoom;

            if (_localPlayer == null)
                _localPlayer = new CustomRealtimeAvatar.LocalPlayer();

            avatars = new Dictionary<int, CustomRealtimeAvatar>();
        }

        private void OnEnable() {
            // Create avatar if we're already connected
            if (_realtime.connected)
                CreateAvatarIfNeeded();
        }

        private void OnDisable() {
            // Destroy avatar if needed
            DestroyAvatarIfNeeded();
        }

        void OnDestroy() {
            _realtime.didConnectToRoom -= DidConnectToRoom;
        }

        void DidConnectToRoom(Normal.Realtime.Realtime room) {
            if (!gameObject.activeInHierarchy || !enabled)
                return;

            // Create avatar
            CreateAvatarIfNeeded();
        }

        public static CustomRealtimeAvatar.DeviceType GetRealtimeAvatarDeviceTypeForLocalPlayer() {
            switch (XRSettings.loadedDeviceName) {
                case "OpenVR":
                    return CustomRealtimeAvatar.DeviceType.OpenVR;
                case "Oculus":
                    return CustomRealtimeAvatar.DeviceType.Oculus;
                default:
                    return CustomRealtimeAvatar.DeviceType.Unknown;
            }
        }

        public void _RegisterAvatar(int clientID, CustomRealtimeAvatar avatar) {
            if (avatars.ContainsKey(clientID)) {
                Debug.LogError("RealtimeAvatar registered more than once for the same clientID (" + clientID + "). This is a bug!");
            }
            avatars[clientID] = avatar;
            //Disable follow objects as needed
            if (!avatar.isLocalAvatar)
            {
                avatar.gameObject.name = "Remote Avatar";
                /* List<IK_Feet> feetComponents = avatar.GetComponentsInChildren<IK_Feet>(true).ToList();
                feetComponents.ForEach(x => x.SetControlState(ControlState.REMOTE_COPY_TAKES_OVER, this));
                feetComponents.ForEach(x => x.enabled = false); */
                List<FollowObject> follows = avatar.GetComponentsInChildren<FollowObject>(true).ToList();
                follows.ForEach(x => x.SetControlState(ControlState.REMOTE_COPY_TAKES_OVER, this));
                follows.ForEach(x => x.enabled = false);
                //avatar.enabled = false; //The avatar caused weird issues at some point in the past.
            }
            else
            {
                avatar.gameObject.name = "Local Avatar";
            }
            
            // Fire event
            if (avatarCreated != null) {
                try {
                    avatarCreated(this, avatar, avatar.isLocalAvatar);
                } catch (System.Exception exception) {
                    Debug.LogException(exception);
                }
            }
        }

        public void _UnregisterAvatar(CustomRealtimeAvatar avatar) {
            // Removing the matching entry (if it still exists in the collection) by value
            List<KeyValuePair<int, CustomRealtimeAvatar>> matchingAvatars = avatars.Where(keyValuePair => keyValuePair.Value == avatar).ToList();
            foreach (KeyValuePair<int, CustomRealtimeAvatar> matchingAvatar in matchingAvatars) {
                int avatarClientID = matchingAvatar.Key;
                avatars.Remove(avatarClientID);
            }

            // Fire event
            if (avatarDestroyed != null) {
                try {
                    avatarDestroyed(this, avatar, avatar.isLocalAvatar);
                } catch (System.Exception exception) {
                    Debug.LogException(exception);
                }
            }
        }

        private void SetLocalAvatarPrefab(GameObject localAvatarPrefab) {
            if (localAvatarPrefab == _localAvatarPrefab)
                return;

            _localAvatarPrefab = localAvatarPrefab;

            // Replace the existing avatar if we've already instantiated the old prefab.
            if (localAvatar != null) {
                DestroyAvatarIfNeeded();
                CreateAvatarIfNeeded();
            }
        }

        public void CreateAvatarIfNeeded() {
            if (!_realtime.connected) {
                Debug.LogError("RealtimeAvatarManager: Unable to create avatar. Realtime is not connected to a room.");
                return;
            }

            if (localAvatar != null)
                return;

            if (_localAvatarPrefab == null) {
                Debug.LogWarning("Realtime Avatars local avatar prefab is null. No avatar prefab will be instantiated for the local player.");
                return;
            }

            GameObject avatarGameObject = Normal.Realtime.Realtime.Instantiate(_localAvatarPrefab.name, new Normal.Realtime.Realtime.InstantiateOptions {
                ownedByClient               = true,
                preventOwnershipTakeover    = true,
                destroyWhenOwnerLeaves      = true,
                destroyWhenLastClientLeaves = true,
                useInstance                 = _realtime,
            });

            if (avatarGameObject == null) {
                Debug.LogError("RealtimeAvatarManager: Failed to instantiate RealtimeAvatar prefab for the local player.");
                return;
            }

            localAvatar = avatarGameObject.GetComponent<CustomRealtimeAvatar>();
            if (localAvatar == null) {
                Debug.LogError("RealtimeAvatarManager: Successfully instantiated avatar prefab, but could not find the RealtimeAvatar component.");
                return;
            }

            localAvatar._InitializeLocalAvatar(_localPlayer);
            localAvatar.deviceType  = GetRealtimeAvatarDeviceTypeForLocalPlayer();
#if !UNITY_2020_2_OR_NEWER
#pragma warning disable 0618
            // Unity deprecated this API in 2020.2 without a clear replacement.
            localAvatar.deviceModel = XRDevice.model;
#pragma warning restore 0618
#endif
        }

        public void DestroyAvatarIfNeeded() {
            if (localAvatar == null)
                return;

            Normal.Realtime.Realtime.Destroy(localAvatar.gameObject);

            localAvatar = null;
        }
    }
}
