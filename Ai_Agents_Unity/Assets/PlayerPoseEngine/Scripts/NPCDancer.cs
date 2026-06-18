using UnityEngine;
using PlayerPoseEngine.Scripts;

namespace DancingGame
{
    public class NPCDancer : MonoBehaviour
    {
        // Game setup
        public PosemapPlayer playerManager;
        public AudioSource playerAudio;
        
        // NPC IK target references
        public Transform npcRoot; 
        public Transform headTarget;
        public Transform lHandTarget;
        public Transform rHandTarget;
        public Transform lArmHint;
        public Transform rArmHint;

        // Helper variables for pose interpolation/Correction
        public float smoothTime = 0.2f; 
        public float anticipationBeats = 2f; 
        public bool swapHands = true;
        public bool invertXAxis = false;
        public Vector3 headBodyOffset = new Vector3(0, -0.6f, 0);

        // Limits for pose targets to keep the NPC movement natural
        public float minChestDistance = 0.25f; 
        public float maxArmReach = 0.65f;
        public float shoulderHeight = 0f;

        // Offset for elbow hint positions
        public Vector3 elbowHintOffset = new Vector3(0.5f, -0.5f, 0f);

        private Vector3 anchorPos;
        private Quaternion anchorRot;

        private Vector3 idleHeadPos, idleLHandPos, idleRHandPos;
        private Quaternion idleHeadRot; 
        
        private Vector3 currentHeadPos, currentLHandPos, currentRHandPos;
        private Quaternion currentHeadRot;
        
        //Velocity variables for SmoothDamp
        private Vector3 headVel, lHandVel, rHandVel, rootVel, lHintVel, rHintVel;
        //For dance movement
        public float bounceAmount = 0.04f;
        public float swayAmount = 0.05f;
        private Vector3 currentDanceOffset;

        void Start()
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) Destroy(cc);

            anchorPos = npcRoot.position;
            anchorRot = npcRoot.rotation;

            if (headTarget != null) 
            {
                idleHeadPos = Quaternion.Inverse(anchorRot) * (headTarget.position - anchorPos);
                idleHeadRot = Quaternion.Inverse(anchorRot) * headTarget.rotation;
                currentHeadPos = headTarget.position;
                currentHeadRot = headTarget.rotation;
            }

            if (lHandTarget != null) 
            {
                idleLHandPos = Quaternion.Inverse(anchorRot) * (lHandTarget.position - anchorPos);
                currentLHandPos = lHandTarget.position;
            }
            
