using NUnit.Framework;
using System.Collections.Generic;
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
                List<IK_Feet> feetComponents = avatar.GetComponentsInChildren<IK_Feet>(true).ToList();
                feetComponents.ForEach(x => x.enabled = false);
                List<FollowObject> follows = avatar.GetComponentsInChildren<FollowObject>(true).ToList();
                follows.ForEach(x => x.enabled = false);
            }
        }
    }
    
}
