using UnityEngine;
using Normal.Realtime;

public class AvatarPrefabAssigner : MonoBehaviour
{
    [Header("Avatar Prefabs")]
    [Tooltip("The avatar prefab to be used for players with an even client ID.")]
    public GameObject maleAvatarPrefab;

    [Tooltip("The avatar prefab to be used for players with an odd client ID.")]
    public GameObject femaleAvatarPrefab;

    private Realtime _realtime;
    private RealtimeAvatarManager _realtimeAvatarManager;

    private void Awake()
    {
        // Get the Realtime and RealtimeAvatarManager components on this GameObject.
        _realtime = GetComponent<Realtime>();
        _realtimeAvatarManager = GetComponent<RealtimeAvatarManager>();

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