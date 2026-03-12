/* using UnityEngine;
using System.Collections;
using TMPro; 

namespace SquatGame
{
    [RequireComponent(typeof(AudioSource))]
    public class SquatManager : MonoBehaviour
    {
        [Header("Debug")]
        public bool startGameNow = false; 

        [Header("Setup")]
        public GameObject wallPrefab; 
        public Transform spawnPoint;  
        
        [Header("Audio")]
        public AudioClip gameMusic; 
       

        [Header("Difficulty")]
        public float spawnInterval; 
        public float holeHeightMax; 
        public float holeHeightMin;
        public float heightOffset; 

        [Header("UI")]
        public GameObject startButton; 
        public GameObject stopButton;
        public TextMeshProUGUI countdownText; 
        public int score = 0;
        public int combo = 0; 
        
        public bool isRunning = false;
        
        private AudioSource audioSource;
        private float timer = 0;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            ShowStartMenu();
            if(countdownText) countdownText.gameObject.SetActive(false);
        }

      
        public void OnStartButtonPressed()
        {
            StartCoroutine(StartGameRoutine());
        }

        [ContextMenu("▶ START SEQUENCE NOW")]
        public void DebugStartSequence()
        {
            if(Application.isPlaying)
            {
                StartCoroutine(StartGameRoutine());
            }
            else
            {
                Debug.LogWarning("Press play first!");
            }

        }
        IEnumerator StartGameRoutine()
        {
            
            isRunning = false;
            score = 0;
            combo = 0;
            timer = 0;

            if(startButton) startButton.SetActive(false);
            
            var walls = FindObjectsByType<SquatWall>(FindObjectsSortMode.None);
            foreach (var w in walls) Destroy(w.gameObject);

            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                countdownText.text = "3"; yield return new WaitForSeconds(1.0f);
                countdownText.text = "2"; yield return new WaitForSeconds(1.0f);
                countdownText.text = "1"; yield return new WaitForSeconds(1.0f);
                countdownText.text = "GO!"; yield return new WaitForSeconds(0.5f);
                countdownText.gameObject.SetActive(false);
                stopButton.SetActive(true);
            }

            
            if (audioSource != null && gameMusic != null)
            {
                audioSource.clip = gameMusic;
                audioSource.loop = false; 
                audioSource.Play();
            }

           
            isRunning = true;
            SpawnWall(); 
            timer = spawnInterval;
        }

        void Update()
        {
           
            if (startGameNow)
            {
                startGameNow = false;
                StartCoroutine(StartGameRoutine());
            }

            if (!isRunning) return;

        
            if (audioSource != null && !audioSource.isPlaying)
            {
                ShowStartMenu();
                return;
            }

           
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                SpawnWall();
                timer = spawnInterval;
            }
        }

        void ShowStartMenu()
        {
            Debug.Log("Round finished. Showing start button.");
            isRunning = false; 
            if(startButton) startButton.SetActive(true);
            
           
        }

        void SpawnWall()
        {
            if (wallPrefab == null) return;
            GameObject newWall = Instantiate(wallPrefab, spawnPoint.position, spawnPoint.rotation);
            
            SquatWall wallScript = newWall.GetComponent<SquatWall>();
            if (wallScript == null) wallScript = newWall.AddComponent<SquatWall>();
            wallScript.manager = this;

            float randomHeight = Random.Range(holeHeightMin, holeHeightMax);
            newWall.transform.Translate(Vector3.up * (randomHeight + heightOffset), Space.Self);
        }

        public void AddScore()
        {
            if (!isRunning) return;
            combo++; 
            score += 10 * combo; 
        }

        public void PlayerHit()
        {
            if (!isRunning) return;
            combo = 0;
            score -= 50; 
            if (score < 0) score = 0;
        }
        public void OnStopButtonPressed()
        {
            if (!isRunning) return;
            if (audioSource != null) audioSource.Stop();
            ShowStartMenu();
            stopButton.SetActive(false);
        }
    }
}  */
 
 


using UnityEngine;
using System.Collections;
using TMPro; 
using PlayerPoseEngine.Scripts; 

namespace SquatGame
{
    [RequireComponent(typeof(AudioSource))]
    public class SquatManager : MonoBehaviour
    {
        [Header("Debug")]
        public bool startGameNow = false; 

        [Header("Setup")]
        public Transform spawnPoint;  

