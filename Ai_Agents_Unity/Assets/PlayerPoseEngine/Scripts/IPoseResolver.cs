using System;
using UnityEngine;

namespace PlayerPoseEngine.Scripts {
    
    public abstract class PoseResolver : MonoBehaviour
    {
        public Action<DancePoseResolver, PlayerPose> onPoseFulfilled;
        public Action<DancePoseResolver, PlayerPose> onPoseFailed;

        [Header("Player Objects")]
        public GameObject headObject;
        public GameObject lHandObject;
        public GameObject rHandObject;
        public GameObject lFootObject;
        public GameObject rFootObject;

        [Header("PoseVisualization")]
        public GameObject poseRoot;
        public GameObject lHandVisSphere;
        public GameObject rHandVisSphere;
        public GameObject lFootVisSphere;
        public GameObject rFootVisSphere;
        public GameObject headVisSphere;

        public float playerSize;

        //Also represents despawn time, which is twice this
        public float timeToPlayer;

        public float timeAlive = 0f;


        public virtual void RequestPose(PlayerPose pose)
        {

        }


    }

}
