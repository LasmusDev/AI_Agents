using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayerPoseEngine.Scripts{
    
    public class SquatsPoseResolver : PoseResolver
    {


        
        public PlayerPose currentlyRequestedPose;
        public bool squatPoseFailed;
        public List<PlayerPose> availablePoses;
        public Vector3 startingPoint;
        public float hitBoxDepth = 0.5f;
        public float hitBoxHeight = 0.5f;

        public bool visualizePose;
    
        Dictionary<string, PlayerPose> availablePosesDict;
        
        public void Start()
        {
            if (availablePoses != null)
            {
                availablePosesDict = availablePoses.ToDictionary(x => x.name, x => x);
            }
        }
    
        void Update()
        {
            if(currentlyRequestedPose == null)
            {
                squatPoseFailed = false;
                timeAlive = 0;
            }
            timeAlive += Time.deltaTime;

            CheckPoseFailure();
            if (squatPoseFailed)
            {
                return;
            }


            if(timeAlive > timeToPlayer * 2)
            {
                if (!squatPoseFailed && onPoseFailed != null)
                {
                    //If this goes past the playing without failing, we consider it a success
                    onPoseFulfilled.Invoke(this, currentlyRequestedPose);
                }
                squatPoseFailed = false;
            }
        }

        public void CheckPoseFailure()
        {
            Vector3 targetPos = headObject.transform.position;
            if (Mathf.Abs(targetPos.x - this.transform.position.x) > hitBoxDepth && Mathf.Abs(targetPos.y - this.transform.position.y) > hitBoxHeight)
            {
                if (!squatPoseFailed && onPoseFailed != null)
                {
                    onPoseFailed.Invoke(this, currentlyRequestedPose);
                }
                squatPoseFailed = true;
            }
        }
    
        public override void RequestPose(PlayerPose pose)
        {
            
            squatPoseFailed = false;
            timeAlive = 0;
            currentlyRequestedPose = pose;
        }

        public void RequestPose(string poseName)
        {
            if(!availablePosesDict.TryGetValue(poseName, out currentlyRequestedPose))
            {
                Debug.LogWarning("Requested Pose: " + poseName + " not found.");
            } else
            {
                RequestPose(currentlyRequestedPose);
            }
        }
    }
} 




