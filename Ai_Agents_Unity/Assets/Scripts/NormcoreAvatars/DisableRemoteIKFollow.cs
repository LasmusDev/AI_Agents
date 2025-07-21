using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NormcoreAvatars{
    
    public class DisableRemoteIKFollow : MonoBehaviour
    {
        CustomRealtimeAvatarManager manager;

        public void Awake()
        {
            manager = GetComponent<CustomRealtimeAvatarManager>();
            manager.avatarCreated += DisableIKFollows;
        }

        private void DisableIKFollows(CustomRealtimeAvatarManager avatarManager, CustomRealtimeAvatar avatar, bool isLocalAvatar)
        {
            if (!isLocalAvatar)
            {
                List<IK_Feet> feetComponents = avatar.GetComponentsInChildren<IK_Feet>(true).ToList();
                feetComponents.ForEach(x => x.enabled = false);
                List<FollowObject> follows = avatar.GetComponentsInChildren<FollowObject>(true).ToList();
                follows.ForEach(x => x.enabled = false);
                avatar.GetComponent<CustomRealtimeAvatar>().enabled = false;
            }
        }
    }
    
}
