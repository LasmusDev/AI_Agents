/* using UnityEngine;
using Normal.Realtime;

namespace NormcoreAvatars {
    
    public class AvatarPrefabAssigner : MonoBehaviour
    {
        [Header("Avatar Prefabs")]
        [Tooltip("The avatar prefab to be used for players with an even client ID.")]
        public GameObject maleAvatarPrefab;
    
        [Tooltip("The avatar prefab to be used for players with an odd client ID.")]
        public GameObject femaleAvatarPrefab;
    
        private Realtime _realtime;
        private NormcoreAvatars.CustomRealtimeAvatarManager _realtimeAvatarManager;
    
        private void Awake()
        {
            // Get the Realtime and CustomRealtimeAvatarManager components on this GameObject.
            _realtime = GetComponent<Realtime>();
            _realtimeAvatarManager = GetComponent<NormcoreAvatars.CustomRealtimeAvatarManager>();
    
            if (_realtime == null || _realtimeAvatarManager == null)
            {
                Debug.LogError("AvatarPrefabAssigner: Realtime and/or RealtimeAvatarManager component not found on this GameObject. This script should be placed on the same GameObject as those components.");
                return;
            }
    
            // Subscribe to the didConnectToRoom event.
            _realtime.didConnectToRoom += DidConnectToRoom;
        }
    
        private void OnDestroy()
        {
            // Unsubscribe from the event to prevent memory leaks.
            if (_realtime != null)
            {
                _realtime.didConnectToRoom -= DidConnectToRoom;
            }
        }
    
        private void DidConnectToRoom(Realtime realtime)
        {
            // Check if the connected realtime instance is ours.
            if (realtime != _realtime) return;
    
            // Get the local client ID.
            int localClientID = _realtime.clientID;
    
            // Assign the avatar prefab based on whether the client ID is even or odd.
            if (localClientID % 2 == 0)
            {
                _realtimeAvatarManager.localAvatarPrefab = maleAvatarPrefab;
                Debug.Log($"Client {localClientID} is even. Assigning Male avatar.");
            }
            else
            {
                _realtimeAvatarManager.localAvatarPrefab = femaleAvatarPrefab;
                Debug.Log($"Client {localClientID} is odd. Assigning Female avatar.");
            }
        }
    }
} */

using UnityEngine;
using Normal.Realtime;

namespace NormcoreAvatars
{
    public class AvatarPrefabAssigner : MonoBehaviour
    {
        [Header("Avatar Prefabs (Resources Ordner!)")]
        [Tooltip("Prefab für gerade Client-IDs (0, 2, 4...)")]
        public GameObject maleAvatarPrefab;

        [Tooltip("Prefab für ungerade Client-IDs (1, 3, 5...)")]
        public GameObject femaleAvatarPrefab;

        private Realtime _realtime;
   
        private RealtimeAvatarManager _manager;

        private void Awake()
        {
            _realtime = GetComponent<Realtime>();
            _manager = GetComponent<RealtimeAvatarManager>();

            if (_manager != null)
            {
               
                _manager.enabled = false;
            }
            else
            {
                Debug.LogError("AvatarPrefabAssigner: RealtimeAvatarManager fehlt!");
            }
        }

        private void OnEnable()
        {
            if (_realtime != null) _realtime.didConnectToRoom += DidConnectToRoom;
        }

        private void OnDisable()
        {
            if (_realtime != null) _realtime.didConnectToRoom -= DidConnectToRoom;
        }

        private void DidConnectToRoom(Realtime realtime)
        {
           
            if (_manager == null) return;

           
            int clientID = realtime.clientID;
            GameObject selectedPrefab;

            if (clientID % 2 == 0)
            {
                selectedPrefab = maleAvatarPrefab;
                Debug.Log($"Client ID {clientID} ist gerade -> Setze Male Avatar.");
            }
            else
            {
                selectedPrefab = femaleAvatarPrefab;
                Debug.Log($"Client ID {clientID} ist ungerade -> Setze Female Avatar.");
            }

           
            _manager.localAvatarPrefab = selectedPrefab;

            
            _manager.enabled = true;
        }
    }
}