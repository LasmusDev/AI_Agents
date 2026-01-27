using UnityEngine;
using System;


namespace PlayerPoseEngine.Scripts {
    
    [CreateAssetMenu(fileName = "Posemap", menuName = "ScriptableObjects/Posemap", order = 1)]
    public class Posemap : ScriptableObject
    {
        public AudioClip song;
        
        [Tooltip("The song speed. IMPORTANT: Must be correct (e.g. 180 for Samurai).")]
        public float bpm; 
        
        public BeatToPose[] poses;

        
    }
    
    [Serializable]
    public struct BeatToPose
    {
        public float beat; 
        public PlayerPose pose;
    }
}