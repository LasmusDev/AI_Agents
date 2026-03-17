using UnityEngine;
using System.Collections.Generic;
using PlayerPoseEngine.Scripts;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SquatGame
{
    public class SquatBeatGenerator : MonoBehaviour
    {
        [Header("Required References")]
        [Tooltip("The audio track to be analyzed")]
        public AudioClip audioTrack;
        
        [Tooltip("The pose map that will be automatically filled")]
        public SquatPoseMap targetPoseMap;

        [Tooltip("The default wall prefab set for each block")]
        public GameObject defaultWallPrefab;

        [Tooltip("A list of poses. The generator randomly picks one for each beat!")]
        public List<PlayerPose> randomPosesToPick;

        [Header("Beat Detection Settings")]
        [Tooltip("How sensitive is the detector? (Smaller value = more blocks)")]
        public float sensitivity = 1.8f;
        
        [Tooltip("How many seconds must pass at minimum between two walls?")]
        public float minTimeBetweenBlocks = 1.5f;

        // A small button in the inspector (Right-click on the script -> Generate Beat Map)
        [ContextMenu("GENERATE BEAT MAP")]
        public void GenerateMap()
        {
            if (audioTrack == null || targetPoseMap == null)
            {
                Debug.LogError("ERROR: Please assign both an audio track and a target pose map before generating!");
                return;
            }

            // create a new list for steps 
            targetPoseMap.steps = new List<SquatPoseMapStep>();

            // Check for poses once 
            bool hasPoses = randomPosesToPick != null && randomPosesToPick.Count > 0;
            if (!hasPoses)
            {
                Debug.LogWarning("No poses assigned in the list! The generator will create blocks without pose requirements.");
            }

            // Read audio data
            float[] samples = new float[audioTrack.samples * audioTrack.channels];
            audioTrack.GetData(samples, 0);

            // Variables for beat detection
            int windowSize = 1024; 
            List<float> energyHistory = new List<float>(); 
            float lastBeatTime = -minTimeBetweenBlocks;
            int historySize = 43; 

            // Store the sum of the history 
            float runningEnergySum = 0f;

            // Analyze audio data in windows
            for (int i = 0; i < samples.Length; i += windowSize)
            {
                // Calculate current energy in this window
                float currentEnergy = 0;
                for (int j = 0; j < windowSize && i + j < samples.Length; j++)
                {
                    currentEnergy += Mathf.Abs(samples[i + j]);
                }
                currentEnergy /= windowSize;

                // Calculate average energy from history
                float averageEnergy = 0;
                if (energyHistory.Count > 0)
                {
                    averageEnergy = runningEnergySum / energyHistory.Count;
                }

                // Calculate current time in seconds
                float currentTime = (float)i / (audioTrack.frequency * audioTrack.channels);

                // Beat detection
                if (currentEnergy > (averageEnergy * sensitivity) && (currentTime - lastBeatTime) > minTimeBetweenBlocks)
                {
                    SquatPoseMapStep newStep = new SquatPoseMapStep();
                    newStep.spawnTime = currentTime;
                    newStep.wallPrefab = defaultWallPrefab;

                    // Assign random pose (if available)
                    if (hasPoses)
                    {
                        int randomIndex = Random.Range(0, randomPosesToPick.Count);
                        newStep.poseAsset = randomPosesToPick[randomIndex];
                    }

                    targetPoseMap.steps.Add(newStep);
                    lastBeatTime = currentTime; 
                }

                // Update history (and adjust the running sum)
                energyHistory.Add(currentEnergy);
                runningEnergySum += currentEnergy;

                if (energyHistory.Count > historySize)
                {
                    float removedEnergy = energyHistory[0];
                    energyHistory.RemoveAt(0);
                    runningEnergySum -= removedEnergy; // Subtract the removed value from the sum
                }
            }

            Debug.Log($"Finished! {targetPoseMap.steps.Count} blocks were added to the map according to the beat!");

            // Save changes to the asset
#if UNITY_EDITOR
            EditorUtility.SetDirty(targetPoseMap);
            AssetDatabase.SaveAssets();
#endif
        }
    }
}