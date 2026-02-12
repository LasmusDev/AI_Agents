using UnityEngine;
using System.Collections.Generic;

namespace YogaGame
{
    [System.Serializable]
    public class YogaStep
    {
        public string stepName;     // Name, e.g. "Krieger 1"
        public GameObject posePrefab;     //The prefab to spawn for this step (with YogaPosePrefab script on it)
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