        [Header("Player Tracking (automatic height adjustment)")]
        [Tooltip("Main VR camera (to measure eye height)")]
        public Transform playerHead; 
        [Tooltip("The center/floor of your play area (e.g. XR Origin)")]
        public Transform playerRoot;
        public float referencePlayerHeight = 1.6f;
        
        [Header("Level Data (Timeline)")]
        [Tooltip("Drag your created PoseMap file here!")]
        public SquatPoseMap currentPoseMap;

        [Header("Audio")]
        public AudioClip gameMusic; 

        [Header("Difficulty")]
        public float holeHeightMax = 1.6f; 
        public float holeHeightMin = 1.2f;
        public float heightOffset = 0f; 

        [Header("UI")]
        public GameObject startButton; 
        public GameObject stopButton;
        public TextMeshProUGUI countdownText; 
        public int score = 0;
        public int combo = 0; 
        
        public bool isRunning = false;
        
        private AudioSource audioSource;
        
        // PoseMap Tracking
        private float timer = 0f;
        private int currentStepIndex = 0;

         // Auto-Size Tracking
        private float currentHeightScaleFactor = 1.0f;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            ShowStartMenu();
            if(countdownText) countdownText.gameObject.SetActive(false);
        }

        public void OnStartButtonPressed()
        {
            StartCoroutine(StartGameRoutine());
        }

        [ContextMenu("▶ START SEQUENCE NOW")]
        public void DebugStartSequence()
        {
            if(Application.isPlaying)
            {
                StartCoroutine(StartGameRoutine());
            }
            else
            {
                Debug.LogWarning("Press play first!");
            }
        }

        IEnumerator StartGameRoutine()
        {
            isRunning = false;
            score = 0;
            combo = 0;
            
            // Set timer and index to 0, so we start from the beginning of the PoseMap
            timer = 0f;
            currentStepIndex = 0;
             if (playerHead != null && playerRoot != null)
            {
                // Measure the actual distance from the floor to the headset
                float actualPlayerHeight = playerHead.position.y - playerRoot.position.y;
                
                // Calculate the scaling factor (e.g. 1.2m / 1.6m = 0.75)
                currentHeightScaleFactor = actualPlayerHeight / referencePlayerHeight;
                Debug.Log($"Height measured: {actualPlayerHeight}m. Walls will be scaled by factor {currentHeightScaleFactor}.");
            }
            else
            {
                Debug.LogWarning("PlayerHead or PlayerRoot is missing! Automatic scaling disabled.");
                currentHeightScaleFactor = 1.0f; // Use standard size if nothing is assigned
            }

            if(startButton) startButton.SetActive(false);
            
            var walls = FindObjectsByType<SquatWall>(FindObjectsSortMode.None);
            foreach (var w in walls) Destroy(w.gameObject);

            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                countdownText.text = "3"; yield return new WaitForSeconds(1.0f);
                countdownText.text = "2"; yield return new WaitForSeconds(1.0f);
                countdownText.text = "1"; yield return new WaitForSeconds(1.0f);
                countdownText.text = "GO!"; yield return new WaitForSeconds(0.5f);
                countdownText.gameObject.SetActive(false);
                stopButton.SetActive(true);
            }

            // Sort the PoseMap steps by spawn time to ensure correct order
            if (currentPoseMap != null && currentPoseMap.steps.Count > 0)
            {
                currentPoseMap.steps.Sort((a, b) => a.spawnTime.CompareTo(b.spawnTime));
            }

            if (audioSource != null && gameMusic != null)
            {
                audioSource.clip = gameMusic;
                audioSource.loop = false; 
                audioSource.Play();
            }

