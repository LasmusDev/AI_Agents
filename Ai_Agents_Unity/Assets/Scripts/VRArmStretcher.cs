using UnityEngine;

namespace PlayerPoseEngine.Scripts
{
    [DefaultExecutionOrder(1000)] 
    public class VRArmStretcher : MonoBehaviour
    {
        //References to the bones of the arm
        public Transform shoulderBone; 
        public Transform elbowBone;    
        public Transform handBone;     
        
        //Reference to the IK Target
        public Transform ikTarget;

        //Helper variable to determine if this is the left or right arm (for automatic bone finding)
        public bool isLeftArm = true;
        //Default local positions of the elbow and hand bones
        private Vector3 defaultElbowLocalPos;
        private Vector3 defaultHandLocalPos;
        private float originalArmLength;

        void Start()
        {
            
            //If the IK target is not assigned, we use the transform of this script as the target
            if (ikTarget == null) ikTarget = this.transform;

            //Search for the bones in the avatar's hierarchy if they are not assigned
            Transform avatarRoot = this.transform.root;

            if (shoulderBone == null)
                shoulderBone = avatarRoot.FindRecursive(isLeftArm ? "l_upperarm" : "r_upperarm");
            
            if (elbowBone == null)
                elbowBone = avatarRoot.FindRecursive(isLeftArm ? "l_forearm" : "r_forearm");
            
            if (handBone == null)
                handBone = avatarRoot.FindRecursive(isLeftArm ? "l_hand" : "r_hand");
            // If any of the bones are still null, we log a warning and exit
            if (shoulderBone == null || elbowBone == null || handBone == null)
            {
                Debug.LogWarning("VRArmStretcher: Bones not assigned and could not be found automatically.");
                return;
            }

            defaultElbowLocalPos = elbowBone.localPosition;
            defaultHandLocalPos = handBone.localPosition;

            float upperLength = Vector3.Distance(shoulderBone.position, elbowBone.position);
            float lowerLength = Vector3.Distance(elbowBone.position, handBone.position);
            originalArmLength = upperLength + lowerLength;
        }
        // In LateUpdate, we adjust the positions of the elbow and hand bones based on the distance to the IK target
        void LateUpdate()
        {
            if (ikTarget == null || shoulderBone == null || elbowBone == null || handBone == null) return;

            float targetDistance = Vector3.Distance(shoulderBone.position, ikTarget.position);

            if (targetDistance > originalArmLength)
            {
                float stretchFactor = targetDistance / originalArmLength;

                elbowBone.localPosition = defaultElbowLocalPos * stretchFactor;
                handBone.localPosition = defaultHandLocalPos * stretchFactor;
            }
            else
            {
                elbowBone.localPosition = defaultElbowLocalPos;
                handBone.localPosition = defaultHandLocalPos;
            }
        }
    }
}