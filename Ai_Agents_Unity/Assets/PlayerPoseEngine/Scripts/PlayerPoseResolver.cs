using PlayerPoseEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.XR.OpenXR.Input;

namespace PlayerPoseEngine.Scripts {
    
    public class PlayerPoseResolver : MonoBehaviour
    {
        public PlayerPose currentlyRequestedPose;
        public Action<PlayerPose> onPlayerPoseFulfilled;
        public bool playerPoseFulfilled;
        public float poseHeldTime;
        public float playerSize;
        public List<PlayerPose> availablePoses;


        [Header("Player Objects")]

        public GameObject headObject;
        public GameObject lHandObject;
        public GameObject rHandObject;
        public GameObject lFootObject;
        public GameObject rFootObject;
       
        [Header("PoseVisualization")]
        //The root the pose is based on. If this is the player root, it will move with the player.
        public GameObject poseRoot;
        public GameObject lHandVisSphere;
        public GameObject rHandVisSphere;
        public GameObject lFootVisSphere;
        public GameObject rFootVisSphere;
        public GameObject headVisSphere;
        public bool visualizePose;
    
        Dictionary<string, PlayerPose> availablePosesDict;
        //The world matrix of the player core when the current pose was requested
        Matrix4x4 playerMatrixAtRequest;
        
    
    
        public void Start()
        {
            availablePosesDict = availablePoses.ToDictionary(x => x.name, x => x);
        }
    
        // Update is called once per frame
        void Update()
        {
            if(currentlyRequestedPose == null)
            {
                playerPoseFulfilled = false;
                poseHeldTime = 0;
            }
            bool poseFulfilled = visualizePose ? CheckAndVisualizePoseRequest(currentlyRequestedPose) : IsPoseRequestFulfilled(currentlyRequestedPose);
            if (poseFulfilled)
            {
                if (!playerPoseFulfilled && onPlayerPoseFulfilled != null)
                {
                    onPlayerPoseFulfilled.Invoke(currentlyRequestedPose);
                }
                playerPoseFulfilled = true;
                poseHeldTime += Time.deltaTime;           
            }
        }
    
        /// <summary>
        /// Requests the given pose from the player
        /// </summary>
        /// <param name="pose">The pose to be validated against</param>
        public void RequestPose(PlayerPose pose)
        {
            ValidatePoseData(pose);
            playerMatrixAtRequest = poseRoot.transform.localToWorldMatrix;
            currentlyRequestedPose = pose;
        }

        /// <summary>
        /// Requests the given pose from the player
        /// </summary>
        /// <param name="pose">The name of the pose to be validated against. Must be in the availablePoses dictionary to work.</param>
        public void RequestPose(string poseName)
        {
            if(!availablePosesDict.TryGetValue(poseName, out currentlyRequestedPose))
            {
                Debug.LogWarning("Requested Pose: " + poseName + " but it wasnt indexed in the available Poses.");
            } else
            {
                RequestPose(currentlyRequestedPose);
            }
        }
    
        /// <summary>
        /// Validates the data of the given pose and throws a Warning into log if the pose seems erroneous.
        /// </summary>
        /// <param name="pose"></param>
        public void ValidatePoseData(PlayerPose pose)
        {
            if(pose.limbRequirements.Select(x => x.limb).Distinct().Count() != pose.limbRequirements.Count())
            {
                Debug.LogWarning("Pose: " + pose.name + "contains the same limb more than once!");
            }
            if(pose.limbRequirements.Any(x => x.relativePos.magnitude > 2.5))
            {
                Debug.LogWarning("Pose: " + pose.name + "requests position more than 2.5m away from root, this is unlikely to be possible.");
            }
        }
    
