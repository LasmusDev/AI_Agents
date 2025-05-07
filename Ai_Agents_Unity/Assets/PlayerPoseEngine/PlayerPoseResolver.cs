using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class PlayerPoseResolver : MonoBehaviour
{
    public PlayerPose currentlyRequestedPose;
    public Action<PlayerPose> onPlayerPoseFulfilled;
    public bool playerPoseFulfilled;
    public float poseHeldTime;
    public float playerSize;

    public GameObject headObject;
    public GameObject lHandObject;
    public GameObject rHandObject;
    public GameObject lFootObject;
    public GameObject rFootObject;
    public List<PlayerPose> availablePoses;

    Dictionary<string, PlayerPose> availablePosesDict;
    Matrix4x4 playerMatrixRequestTime;
    GameObject playerCenter;
    


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
        if (IsPoseRequestFulfilled(currentlyRequestedPose))
        {
            if (!playerPoseFulfilled && onPlayerPoseFulfilled != null)
            {
                onPlayerPoseFulfilled.Invoke(currentlyRequestedPose);
            }
            playerPoseFulfilled = true;
            poseHeldTime += Time.deltaTime;           
        }

    }

    public void RequestPose(PlayerPose pose)
    {
        ValidatePoseData(pose);
        playerMatrixRequestTime = CalculatePlayerCenter().localToWorldMatrix;
        currentlyRequestedPose = pose;
    }

    public void RequestPose(string poseName)
    {
        if(!availablePosesDict.TryGetValue(poseName, out currentlyRequestedPose))
        {
            Debug.LogWarning("Requested Pose: " + poseName + " but it wasnt indexed in the available Poses.");
        } else
        {
            ValidatePoseData(currentlyRequestedPose);
            playerMatrixRequestTime = CalculatePlayerCenter().localToWorldMatrix;
        }
    }

    public Transform CalculatePlayerCenter()
    {
        if(playerCenter == null)
        {
            playerCenter = new GameObject();
        }
        Vector3 pos = Vector3.zero;
        if(headObject != null)
        {
            pos = headObject.transform.position;
            if (lHandObject != null && rHandObject != null)
            {
                Vector3 betweenHands = Vector3.Lerp(lHandObject.transform.position, rHandObject.transform.position, 0.5f);
                pos = Vector3.Lerp(betweenHands, pos, 0.5f);
            }
            playerCenter.transform.position = pos;
            playerCenter.transform.LookAt(headObject.transform.position);
        }
        return playerCenter.transform;
    }

    public void ValidatePoseData(PlayerPose pose)
    {
        if(pose.limbRequirements.Select(x => x.limb).Distinct().Count() != pose.limbRequirements.Count())
        {
            Debug.LogWarning("Pose: " + pose.name + "contains the same limb more than once!");
        }
    }

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
            CalculatePlayerCenter();
            Vector3 adjustedPos = CalculateAdjustedPositioning(playerCenter.transform, pose, limbReq);
            isFulfilled = isFulfilled && (Vector3.Distance(comparisonObject.transform.position, adjustedPos) < limbReq.tolerance);
        }
        return isFulfilled;
    }

    public Vector3 CalculateAdjustedPositioning(Transform playerCenter, PlayerPose p, LimbRequirement lr)
    {
        switch (p.positioningMode)
        {
            case PositioningMode.WORLD: return lr.relativePos; 
            case PositioningMode.PLAYER_CENTER_AT_REQUEST: return playerMatrixRequestTime.MultiplyPoint3x4(lr.relativePos);
            case PositioningMode.PLAYER_CORE: return playerCenter.transform.localToWorldMatrix.MultiplyPoint3x4(lr.relativePos); 
        }
        return Vector3.zero;
    }
    
}
