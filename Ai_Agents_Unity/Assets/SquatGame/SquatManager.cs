using UnityEngine;
using System.Collections;
using TMPro; 
using PlayerPoseEngine.Scripts;

namespace SquatGame
{
    [RequireComponent(typeof(AudioSource))]
    public class SquatManager : MonoBehaviour
    {

        [Header("Setup")]
        public Transform spawnPoint;  
        
        [Header("Level Data")]
        public SquatPoseMap currentPoseMap;

        [Header("Player Tracking")]
        public Transform playerHead; 
        public Transform playerRoot;

        [Header("Audio")]
        public AudioClip gameMusic; 

        [Header("Difficulty ")]
        [Tooltip("Wall higher -> negativ value, Wall lower -> positive value")]
        public float highWallOffset = 0.2f;
        
        [Header("UI")]
        public GameObject startButton; 
        public GameObject stopButton;
        public TextMeshProUGUI countdownText; 
        public int score = 0;
        public int combo = 0; 
        
        public bool isRunning = false;
        
        private AudioSource audioSource;
        private float timer = 0f;
        private int currentStepIndex = 0;
        
        // saving the measured headset height to adjust wall spawn heights accordingly
        private float measuredHeadsetHeight = 1.6f;

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

        [ContextMenu("START SEQUENCE NOW")]
        public void DebugStartSequence()
        {
            if(Application.isPlaying)
            {
                StartCoroutine(StartGameRoutine());
            }
        }

        IEnumerator StartGameRoutine()
        {
            isRunning = false;
            score = 0;
            combo = 0;
            timer = 0f;
            currentStepIndex = 0;

            // Headset height measurement for dynamic wall height adjustment
            if (playerHead != null && playerRoot != null)
            {
                measuredHeadsetHeight = playerHead.position.y - playerRoot.position.y;
                Debug.LogWarning($"Headset measured at: {measuredHeadsetHeight}m.");
            }
            else
            {
                Debug.LogWarning("WARNING: PlayerHead or PlayerRoot missing! Using 1.6m as default.");
                measuredHeadsetHeight = 1.6f;
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

            if (!isRunning) return;

            if (audioSource != null && !audioSource.isPlaying)
            {
                ShowStartMenu();
                return;
            }

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

            // height based on pose data
            float targetHeight = measuredHeadsetHeight - highWallOffset;
            // spawn the wall at the calculated height
            Vector3 finalPosition = spawnPoint.position;
            float floorY = (playerRoot != null) ? playerRoot.position.y : 0f;
            
            finalPosition.y = floorY + targetHeight;

            newWall.transform.position = finalPosition;
        }

        public void AddScore()
        {
            if (!isRunning) return;
            combo++; 
            score += 50 + (25 * combo); 
        }

        public void PlayerHit()
        {
            if (!isRunning) return;
            combo = 0;
            score -= 100; 
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