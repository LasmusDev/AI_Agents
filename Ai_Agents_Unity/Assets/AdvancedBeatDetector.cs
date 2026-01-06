using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlayerPoseEngine.Scripts
{
    public class AdvancedBeatDetector : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The ScriptableObject to write the notes to")]
        public Posemap targetPoseMap;
        
        [Tooltip("The list of possible poses to randomly choose from")]
        public List<PlayerPose> posePalette;

        [Header("Music Analysis Settings")]
        [Tooltip("The exact BPM of the song (e.g., 180 for Samurai)")]
        public float bpm = 180f;
        
        [Tooltip("Shifts all notes in time (in seconds). MP3s often have 0.05s silence at the start.")]
        public float globalOffset = 0.05f;

        [Header("Algorithm Tuning")]
        [Range(1.01f, 3.0f)]
        [Tooltip("How much louder must a beat be than the average? (1.1 = very sensitive, 1.5 = hard kicks only)")]
        public float sensitivity = 1.3f;

        [Tooltip("How many samples are analyzed per step? (1024 is a good standard)")]
        public int sampleWindow = 1024;

        [Header("Gameplay Flow")]
        [Tooltip("Snap grid: 4 = sixteenth notes (fast), 2 = eighth notes, 1 = whole beats")]
        public int quantization = 4;
        
        [Tooltip("Minimum distance in beats. At 180 BPM, '2' prevents spawning every millisecond.")]
        public float minBeatsBetweenPoses = 1.0f;

        
        private List<float> debugBeatTimes = new List<float>();

        [ContextMenu("Analyze & Generate Map")]
        public void AnalyzeSong()
        {
            
            if (targetPoseMap == null || targetPoseMap.song == null)
            {
                Debug.LogError("ERROR: No posemap or song assigned!");
                return;
            }
            if (posePalette == null || posePalette.Count == 0)
            {
                Debug.LogError("ERROR: The 'Pose Palette' is empty! Please add poses.");
                return;
            }

            
            AudioClip clip = targetPoseMap.song;
            int channels = clip.channels;
            float[] audioData = new float[clip.samples * channels];
            clip.GetData(audioData, 0);

          
            List<BeatToPose> generatedPoses = new List<BeatToPose>();
            debugBeatTimes.Clear();
            
           
            targetPoseMap.poses = new BeatToPose[0];

          
            float maxEnergyFound = 0f;
            float avgEnergyOverall = 0f;
            int beatsFound = 0;

            
            float secPerBeat = 60f / bpm;
            int historySize = 43; 
            Queue<float> energyHistory = new Queue<float>();
            float lastAddedBeat = -10f;
            int lastPoseIndex = -1;

            
            for (int i = 0; i < audioData.Length; i += sampleWindow * channels)
            {
                
                float currentEnergy = 0;
                for (int j = 0; j < sampleWindow * channels; j++)
                {
                    if (i + j < audioData.Length)
                    {
                        float val = audioData[i + j];
                        currentEnergy += val * val;
                    }
                }
                currentEnergy = Mathf.Sqrt(currentEnergy / (sampleWindow * channels));

                
                if(currentEnergy > maxEnergyFound) maxEnergyFound = currentEnergy;
                avgEnergyOverall += currentEnergy;

               
                float localAverage = energyHistory.Count > 0 ? energyHistory.Average() : 0;
                energyHistory.Enqueue(currentEnergy);
                if (energyHistory.Count > historySize) energyHistory.Dequeue();

               
                if (currentEnergy > localAverage * sensitivity && currentEnergy > 0.05f)
                {
                    float currentTime = (float)i / (float)channels / clip.frequency;
                    currentTime -= globalOffset;

                    float rawBeat = currentTime / secPerBeat;
                    
                    
                    float snappedBeat = Mathf.Round(rawBeat * quantization) / (float)quantization;

                 
                    if (snappedBeat > lastAddedBeat + (minBeatsBetweenPoses / quantization) && snappedBeat >= 0)
                    {
                     
                        BeatToPose newEntry = new BeatToPose();
                        newEntry.beat = snappedBeat;
                        
                       
                        int randomIdx = UnityEngine.Random.Range(0, posePalette.Count);
                        if (randomIdx == lastPoseIndex && posePalette.Count > 1)
                        {
                            randomIdx = (randomIdx + 1) % posePalette.Count;
                        }

                        newEntry.pose = posePalette[randomIdx];
                        generatedPoses.Add(newEntry);
                        
                        
                        debugBeatTimes.Add(snappedBeat);

                        beatsFound++;
                        lastAddedBeat = snappedBeat;
                        lastPoseIndex = randomIdx;
                    }
                }
            }

            
            avgEnergyOverall /= (audioData.Length / (float)(sampleWindow * channels));

            
            Debug.Log("---------------- ANALYSIS REPORT ----------------");
            Debug.Log($"Song: {clip.name} | Max Energy: {maxEnergyFound:F4} | Avg Energy: {avgEnergyOverall:F4}");
            
          
            float recommendedSens = avgEnergyOverall > 0 ? (maxEnergyFound / avgEnergyOverall) : 1.5f;
            Debug.Log($"<b>Recommended Sensitivity approx.: {recommendedSens * 0.85f:F2}</b>");

            if (beatsFound == 0)
            {
                Debug.LogError($"<color=red>NO BEATS FOUND!</color> Your Sensitivity ({sensitivity}) is too high.");
                Debug.LogError($"Try this value: {recommendedSens * 0.7f:F2}");
            }
            else
            {
                Debug.Log($"<color=green>SUCCESS!</color> {beatsFound} poses have been generated.");
                
                
                targetPoseMap.poses = generatedPoses.OrderBy(x => x.beat).ToArray();
                #if UNITY_EDITOR
                EditorUtility.SetDirty(targetPoseMap);
                #endif
            }
            Debug.Log("Generation completed");
        }

        
        void OnDrawGizmos()
        {
          
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);

            if (debugBeatTimes == null || debugBeatTimes.Count == 0) return;

            Gizmos.color = new Color(1, 0, 0, 0.75f); 
            
           
            float visualSpacing = 2.0f; 

            foreach (float beat in debugBeatTimes)
            {
                
                Vector3 pos = transform.position + (transform.forward * beat * visualSpacing);
                
                Gizmos.DrawLine(pos + Vector3.up * 2, pos + Vector3.down * 2);
                
               
                Gizmos.DrawCube(pos, new Vector3(0.2f, 0.2f, 0.2f));
            }
        }
    }
}