/* using PlayerPoseEngine.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace PlayerPoseEngine.Scripts{
    
    public class PosemapPlayer : MonoBehaviour
    {
        public int BPM;
        public Vector3 from;
        public Vector3 to;
        public int currBeat;
        public int visibleBeats;
    
        [SerializeField, ReadOnly]
        public int score = 0;
    
        [SerializeField, ReadOnly]
        public int combo = 1;
    
        [SerializeField, ReadOnly]
        private float poseSpeed;
        [SerializeField, ReadOnly]
        private float timePerBeat;
    
        public Posemap poseMap;
        public PosePool pool;
    
    
        List<PlayerPoseResolver> activePoseResolvers = new List<PlayerPoseResolver>();
    
    
        public void Start()
        {        
            timePerBeat = 60f / BPM;
            poseSpeed = (1f/(float)visibleBeats)/timePerBeat;
            StartPosemapPlayback();
        }
    
        
    
        public void StartPosemapPlayback()
        {
            StartCoroutine(PlayPosemap(poseMap));
        }
    
        public void StartPosemapPlayback(Posemap map)
        {
            poseMap = map;
            StartCoroutine(PlayPosemap(map));
        }
    
        public void StopPosemapPlayback()
        {
            StopAllCoroutines();
        }
    
        public void ScorePose(PlayerPoseResolver resolver, PlayerPose p)
        {
            combo++;
            score += 100 * combo;
            ReleaseResolver(resolver);
        }
    
        public void ReleaseResolver(PlayerPoseResolver res)
        {
            res.onPlayerPoseFulfilled -= ScorePose;
            pool.Release(res);
            activePoseResolvers.Remove(res);
        }
    
        public PlayerPoseResolver ConnectToResolver(PlayerPoseResolver res)
        {       
            res.transform.position = from;
            res.transform.LookAt(res.transform.position + this.transform.forward);
            activePoseResolvers.Add(res);
            res.onPlayerPoseFulfilled += ScorePose;
            return res;
        }
    
        public IEnumerator PlayPosemap(Posemap map)
        {
            activePoseResolvers = new List<PlayerPoseResolver>();
            float timeSinceLastBeat = 0;
            while (true)
            {
                timeSinceLastBeat += Time.deltaTime;
                if (timeSinceLastBeat > timePerBeat)
                {
                    timeSinceLastBeat -= timePerBeat;
                    currBeat += 1;
                    PlayerPose next = map.GetPose(currBeat - visibleBeats);
                    if (next != null)
                    {
                        ConnectToResolver(pool.Get()).RequestPose(next); ;
                    }
                }
                Vector3 movement = (to - from) * poseSpeed * Time.deltaTime;
                PlayerPoseResolver toRemove = null;
                foreach(PlayerPoseResolver resolver in activePoseResolvers)
                {
                   
                    resolver.transform.position += movement;
                    if(Vector3.Distance(from, to) < Vector3.Distance(from, resolver.transform.position))
                    {                     
                        toRemove = resolver;
                    }
                }
                if(toRemove != null)
                {
                    combo = 0;
                    ReleaseResolver(toRemove);
                }
                yield return null;
            }
    
        }
    }
    
}
 */




