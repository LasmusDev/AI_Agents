using System.Collections.Generic;
using UnityEngine;

namespace DancingGame
{
    // EINE Pose im Song
    [System.Serializable]
    public class PoseNote
    {
        public float time;       // Wann muss die Pose gemacht werden?
        public string poseName;  // Name der Pose (z.B. "HandsUp", "T-Pose")
    }

    // Das KOMPLETTE Level
    [System.Serializable]
    public class DancingLevelData
    {
        public string songName;
        public List<PoseNote> notes = new List<PoseNote>();
    }
}