        /// <summary>
        /// Returns whether the given pose is fulfilled, without triggering or updating visualization
        /// </summary>
        /// <param name="pose"></param>
        /// <returns></returns>
        public bool IsPoseRequestFulfilled(PlayerPose pose)
        {
            GameObject comparisonObject;
            bool isFulfilled = true;
            foreach (LimbRequirement limbReq in pose.limbRequirements)
            {
                comparisonObject = null;
                switch (limbReq.limb)
                {
                    case Limb.HEAD: comparisonObject = headObject; break;
                    case Limb.RHAND: comparisonObject = rHandObject; break;
                    case Limb.LHAND: comparisonObject = lHandObject; break;
                    case Limb.RFOOT: comparisonObject = rFootObject; break;
                    case Limb.LFOOT: comparisonObject = lFootObject; break;
                    default: Debug.LogError("Invalid Limb in LimbRequirement"); return false;
                }
                Vector3 adjustedPos = CalculateAdjustedPositioning(poseRoot.transform, pose, limbReq);
                isFulfilled = isFulfilled && (Vector3.Distance(comparisonObject.transform.position, adjustedPos) < limbReq.tolerance);
            }
            return isFulfilled;
        }
    
        /// <summary>
        /// Returns whether the given pose is fulfilled, and updates/starts visualization
        /// </summary>
        /// <param name="pose"></param>
        /// <returns></returns>
        public bool CheckAndVisualizePoseRequest(PlayerPose pose)
        {
            GameObject comparisonObject;
            GameObject visualizationSphere;
            bool isFulfilled = true;
            lHandVisSphere.SetActive(false);
            rHandVisSphere.SetActive(false);
            lFootVisSphere.SetActive(false);
            rFootVisSphere.SetActive(false);
            headVisSphere.SetActive(false);
            foreach (LimbRequirement limbReq in pose.limbRequirements)
            {
                comparisonObject = null;
                visualizationSphere = null;
                switch (limbReq.limb)
                {
                    case Limb.HEAD:
                        comparisonObject = headObject;
                        visualizationSphere = headVisSphere;
                        break;
                    case Limb.RHAND:
                        comparisonObject = rHandObject;
                        visualizationSphere = rHandVisSphere; 
                        break;
                    case Limb.LHAND:
                        comparisonObject = lHandObject;
                        visualizationSphere = lHandVisSphere; 
                        break;
                    case Limb.RFOOT:
                        comparisonObject = rFootObject;
                        visualizationSphere = rFootVisSphere; 
                        break;
                    case Limb.LFOOT:
                        comparisonObject = lFootObject;
                        visualizationSphere = lFootVisSphere; 
                        break;
                    default: Debug.LogError("Invalid Limb in LimbRequirement"); return false;
                }
                Vector3 adjustedPos = CalculateAdjustedPositioning(poseRoot.transform, pose, limbReq);
                visualizationSphere.SetActive(true);
                visualizationSphere.transform.position = adjustedPos;
                visualizationSphere.transform.localScale = new Vector3(limbReq.tolerance, limbReq.tolerance, limbReq.tolerance);
                bool isFulFilledIndividual = (Vector3.Distance(comparisonObject.transform.position, adjustedPos) < limbReq.tolerance);
                isFulfilled = isFulfilled && isFulFilledIndividual;              
                visualizationSphere.GetComponent<PoseVisual>().ToggleFulfilled(isFulFilledIndividual);
            }
            return isFulfilled;
        }


        /// <summary>
        /// Calculates the limbs requested world position, based on the players center and the limb positioning mode in the pose.
        /// </summary>
        /// <param name="playerRoot">The player center used for assessing repositioning, only relevant in PositioningMode.PLAYER_CORE</param>
        /// <param name="p">The requested pose</param>
        /// <param name="lr">The limb requirement</param>
        /// <returns></returns>
        public Vector3 CalculateAdjustedPositioning(Transform playerRoot, PlayerPose p, LimbRequirement lr)
        {
            switch (p.positioningMode)
            {
                case PositioningMode.WORLD: return lr.relativePos; 
                case PositioningMode.PLAYER_CENTER_AT_REQUEST: return playerMatrixAtRequest.MultiplyPoint3x4(lr.relativePos);
                case PositioningMode.PLAYER_CORE: return playerRoot.transform.localToWorldMatrix.MultiplyPoint3x4(lr.relativePos); 
            }
            return Vector3.zero;
        }
        
    }
    
}
