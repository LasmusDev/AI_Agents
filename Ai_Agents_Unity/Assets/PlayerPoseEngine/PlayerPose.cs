using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPose : ScriptableObject
{
    /// <summary>
    /// How precise the user has to fit the pose, and whether this is global or defined by each limb
    /// </summary>
    public bool enforceTolerance;
    public float tolerance;
    public List<LimbRequirement> limbRequirements;    
    public PositioningMode positioningMode;
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
   WORLD, //Requires the player to stay in a precise world position (still scaled based on player height)
   PLAYER_CENTER_AT_REQUEST, //Positioning is set around the player center when pose is requested
   PLAYER_CORE //Positioning is relative to the players center, and moves if the player moves
}
