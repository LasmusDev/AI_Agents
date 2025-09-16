using PlayerPoseEngine.Scripts;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Posemap", menuName = "ScriptableObjects/Posemap", order = 1)]
public class Posemap : ScriptableObject
{
    public AudioClip song;
    public BeatToPose[] poses;
    public int repeatOn = 10000;
    Dictionary<int, PlayerPose> posesInternal;

    


    public PlayerPose GetPose(int beat)
    {
        if (posesInternal == null || poses.Length != posesInternal.Count)
        {
            posesInternal = poses.ToDictionary(x => x.beat, x => x.pose);
        }
        if (posesInternal.ContainsKey(beat % repeatOn))
        {
            return posesInternal[beat % repeatOn];
        } else {  
            return null; 
        }
    }

}

[Serializable]
public struct BeatToPose
{
    public int beat;
    public PlayerPose pose;
}