            isRunning = true;
        }

        void Update()
        {
            if (startGameNow)
            {
                startGameNow = false;
                StartCoroutine(StartGameRoutine());
            }

            if (!isRunning) return;

            if (audioSource != null && !audioSource.isPlaying)
            {
                ShowStartMenu();
                return;
            }

            // Play the PoseMap timeline: Check if we have a PoseMap and if there are steps left to spawn
            if (currentPoseMap != null && currentStepIndex < currentPoseMap.steps.Count)
            {
                timer += Time.deltaTime; // Timer synced with the game time

                if (timer >= currentPoseMap.steps[currentStepIndex].spawnTime)
                {
                    SquatPoseMapStep step = currentPoseMap.steps[currentStepIndex];
                    
                    // Spawn the wall with the associated pose 
                    SpawnWall(step.wallPrefab, step.poseAsset);
                    
                    currentStepIndex++; 
                }
            }
        }

        void ShowStartMenu()
        {
            Debug.Log("Round finished. Showing start button.");
            isRunning = false; 
            if(startButton) startButton.SetActive(true);
        }

        // Complete SpawnWall method that takes into account the pose data for dynamic hole heights
        void SpawnWall(GameObject prefabToSpawn, PlayerPose pose)
        {
            if (prefabToSpawn == null) return;
            
            // Wall spawn
            GameObject newWall = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
            
            SquatWall wallScript = newWall.GetComponent<SquatWall>();
            if (wallScript == null) wallScript = newWall.AddComponent<SquatWall>();
            wallScript.manager = this;

            // random height as default
            float targetHeight = Random.Range(holeHeightMin, holeHeightMax);

            // if pose data is available, adjust the target height based on the head position
            if (pose != null)
            {
                foreach (var req in pose.limbRequirements)
                {
                    if (req.limb == Limb.HEAD)
                    {
                        // If the head is above a certain threshold, we assume
                        //  the player is standing and set a higher hole, otherwise a lower one for squats
                        targetHeight = (req.relativePos.y > 1.4f) ? holeHeightMax : holeHeightMin;
                        break;
                    }
                }
            }

            // Apply height offset and move the wall up
            newWall.transform.Translate(Vector3.up * (targetHeight + heightOffset), Space.Self);
        }

        public void AddScore()
        {
            if (!isRunning) return;
            combo++; 
            score += 10 * combo; 
        }

        public void PlayerHit()
        {
            if (!isRunning) return;
            combo = 0;
            score -= 50; 
            if (score < 0) score = 0;
        }

        public void OnStopButtonPressed()
        {
            if (!isRunning) return;
            if (audioSource != null) audioSource.Stop();
            ShowStartMenu();
            stopButton.SetActive(false);
        }
    }
} 