/* using UnityEngine;
using TMPro; 
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PlayerPoseEngine.Scripts
{
    [RequireComponent(typeof(AudioSource))]
    public class PosemapPlayer : MonoBehaviour
    {
        [Header("Audio Settings")]
        [Range(0f, 1f)] public float musicVolume = 0.5f;
        public float BPM = 120f;

        [Header("Spawn Settings")]
        public Vector3 from;
        public Vector3 to;
        public float visibleBeats = 4f;
        public float spawnOffset = 0f;

        [Header("UI References")]
        public GameObject startMenuCanvas;    
        public TextMeshProUGUI countdownText; 
        public TextMeshProUGUI scoreText; // Optional: Zeigt Score im Spiel/Ende an

        [Header("Runtime Stats")]
        [SerializeField] public float songPosition;
        [SerializeField] public float songPositionInBeats;
        [SerializeField] public int score = 0;
        [SerializeField] public int combo = 0;

        [Header("References")]
        public Posemap poseMap;
        public PosePool pool;
        
        private AudioSource audioSource;
        private float secPerBeat;
        private float dspSongStartTime;
        private int nextIndexToSpawn = 0;
        private bool isPlaying = false; // Ist das Spiel aktiv?
        private bool songStarted = false; // Lief der Song schon los?
        private List<PlayerPoseResolver> activePoseResolvers = new List<PlayerPoseResolver>();

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void Start()
        {
            if(audioSource != null) audioSource.volume = musicVolume;
            if(countdownText != null) countdownText.gameObject.SetActive(false);
            
            // Menü sicherheitshalber aktivieren am Anfang
            if(startMenuCanvas != null) startMenuCanvas.SetActive(true);
        }

        public void StartGame()
        {
            if (poseMap != null)
            {
                // Verhindert Doppel-Klicks
                if (isPlaying) return; 

                StartCoroutine(StartCountdownRoutine());
            }
            else
            {
                Debug.LogError("Keine Posemap zugewiesen!");
            }
        }

        IEnumerator StartCountdownRoutine()
        {
            if (startMenuCanvas != null) startMenuCanvas.SetActive(false);
            
            // Cleanup: Falls vom letzten Spiel noch Wände da sind -> WEG DAMIT!
            CleanupActiveResolvers();

            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                countdownText.text = "3";
                yield return new WaitForSeconds(1.0f);
                countdownText.text = "2";
                yield return new WaitForSeconds(1.0f);
                countdownText.text = "1";
                yield return new WaitForSeconds(1.0f);
                countdownText.text = "GO!";
                yield return new WaitForSeconds(0.5f);
                countdownText.gameObject.SetActive(false);
            }

            StartPosemapPlayback(poseMap);
        }

        public void StartPosemapPlayback(Posemap map)
        {
            poseMap = map;
            if (poseMap.bpm > 0) this.BPM = poseMap.bpm;
            secPerBeat = 60f / BPM;

            if (poseMap.song != null)
            {
                audioSource.clip = poseMap.song;
                audioSource.volume = musicVolume;
            }

            // Reset Stats
            poseMap.poses = poseMap.poses.OrderBy(x => x.beat).ToArray();
            nextIndexToSpawn = 0;
            combo = 0;
            score = 0;
            
            // Audio Start
            dspSongStartTime = (float)AudioSettings.dspTime + 0.1f; // Kleiner Puffer
            audioSource.PlayScheduled(dspSongStartTime);
            
            isPlaying = true;
            songStarted = true;
        }
        
        // Hilfsmethode zum Aufräumen aller fliegenden Wände
        void CleanupActiveResolvers()
        {
            // Alle Wände, die noch in der Liste sind, zurück in den Pool werfen
            foreach(var res in activePoseResolvers)
            {
                if(res != null) pool.Release(res);
            }
            activePoseResolvers.Clear();
        }

        public void FinishGame()
        {
            isPlaying = false;
            songStarted = false;
            
            Debug.Log("Game Finished!");
            
            // Menü wieder anzeigen
            if (startMenuCanvas != null) 
            {
                startMenuCanvas.SetActive(true);
                
                // Optional: Button-Text ändern auf "RESTART"
                // Dazu müsstest du eine Referenz auf den TextMeshPro Button haben.
            }
        }

        void Update()
        {
            if (!isPlaying) return;

            // Lautstärke Update
            if (audioSource != null && Mathf.Abs(audioSource.volume - musicVolume) > 0.001f) 
                audioSource.volume = musicVolume;

            // --- END GAME CHECK ---
            // Wenn der Song gestartet wurde, aber die AudioSource nicht mehr spielt, ist er vorbei.
            if (songStarted && !audioSource.isPlaying)
            {
                // Kleiner Sicherheitscheck: Sind wir wirklich am Ende oder war es ein Lag?
                // (SongPosition > 1 Sekunde)
                if ((AudioSettings.dspTime - dspSongStartTime) > 1.0f)
                {
                    FinishGame();
                    return;
                }
            }

            // --- ZEIT & LOGIK ---
            songPosition = (float)(AudioSettings.dspTime - dspSongStartTime - spawnOffset);
            songPositionInBeats = songPosition / secPerBeat;
            
            // UI Update (Score)
            if (scoreText != null) scoreText.text = $"Score: {score}\nCombo: {combo}x";

            float lookAheadBeat = songPositionInBeats + visibleBeats;
            while (nextIndexToSpawn < poseMap.poses.Length && poseMap.poses[nextIndexToSpawn].beat < lookAheadBeat)
            {
                SpawnPose(poseMap.poses[nextIndexToSpawn]);
                nextIndexToSpawn++;
            }
            UpdateActiveResolvers();
        }

        void SpawnPose(BeatToPose beatData)
        {
            PlayerPoseResolver newResolver = pool.Get();
            newResolver.transform.position = from; 
            newResolver.transform.LookAt(to); 
            newResolver.targetBeat = beatData.beat; 
            newResolver.RequestPose(beatData.pose);
            newResolver.onPlayerPoseFulfilled += ScorePose;
            activePoseResolvers.Add(newResolver);
        }

        void UpdateActiveResolvers()
        {
            for (int i = activePoseResolvers.Count - 1; i >= 0; i--)
            {
                PlayerPoseResolver res = activePoseResolvers[i];
                float beatsRemaining = res.targetBeat - songPositionInBeats;
                float t = 1f - (beatsRemaining / visibleBeats);
                res.transform.position = Vector3.LerpUnclamped(from, to, t);

                if (beatsRemaining < -1.0f) 
                {
                    if(combo > 0) combo = 0; // Miss reset
                    ReleaseResolver(res);
                }
            }
        }

        public void ScorePose(PlayerPoseResolver resolver, PlayerPose p)
        {
            float timingError = Mathf.Abs(resolver.targetBeat - songPositionInBeats);
            if (timingError < 0.5f) 
            {
                combo++;
                score += 100 * combo;
            }
            ReleaseResolver(resolver);
        }

        public void ReleaseResolver(PlayerPoseResolver res)
        {
            res.onPlayerPoseFulfilled -= ScorePose;
            activePoseResolvers.Remove(res);
            pool.Release(res);
        }
    }
} 
 */






