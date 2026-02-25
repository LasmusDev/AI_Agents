using UnityEngine;
using System.Collections.Generic;
using PlayerPoseEngine.Scripts; 

namespace YogaGame
{
    [System.Serializable]
    public class YogaStep
    {
        public string stepName;     
        
        [Tooltip("The PlayerPose data for this step")]
        public PlayerPose poseData; 
        
        [Tooltip("The visual representation of the teacher for this step")]
        public GameObject teacherVisualPrefab; 
        
        public float holdDuration;  
        [TextArea] public string instruction; 
    }

    [CreateAssetMenu(fileName = "NewYogaSession", menuName = "Yoga/Session")]
    public class YogaSession : ScriptableObject
    {
        public string sessionName;
        public List<YogaStep> steps;
    }
}