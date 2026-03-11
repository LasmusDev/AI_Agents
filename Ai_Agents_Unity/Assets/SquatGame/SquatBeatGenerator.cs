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
        [Tooltip("How sensitive is the detector? (Smaller value = more blocks, approximately 1.5 to 2.5 is good)")]
        public float sensitivity = 1.8f;
        
        [Tooltip("How many seconds must pass at minimum between two walls? (To have time for the squat)")]
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

            // Clear existing steps in the target pose map to avoid appending to old data
            targetPoseMap.steps.Clear();

            // check audio track settings
            float[] samples = new float[audioTrack.samples * audioTrack.channels];
            audioTrack.GetData(samples, 0);

            // Variables for beat detection
            int windowSize = 1024; // small window for energy calculation
            List<float> energyHistory = new List<float>(); // history of energy values to calculate the average
            float lastBeatTime = -minTimeBetweenBlocks;

            int historySize = 43; // number of past energy values to consider for the average (about 1 second at 44100Hz with windowSize=1024)

            // analyze the audio data in windows to detect beats
            for (int i = 0; i < samples.Length; i += windowSize)
            {
                // calculate the average energy in the current window
                float currentEnergy = 0;
                for (int j = 0; j < windowSize && i + j < samples.Length; j++)
                {
                    currentEnergy += Mathf.Abs(samples[i + j]);
                }
                currentEnergy /= windowSize;

                // average energy from history
                float averageEnergy = 0;
                if (energyHistory.Count > 0)
                {
                    foreach (float e in energyHistory) averageEnergy += e;
                    averageEnergy /= energyHistory.Count;
                }

                // calculate the current time in seconds
                float currentTime = (float)i / (audioTrack.frequency * audioTrack.channels);

                // detect a beat if the current energy is significantly higher than the average 
                // and if enough time has passed since the last detected beat
                if (currentEnergy > (averageEnergy * sensitivity) && (currentTime - lastBeatTime) > minTimeBetweenBlocks)
                {
                    // set up a new step in the pose map for this beat
                    SquatPoseMapStep newStep = new SquatPoseMapStep();
                    newStep.spawnTime = currentTime;
                    newStep.wallPrefab = defaultWallPrefab;

                    // randomly pick a pose from the list if available
                    if (randomPosesToPick != null && randomPosesToPick.Count > 0)
                    {
                        int randomIndex = Random.Range(0, randomPosesToPick.Count);
                        newStep.poseAsset = randomPosesToPick[randomIndex];
                    }

                    targetPoseMap.steps.Add(newStep);
                    lastBeatTime = currentTime; 
                }

             
                energyHistory.Add(currentEnergy);
                if (energyHistory.Count > historySize) energyHistory.RemoveAt(0);
            }

            Debug.Log($"Finished! {targetPoseMap.steps.Count} blocks were added to the map according to the beat!");

            //Save the changes to the Posemap asset
#if UNITY_EDITOR
            EditorUtility.SetDirty(targetPoseMap);
            AssetDatabase.SaveAssets();
#endif
        }
    }
}