using PlayerPoseEngine.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using TMPro; 
using UnityEngine;
using UnityEngine.XR; 

namespace PlayerPoseEngine.Scripts
{
    [RequireComponent(typeof(AudioSource))]
    public class PosemapPlayer : MonoBehaviour
    {
        [Header("Settings")]
        public float BPM = 120f; 
        public Vector3 from;
        public Vector3 to;
        public float visibleBeats = 4f;
        public float spawnOffset = 0f;

        [Header("Haptic Feedback (Vibration)")]
        [Tooltip("Kurz und hart bei Treffer")]
        public float hitDuration = 0.1f;
        [Range(0,1)] public float hitStrength = 1.0f;
        
        [Tooltip("Lang und weich bei Fehler")]
        public float missDuration = 0.5f;
        [Range(0,1)] public float missStrength = 0.3f;

        [Header("Player Tracking")]
        public GameObject playerHead;
        public GameObject playerLeftHand;
        public GameObject playerRightHand;

        [Header("UI")]
        public GameObject startMenuCanvas;    
        public TextMeshProUGUI countdownText; 
        public TextMeshProUGUI scoreText;

        [Header("Stats")]
        [SerializeField] public int score = 0;
        [SerializeField] public int combo = 0;

        public Posemap poseMap;
        public PosePool pool;
        
        private AudioSource audioSource;
        private float secPerBeat;
        private float dspSongStartTime;
        private int nextIndexToSpawn = 0;
        private bool isPlaying = false; 
        private bool songStarted = false; 
        private List<PlayerPoseResolver> activePoseResolvers = new List<PlayerPoseResolver>();

        private void Awake() { audioSource = GetComponent<AudioSource>(); }

        public void Start() 
        { 
            if(startMenuCanvas) startMenuCanvas.SetActive(true); 
            if(countdownText) countdownText.gameObject.SetActive(false);
        }

        [ContextMenu("DEBUG START")]
        public void DebugStart() { StartGame(); }

        public void StartGame()
        {
            if (poseMap != null) StartCoroutine(StartCountdownRoutine());
        }

