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

        [ContextMenu("Add Loop")]
        public void LoopArray()
        {
            BeatToPose[] copy = new BeatToPose[poses.Length * 2];
            Array.Copy(poses, copy, poses.Length);
            float lastPoseBeat = poses[poses.Length - 1].beat;
            for(int i = 0; i < poses.Length; i++)
            {
                copy[poses.Length+ i].pose = poses[i].pose;
                copy[poses.Length + i].beat = poses[i].beat + lastPoseBeat;
            }
            poses = copy;
        }
    }
    
    [Serializable]
    public struct BeatToPose
    {
        public float beat; 
        public PlayerPose pose;
    }
}