/* using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; 
using PlayerPoseEngine.Scripts;

namespace SquatGame
{
    [RequireComponent(typeof(AudioSource))]
    public class SquatManager : MonoBehaviour
    {
        [Header("Debug")]
        public bool startGameNow = false; 

        [Header("Setup")]
        public Transform spawnPoint;  
        
        [Header("Level Data (Die Zeitleiste)")]
        [Tooltip("Ziehe hier deine erstellte PoseMap-Datei rein!")]
        public SquatPoseMap currentPoseMap;

        [Header("Player Tracking (Automatische Größe)")]
        [Tooltip("Deine VR-Kamera (um die Augenhöhe zu messen)")]
        public Transform playerHead; 
        [Tooltip("Der Mittelpunkt/Boden deines Spielbereichs (z.B. XR Origin)")]
        public Transform playerRoot;

        [Header("Audio")]
        public AudioClip gameMusic; 

        [Header("Difficulty & Kalibrierung")]
        [Tooltip("Für welche Körpergröße hast du die Min/Max Werte unten perfekt eingestellt? (Standard: 1.6)")]
        public float referencePlayerHeight = 1.6f;
        public float holeHeightMax = 1.6f; 
        public float holeHeightMin = 1.2f;
        public float heightOffset = 0f; 

        [Header("UI")]
        public GameObject startButton; 
        public GameObject stopButton;
        public TextMeshProUGUI countdownText; 
        public int score = 0;
        public int combo = 0; 
        
        public bool isRunning = false;
        
        private AudioSource audioSource;
        
        // PoseMap Tracking
        private float timer = 0f;
        private int currentStepIndex = 0;

        // Auto-Size Tracking
        private float currentHeightScaleFactor = 1.0f;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            ShowStartMenu();
            if(countdownText) countdownText.gameObject.SetActive(false);
        }

        public void OnStartButtonPressed()
        {
            StartCoroutine(StartGameRoutine());
        }

        [ContextMenu("▶ START SEQUENCE NOW")]
        public void DebugStartSequence()
        {
            if(Application.isPlaying)
            {
                StartCoroutine(StartGameRoutine());
            }
            else
            {
                Debug.LogWarning("Press play first!");
            }
        }

        IEnumerator StartGameRoutine()
        {
            isRunning = false;
            score = 0;
            combo = 0;
            
            // Timer und Liste auf Anfang setzen
            timer = 0f;
            currentStepIndex = 0;

            // --- NEU: KÖRPERGRÖßE MESSEN ---
            if (playerHead != null && playerRoot != null)
            {
                // Tatsächliche Distanz vom Boden zur Brille messen
                float actualPlayerHeight = playerHead.position.y - playerRoot.position.y;
                
                // Skalierungsfaktor berechnen (z.B. 1.2m / 1.6m = 0.75)
                currentHeightScaleFactor = actualPlayerHeight / referencePlayerHeight;
                Debug.Log($"Größe gemessen: {actualPlayerHeight}m. Wände werden mit dem Faktor {currentHeightScaleFactor} skaliert.");
            }
            else
            {
                Debug.LogWarning("PlayerHead oder PlayerRoot fehlt! Automatische Skalierung deaktiviert.");
                currentHeightScaleFactor = 1.0f; // Standardgröße nutzen, falls nichts zugewiesen wurde
            }
            // ---------------------------------

            if(startButton) startButton.SetActive(false);
            
            var walls = FindObjectsByType<SquatWall>(FindObjectsSortMode.None);
            foreach (var w in walls) Destroy(w.gameObject);

            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                countdownText.text = "3"; yield return new WaitForSeconds(1.0f);
                countdownText.text = "2"; yield return new WaitForSeconds(1.0f);
                countdownText.text = "1"; yield return new WaitForSeconds(1.0f);
                countdownText.text = "GO!"; yield return new WaitForSeconds(0.5f);
                countdownText.gameObject.SetActive(false);
                stopButton.SetActive(true);
            }

            // Map sicherheitshalber nach Zeit sortieren
            if (currentPoseMap != null && currentPoseMap.steps.Count > 0)
            {
                currentPoseMap.steps.Sort((a, b) => a.spawnTime.CompareTo(b.spawnTime));
            }

            if (audioSource != null && gameMusic != null)
            {
                audioSource.clip = gameMusic;
                audioSource.loop = false; 
                audioSource.Play();
            }

            isRunning = true;
        }

        void Update()
        {
            if (startGameNow)
            {
                startGameNow = false;
                StartCoroutine(StartGameRoutine());
            }

            if (!isRunning) return;

            if (audioSource != null && !audioSource.isPlaying)
            {
                ShowStartMenu();
                return;
            }

            // --- POSEMAP ABSPIELEN ---
            if (currentPoseMap != null && currentStepIndex < currentPoseMap.steps.Count)
            {
                timer += Time.deltaTime; 

                if (timer >= currentPoseMap.steps[currentStepIndex].spawnTime)
                {
                    SquatPoseMapStep step = currentPoseMap.steps[currentStepIndex];
                    SpawnWall(step.wallPrefab, step.poseAsset);
                    currentStepIndex++; 
                }
            }
        }

        void ShowStartMenu()
        {
            Debug.Log("Round finished. Showing start button.");
            isRunning = false; 
            if(startButton) startButton.SetActive(true);
        }

        void SpawnWall(GameObject prefabToSpawn, PlayerPose pose)
        {
            if (prefabToSpawn == null) return;
            
            GameObject newWall = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
            
            SquatWall wallScript = newWall.GetComponent<SquatWall>();
            if (wallScript == null) wallScript = newWall.AddComponent<SquatWall>();
            wallScript.manager = this;

            // 1. Standard-Höhe ist Zufall (falls keine Pose hinterlegt wurde)
            float baseTargetHeight = Random.Range(holeHeightMin, holeHeightMax);

            // 2. Pose auswerten (Ist es eine Stand-Pose oder ein Squat?)
            if (pose != null)
            {
                foreach (var req in pose.limbRequirements)
                {
                    if (req.limb == Limb.HEAD)
                    {
                        baseTargetHeight = (req.relativePos.y > 1.4f) ? holeHeightMax : holeHeightMin;
                        break;
                    }
                }
            }

            // --- 3. DIE MAGIE: GRÖßENANPASSUNG AN DEN SPIELER ---
            float scaledHeight = baseTargetHeight * currentHeightScaleFactor;
            
            // 4. Wand auf die final berechnete Höhe plus Offset schieben
            newWall.transform.Translate(Vector3.up * (scaledHeight + heightOffset), Space.Self);
        }

        public void AddScore()
        {
            if (!isRunning) return;
            combo++; 
            score += 10 * combo; 
        }

        public void PlayerHit()
        {
            if (!isRunning) return;
            combo = 0;
            score -= 50; 
            if (score < 0) score = 0;
        }

        public void OnStopButtonPressed()
        {
            if (!isRunning) return;
            if (audioSource != null) audioSource.Stop();
            ShowStartMenu();
            stopButton.SetActive(false);
        }
    }
} */