        IEnumerator StartCountdownRoutine()
        {
            if(startMenuCanvas) startMenuCanvas.SetActive(false);
            foreach(var r in activePoseResolvers) if(r) pool.Release(r);
            activePoseResolvers.Clear();

            if(countdownText) {
                countdownText.gameObject.SetActive(true);
                countdownText.text = "3"; yield return new WaitForSeconds(1);
                countdownText.text = "2"; yield return new WaitForSeconds(1);
                countdownText.text = "1"; yield return new WaitForSeconds(1);
                countdownText.text = "GO"; yield return new WaitForSeconds(0.5f);
                countdownText.gameObject.SetActive(false);
            }
            StartPosemapPlayback(poseMap);
        }

        public void StartPosemapPlayback(Posemap map)
        {
            poseMap = map;
            if (poseMap.bpm > 0) this.BPM = poseMap.bpm;
            secPerBeat = 60f / BPM;

            if (poseMap.song != null) { audioSource.clip = poseMap.song; }
            
            poseMap.poses = poseMap.poses.OrderBy(x => x.beat).ToArray();
            nextIndexToSpawn = 0; combo = 0; score = 0;
            
            dspSongStartTime = (float)AudioSettings.dspTime + 0.1f;
            audioSource.PlayScheduled(dspSongStartTime);
            isPlaying = true; songStarted = true;
        }

        void Update()
        {
            if (!isPlaying) return;

            
            if (songStarted && !audioSource.isPlaying && (AudioSettings.dspTime - dspSongStartTime) > 2.0f)
            {
                isPlaying = false;
                if(startMenuCanvas) startMenuCanvas.SetActive(true);
                return;
            }

            float songPos = (float)(AudioSettings.dspTime - dspSongStartTime - spawnOffset);
            float songBeats = songPos / secPerBeat;
            
            if (scoreText) scoreText.text = $"Score: {score}\nCombo: {combo}x";

           
            float lookAhead = songBeats + visibleBeats;
            while (nextIndexToSpawn < poseMap.poses.Length && poseMap.poses[nextIndexToSpawn].beat < lookAhead)
            {
                SpawnPose(poseMap.poses[nextIndexToSpawn]);
                nextIndexToSpawn++;
            }
            
            
            for (int i = activePoseResolvers.Count - 1; i >= 0; i--)
            {
                PlayerPoseResolver res = activePoseResolvers[i];
                float remaining = res.targetBeat - songBeats;
                float t = 1f - (remaining / visibleBeats);
                res.transform.position = Vector3.LerpUnclamped(from, to, t);

                
                if (remaining < -1.0f) 
                {
                    if(combo > 0) 
                    {
                        combo = 0;
                      
                        TriggerHaptics(missStrength, missDuration);
                    }
                    
                    res.onPlayerPoseFulfilled -= ScorePose;
                    activePoseResolvers.Remove(res);
                    pool.Release(res);
                }
            }
        }

        void SpawnPose(BeatToPose beatData)
        {
            PlayerPoseResolver r = pool.Get();
            r.transform.position = from; 
            r.transform.LookAt(to); 
            r.targetBeat = beatData.beat; 
            
            r.headObject = playerHead;
            r.lHandObject = playerLeftHand;
            r.rHandObject = playerRightHand;
            if(playerHead) r.playerSize = playerHead.transform.position.y;

            r.RequestPose(beatData.pose);
            r.onPlayerPoseFulfilled += ScorePose;
            activePoseResolvers.Add(r);
        }

        public void ScorePose(PlayerPoseResolver res, PlayerPose p)
        {
            combo++; 
            score += 100 * combo;
            
            
            TriggerHaptics(hitStrength, hitDuration);
            
            res.onPlayerPoseFulfilled -= ScorePose;
            activePoseResolvers.Remove(res);
            pool.Release(res);
        }

        void TriggerHaptics(float strength, float duration)
        {
          
            SendImpulseToHand(XRNode.RightHand, strength, duration);
            SendImpulseToHand(XRNode.LeftHand, strength, duration);
        }

        void SendImpulseToHand(XRNode node, float strength, float duration)
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(node);
            if (device.isValid)
            {
                
                device.SendHapticImpulse(0, strength, duration);
            }
        }
    }
}