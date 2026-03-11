using UnityEngine;
using System.Collections.Generic;
using PlayerPoseEngine.Scripts; 

namespace SquatGame
{
    [System.Serializable]
    public class SquatPoseMapStep
    {
        [Tooltip("Your recorded pose (asset) from the PlayerPoseEngine")]
        public PlayerPose poseAsset; 
        
        [Tooltip("The wall prefab to spawn for this specific pose")]
        public GameObject wallPrefab;
        
        [Tooltip("When exactly should the wall spawn? (in seconds)")]
        public float spawnTime; 
    }

    [CreateAssetMenu(fileName = "NewSquatPoseMap", menuName = "SquatGame/PoseMap")]
    public class SquatPoseMap : ScriptableObject
    {
        public List<SquatPoseMapStep> steps;
    }
}