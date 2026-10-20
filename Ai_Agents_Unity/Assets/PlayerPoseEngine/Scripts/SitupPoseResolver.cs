using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayerPoseEngine.Scripts
{
    public class SitupPoseResolver : PoseResolver
    {
        //Pose Reference
        public PlayerPose currentlyRequestedPose;
        public bool situpPoseFailed;
        public List<PlayerPose> availablePoses;
        
        //Wall Settings
        public float wallWidth = 2.5f;  
        public float wallHeight = 0.4f; 
        public float wallDepth = 1.5f; 
        
        //Block Settings
        public float lowBlockFloorHeight = 0.25f;

        private bool isHighBlock = false;
        private bool hasBeenHit = false;

        
        private Dictionary<string, PlayerPose> availablePosesDict = new Dictionary<string, PlayerPose>();

        public void Start()
        {
            if (availablePoses != null)
            {
                availablePosesDict = availablePoses.ToDictionary(x => x.name, x => x);
            }
        }

        void Update()
        {
            if (currentlyRequestedPose == null)
            {
                situpPoseFailed = false;
                hasBeenHit = false;
                timeAlive = 0;
            }
            timeAlive += Time.deltaTime;

            CheckPoseCollision();

            // Check if the player has failed or fulfilled the pose after a certain time
            if (timeAlive > timeToPlayer * 1.2f)
            {
                if (!situpPoseFailed)
                {
                    // Check if the player has hit the high block or not
                    if (isHighBlock && !hasBeenHit)
                    {
                        //Failed to hit the high block
                        if (onPoseFailed != null) onPoseFailed.Invoke(this, currentlyRequestedPose);
                    }
                    else
                    {
                        //Player has fulfilled the pose
                        if (onPoseFulfilled != null) onPoseFulfilled.Invoke(this, currentlyRequestedPose);
                    }
                }
                situpPoseFailed = false;
            }
        }

        public void CheckPoseCollision()
        {
            if (headObject == null || headVisObject == null || situpPoseFailed) return;

            Vector3 targetPos = headObject.transform.position;
            
            // Check if the head visualization object is colliding with the target position
            bool isColliding = Mathf.Abs(targetPos.x - headVisObject.transform.position.x) < (wallDepth / 2f) && 
                               Mathf.Abs(targetPos.y - headVisObject.transform.position.y) < (wallHeight / 2f);

            if (isColliding)
            {
                if (isHighBlock)
                {
                    
                    hasBeenHit = true;
                }
                else
                {
                    
                    if (onPoseFailed != null) onPoseFailed.Invoke(this, currentlyRequestedPose);
                    situpPoseFailed = true;
                }
            }
        }

        public override void RequestPose(PlayerPose pose)
        {
            situpPoseFailed = false;
            hasBeenHit = false;
            timeAlive = 0;
            currentlyRequestedPose = pose;
            VisualizePoseRequest(pose);
        }

        public void VisualizePoseRequest(PlayerPose pose)
        {
            if (pose == null || headVisObject == null) return;
            headVisObject.SetActive(true);

            Vector3 relPos = (pose.limbRequirements != null && pose.limbRequirements.Count > 0) 
                             ? pose.limbRequirements[0].relativePos 
                             : Vector3.zero;

            
            isHighBlock = relPos.y > 0.4f;

            float calculatedY = 0f;

            if (isHighBlock)
            {
                
                calculatedY = headObject != null ? headObject.transform.position.y : 0.8f;
            }
            else
            {
                
                calculatedY = lowBlockFloorHeight;
            }

            headVisObject.transform.localPosition = new Vector3(relPos.x, relPos.y, relPos.z);
            headVisObject.transform.localScale = new Vector3(wallWidth, wallHeight, wallDepth);
        }

        public override void CalcHeightFactor(float heightEstimate)
        {
            Debug.Log("Currently (and likely permanently) ignores height");
        }
    }
}