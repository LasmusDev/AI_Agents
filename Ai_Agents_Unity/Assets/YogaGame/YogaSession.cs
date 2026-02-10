using UnityEngine;
using System.Collections.Generic;
using PlayerPoseEngine.Scripts; // Gets changed due to avatar replacement

namespace YogaGame
{
    [System.Serializable]
    public class YogaStep
    {
        public string stepName;     // Name, e.g. "Krieger 1"
        public PlayerPose pose;     //Gtes changed due to avatar replacement
        public float holdDuration;  // how long must it be held? (seconds)
        [TextArea] public string instruction; // description/instructions
    }

    [CreateAssetMenu(fileName = "NewYogaSession", menuName = "Yoga/Session")]
    public class YogaSession : ScriptableObject
    {
        public string sessionName;
        public List<YogaStep> steps;
    }
}