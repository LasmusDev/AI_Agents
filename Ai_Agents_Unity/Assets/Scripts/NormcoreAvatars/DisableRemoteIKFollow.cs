using System.Linq;
using UnityEngine;

namespace NormcoreAvatars {
    
    public class DisableRemoteIKFollow : MonoBehaviour
    {
        RealtimeAvatarManager manager;

        public void Awake()
        {
            manager = GetComponent<RealtimeAvatarManager>();
            manager.avatarCreated += DisableIKFollows;
        }

        private void DisableIKFollows(RealtimeAvatarManager avatarManager, RealtimeAvatar avatar, bool isLocalAvatar)
        {
            if (!isLocalAvatar)
            {
                avatar.GetComponentsInChildren<IK_Feet>().Select(x => x.enabled = false);
                avatar.GetComponentsInChildren<FollowObject>().Select(x => x.enabled = false);
            }
        }
    }
    
}
