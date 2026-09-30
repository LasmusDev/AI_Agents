using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayerPoseEngine.Scripts{
    
    public class DancePoseResolver : PoseResolver
    {


        
        public PlayerPose currentlyRequestedPose;
        public bool dancePoseFulfilled;
        public float poseHeldTime;
        public List<PlayerPose> availablePoses;

        public PositioningMode positioningMode;


        public bool visualizePose;
    
        Dictionary<string, PlayerPose> availablePosesDict;
        Matrix4x4 playerMatrixAtRequest;
        
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
                dancePoseFulfilled = false;
                poseHeldTime = 0;
                timeAlive = 0;
            }         
            
            bool poseFulfilled = visualizePose ? CheckAndVisualizePoseRequest(currentlyRequestedPose) : IsPoseRequestFulfilled(currentlyRequestedPose);           
            if (poseFulfilled)
            {
                if (!dancePoseFulfilled && onPoseFulfilled != null)
                {
                    onPoseFulfilled.Invoke(this, currentlyRequestedPose);
                }
                dancePoseFulfilled = true;
                poseHeldTime += Time.deltaTime;           
            }

            if(timeAlive > timeToDespawn)
            {
                if (!dancePoseFulfilled && onPoseFailed != null)
                {
                    onPoseFailed.Invoke(this, currentlyRequestedPose);
                }
                poseHeldTime = 0;
                dancePoseFulfilled = false;
            }
        }
    
        public override void RequestPose(PlayerPose pose)
        {
            
            dancePoseFulfilled = false;
            poseHeldTime = 0;
            timeAlive = 0;
            playerMatrixAtRequest = poseRoot.transform.localToWorldMatrix;
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
    
        

        public bool IsPoseRequestFulfilled(PlayerPose pose)
        {
            if (pose == null) return false;
            bool isFulfilled = true;
            foreach (LimbRequirement limbReq in pose.limbRequirements)
            {
                GameObject comparisonObject = GetLimbObject(limbReq.limb);
                if (comparisonObject == null) return false;

                Vector3 adjustedPos = CalculateAdjustedPositioning(poseRoot.transform, pose, limbReq);
                isFulfilled &= (Vector3.Distance(comparisonObject.transform.position, adjustedPos) < limbReq.tolerance);
            }
            return isFulfilled;
        }

        public bool CheckAndVisualizePoseRequest(PlayerPose pose)
        {
            if (pose == null) return false;
            bool isFulfilled = true;
            
       
            if(lHandVisSphere) lHandVisSphere.SetActive(false);
            if(rHandVisSphere) rHandVisSphere.SetActive(false);
            if(lFootVisSphere) lFootVisSphere.SetActive(false);
            if(rFootVisSphere) rFootVisSphere.SetActive(false);
            if(headVisSphere) headVisSphere.SetActive(false);

            foreach (LimbRequirement limbReq in pose.limbRequirements)
            {
                GameObject comparisonObject = GetLimbObject(limbReq.limb);
                GameObject visualizationSphere = GetVisObject(limbReq.limb);
                
                if (visualizationSphere != null)
                {
                    Vector3 adjustedPos = CalculateAdjustedPositioning(poseRoot.transform, pose, limbReq);
                    visualizationSphere.SetActive(true);
                    visualizationSphere.transform.position = adjustedPos;
                    visualizationSphere.transform.localScale = Vector3.one * limbReq.tolerance;
                    
                    bool individualCheck = (Vector3.Distance(comparisonObject.transform.position, adjustedPos) < limbReq.tolerance);
                    isFulfilled &= individualCheck;              
                    
                    if(visualizationSphere.GetComponent<PoseVisual>())
                        visualizationSphere.GetComponent<PoseVisual>().ToggleFulfilled(individualCheck);
                }
            }
            return isFulfilled;
        }

      
        GameObject GetLimbObject(Limb limb) {
            switch(limb) {
                case Limb.HEAD: return headObject;
                case Limb.RHAND: return rHandObject;
                case Limb.LHAND: return lHandObject;
                case Limb.RFOOT: return rFootObject;
                case Limb.LFOOT: return lFootObject;
                default: return null;
            }
        }
        
        GameObject GetVisObject(Limb limb) {
            switch(limb) {
                case Limb.HEAD: return headVisSphere;
                case Limb.RHAND: return rHandVisSphere;
                case Limb.LHAND: return lHandVisSphere;
                case Limb.RFOOT: return rFootVisSphere;
                case Limb.LFOOT: return lFootVisSphere;
                default: return null;
            }
        }

        public Vector3 CalculateAdjustedPositioning(Transform playerRoot, PlayerPose p, LimbRequirement lr)
        {
            switch (positioningMode)
            {
                case PositioningMode.WORLD: return lr.relativePos; 
                case PositioningMode.PLAYER_CENTER_AT_REQUEST: return playerMatrixAtRequest.MultiplyPoint3x4(lr.relativePos);
                case PositioningMode.PLAYER_CORE: return playerRoot.transform.localToWorldMatrix.MultiplyPoint3x4(lr.relativePos); 
            }
            return Vector3.zero;
        }
    }
} 




