using ExtensionMethods;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerPoseEngine.Scripts
{

    public class PlayerPose : ScriptableObject
    {
        public List<LimbRequirement> limbRequirements;
        public float intendedHeight;
        public float recordedLookAngleY;
        public void Init()
        {
            limbRequirements = new List<LimbRequirement>();
        }
        [ContextMenu("Correct Rotation")]
        public void CorrectRotation()
        {
            foreach (LimbRequirement lr in limbRequirements)
            {
                lr.relativePos = lr.relativePos.Rotated(new Vector3(0, recordedLookAngleY, 0), Vector3.up);
            }
            recordedLookAngleY = 0;
        }
    }

   


    [Serializable]
    public class LimbRequirement
    {

        //How precise the user has to fit the pose. 
        public Limb limb;
        public float tolerance;
        public Quaternion relativeRot;
        public Vector3 relativePos;
    }

    public enum Limb
    {
        HEAD, RHAND, LHAND, RFOOT, LFOOT
    }

    public enum PositioningMode
    {
        WORLD,
        PLAYER_CENTER_AT_REQUEST,
        PLAYER_CORE
    }

}
