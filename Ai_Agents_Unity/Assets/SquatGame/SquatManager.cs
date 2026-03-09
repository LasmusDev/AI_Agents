using UnityEngine;
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
} 
 