            if (rHandTarget != null) 
            {
                idleRHandPos = Quaternion.Inverse(anchorRot) * (rHandTarget.position - anchorPos);
                currentRHandPos = rHandTarget.position;
            }
        }

        void Update()
        {
            Vector3 targetHeadPos = idleHeadPos;
            Vector3 targetLHandPos = idleLHandPos;
            Vector3 targetRHandPos = idleRHandPos;
            Quaternion targetHeadRot = idleHeadRot;

            if (playerManager != null && playerAudio != null && playerAudio.isPlaying && playerManager.poseMap != null) 
            {
                float songPos = playerAudio.time;
                float secPerBeat = 60f / playerManager.BPM;
                float currentBeat = songPos / secPerBeat;
                //Avatar bounce effect based on the beat
                float bounce = -Mathf.Cos(currentBeat * Mathf.PI * 2f) * bounceAmount;
                float sway = Mathf.Sin(currentBeat * Mathf.PI) * swayAmount;
                currentDanceOffset = (anchorRot * Vector3.up * bounce) + (anchorRot * Vector3.right * sway);

                BeatToPose? nextPoseData = null;
                foreach (var bp in playerManager.poseMap.poses)
                {
                    if (bp.beat > currentBeat)
                    {
                        nextPoseData = bp;
                        break;
                    }
                }

                if (nextPoseData.HasValue && (nextPoseData.Value.beat - currentBeat) <= anticipationBeats)
                {
                    PlayerPose pose = nextPoseData.Value.pose;
                    foreach (var req in pose.limbRequirements)
                    {
                        Vector3 poseLocalPos = req.relativePos;

                        if (invertXAxis)
                        {
                          poseLocalPos.x *= -1f;  
                        } 

                        // keep hand targets in front of the chest to avoid unnatural poses
                        if (req.limb == Limb.LHAND || req.limb == Limb.RHAND)
                        {
                            if (poseLocalPos.z < minChestDistance) 
                            {
                                poseLocalPos.z = minChestDistance;
                            }
                        }

                        switch (req.limb)
                        {
                            case Limb.HEAD: targetHeadPos = poseLocalPos; break;
                            case Limb.LHAND: 
                            if (swapHands)
                                {
                                  targetRHandPos = poseLocalPos;   
                                }
                                else
                                {
                                  targetLHandPos = poseLocalPos;   
                                } 
                            break;
                            
                            case Limb.RHAND: 
                            if (swapHands)
                                {
                                  targetLHandPos = poseLocalPos;  
                                }
                                else
                                {
                                  targetRHandPos = poseLocalPos;   
                                } 
                            break;
                        }
                    }
                }
            }
            else
                {
                    currentDanceOffset = Vector3.Lerp(currentDanceOffset, Vector3.zero, Time.deltaTime * 5f);
                }
            

            UpdateHeadTarget(ref currentHeadPos, ref currentHeadRot, ref headVel, headTarget, targetHeadPos, targetHeadRot);
            UpdateHandTarget(ref currentLHandPos, ref lHandVel, lHandTarget, targetLHandPos);
            UpdateHandTarget(ref currentRHandPos, ref rHandVel, rHandTarget, targetRHandPos);
            
            UpdateBodyPosition();
            UpdateElbowHints();
        }

        void UpdateHeadTarget(ref Vector3 currentPos, ref Quaternion currentRot, ref Vector3 velocity, Transform ikTarget, Vector3 localTargetPos, Quaternion localTargetRot)
        {
            if (ikTarget == null) return;
            Vector3 worldTargetPos = anchorPos + currentDanceOffset + (anchorRot * localTargetPos);
            Quaternion worldTargetRot = anchorRot * localTargetRot;
            
            currentPos = Vector3.SmoothDamp(currentPos, worldTargetPos, ref velocity, smoothTime);
            currentRot = Quaternion.Slerp(currentRot, worldTargetRot, Time.deltaTime * (1f / smoothTime)); 

            ikTarget.position = currentPos;
            ikTarget.rotation = currentRot;
        }

        void UpdateHandTarget(ref Vector3 currentPos, ref Vector3 velocity, Transform ikTarget, Vector3 localTargetPos)
        {
            if (ikTarget == null) return;
            Vector3 worldTargetPos = anchorPos + currentDanceOffset + (anchorRot * localTargetPos);

            Vector3 shoulderPos = npcRoot.position + (npcRoot.up * shoulderHeight);
            Vector3 reachDir = worldTargetPos - shoulderPos;
            if (reachDir.magnitude > maxArmReach)
            {
                worldTargetPos = shoulderPos + reachDir.normalized * maxArmReach;
            }
            
            // for smooth movement Vector3.SmoothDamp
            currentPos = Vector3.SmoothDamp(currentPos, worldTargetPos, ref velocity, smoothTime);
            ikTarget.position = currentPos;
        }

        void UpdateBodyPosition()
        {
            if (headTarget != null && npcRoot != null)
            {
                Vector3 targetRootPos = currentHeadPos + headBodyOffset;
                targetRootPos.x = anchorPos.x + currentDanceOffset.x;
                targetRootPos.z = anchorPos.z + currentDanceOffset.z;
                npcRoot.position = Vector3.SmoothDamp(npcRoot.position, targetRootPos, ref rootVel, smoothTime);
            }
        }

        void UpdateElbowHints()
        {
            if (lArmHint != null)
            {
                Vector3 lHintPos = npcRoot.position + npcRoot.TransformDirection(new Vector3(-elbowHintOffset.x, elbowHintOffset.y, elbowHintOffset.z));
                lArmHint.position = Vector3.SmoothDamp(lArmHint.position, lHintPos, ref lHintVel, smoothTime);
            }

            if (rArmHint != null)
            {
                Vector3 rHintPos = npcRoot.position + npcRoot.TransformDirection(new Vector3(elbowHintOffset.x, elbowHintOffset.y, elbowHintOffset.z));
                rArmHint.position = Vector3.SmoothDamp(rArmHint.position, rHintPos, ref rHintVel, smoothTime);
            }
        }
    }
}