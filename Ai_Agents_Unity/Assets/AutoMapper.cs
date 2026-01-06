using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlayerPoseEngine.Scripts
{
    public class AutoMapper : MonoBehaviour
    {
        [Header("Detection Settings")]
        [Tooltip("Wie empfindlich reagiert das System auf Lautstärke? (1.2 - 2.0 ist meist gut)")]
        public float sensitivity = 1.5f; 
        
        [Tooltip("Mindestabstand zwischen zwei Poses in Sekunden (verhindert Spam)")]
        public float minDistance = 0.2f; // Bei 180 BPM lieber niedriger (0.2s) ansetzen!
        
        [Tooltip("Wie genau soll das Raster sein? 4 = 16tel Noten, 2 = 8tel Noten, 1 = Ganze Beats")]
        public float snapGrid = 4f; 

        [Header("Analysis Quality")]
        public int sampleWindow = 1024;

        [Header("References")]
        public Posemap targetPoseMap;
        public List<PlayerPose> possiblePoses; // Pool aus dem zufällig gewählt wird

        [ContextMenu("Generate Poses from Audio")]
        public void GenerateMap()
        {
            if (targetPoseMap == null || targetPoseMap.song == null)
            {
                Debug.LogError("Fehler: Keine Posemap oder kein Song zugewiesen!");
                return;
            }

            if (possiblePoses == null || possiblePoses.Count == 0)
            {
                Debug.LogError("Fehler: Die Liste 'Possible Poses' ist leer.");
                return;
            }

            // 1. Setup & Audio Daten holen
            AudioClip clip = targetPoseMap.song;
            
            // BPM holen (Falls nicht gesetzt, Standard 120 nutzen um Crash zu vermeiden)
            // HINWEIS: Stelle sicher, dass du 'public float bpm' in Posemap.cs hast!
            float bpm = targetPoseMap.bpm > 0 ? targetPoseMap.bpm : 120f;
            float secPerBeat = 60f / bpm;

            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            List<BeatToPose> newPoses = new List<BeatToPose>();
            int channels = clip.channels;
            
            float currentAverageEnergy = 0;
            float lastSpawnTime = -minDistance; // Damit wir direkt am Anfang spawnen dürfen

            // 2. Analyse Loop
            for (int i = 0; i < samples.Length; i += sampleWindow * channels)
            {
                float instantEnergy = 0;
                
                // Energie im aktuellen Fenster berechnen
                for (int j = 0; j < sampleWindow * channels; j++)
                {
                    if (i + j < samples.Length)
                    {
                        float val = samples[i + j];
                        instantEnergy += val * val; // Quadratische Energie (lauter = viel höherer Wert)
                    }
                }
                
                // Durchschnitt glätten (Moving Average)
                currentAverageEnergy = Mathf.Lerp(currentAverageEnergy, instantEnergy, 0.05f);

                // Zeit in Sekunden berechnen
                float currentTime = (float)i / (float)channels / clip.frequency;

                // 3. Beat Detection
                // Ist die aktuelle Stelle deutlich lauter als der Durchschnitt?
                if (instantEnergy > currentAverageEnergy * sensitivity && 
                    (currentTime - lastSpawnTime) > minDistance)
                {
                    // Treffer! Beat berechnen
                    float exactBeat = currentTime / secPerBeat;

                    // Quantisierung (Snapping)
                    // Wir runden den Beat auf das nächste Raster (z.B. Viertel-Schritte)
                    // Beispiel: 1.234 -> 1.25
                    float quantizedBeat = Mathf.Round(exactBeat * snapGrid) / snapGrid;

                    BeatToPose newEntry = new BeatToPose();
                    
                    // HIER: Das funktioniert nur, wenn du in Posemap.cs 'float beat' hast!
                    newEntry.beat = quantizedBeat; 
                    
                    // Zufällige Pose wählen
                    newEntry.pose = possiblePoses[Random.Range(0, possiblePoses.Count)];

                    // Doppelte Einträge vermeiden (falls zwei Beats auf denselben Snap fallen)
                    if(!newPoses.Any(x => Mathf.Approximately(x.beat, newEntry.beat)))
                    {
                        newPoses.Add(newEntry);
                        lastSpawnTime = currentTime;
                    }
                }
            }

            // 4. Sortieren und Speichern
            targetPoseMap.poses = newPoses.OrderBy(x => x.beat).ToArray();
            
            #if UNITY_EDITOR
            EditorUtility.SetDirty(targetPoseMap);
            #endif
            
            Debug.Log($"Analyse abgeschlossen für {bpm} BPM. {newPoses.Count} Poses generiert.");
        }
    }
}