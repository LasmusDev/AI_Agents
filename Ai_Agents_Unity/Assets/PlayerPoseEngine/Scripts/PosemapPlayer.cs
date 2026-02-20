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
        [Tooltip("Short and hard on hit")]
        public float hitDuration = 0.1f;
        [Range(0,1)] public float hitStrength = 1.0f;
        
        [Tooltip("Long and soft on miss")]
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
        public void DebugStart() { 
            StartGame(); 
            }

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