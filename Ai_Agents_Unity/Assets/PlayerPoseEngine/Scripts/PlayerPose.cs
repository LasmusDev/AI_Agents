using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerPoseEngine.Scripts {
    
    public class PlayerPose : ScriptableObject
    {
        public List<LimbRequirement> limbRequirements;    
        public float intendedHeight;
    
        public void Init()
        {
            limbRequirements = new List<LimbRequirement>();
        }
    }
    
    [Serializable]
    public class LimbRequirement
    {
        /// <summary>
        /// How precise the user has to fit the pose. 
        /// </summary>
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
