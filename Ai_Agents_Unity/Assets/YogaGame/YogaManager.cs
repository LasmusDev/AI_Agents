using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace YogaGame
{
    public class YogaManager : MonoBehaviour
    {
        [Header("Player Setup")]
        public Transform playerHead; // The Main Camera of the XR Origins, used for positioning the poses in front of the player

        [Header("Positionierung")]
        public float spawnDistance = 2.0f; // Value for how far in front of the player the pose should spawn
        public float spawnHeightOffset = 0f; // Optional height offset for the spawned pose (e.g. to align with the floor)

        [Header("Global UI (Optional)")]
        public GameObject yogaCanvas; // The Yoga Menu
        public TextMeshProUGUI timerText; // Global timer

        private YogaSession currentSession;
        private int currentStepIndex = 0;
        private bool isSessionActive = false;
        
        // Aktueller Zustand
        private GameObject currentPoseInstance; // The pose to spawn for the current step
        private YogaPosePrefab currentPoseScript; 
        private float currentHoldTimer = 0f;

        public void StartSession(YogaSession session)
        {
            if (session == null || session.steps.Count == 0) return;

            currentSession = session;
            currentStepIndex = 0;
            isSessionActive = true;
            
            if(yogaCanvas) yogaCanvas.SetActive(true);
            
            SpawnStep(currentStepIndex);
        }

        public void StopSession()
        {
            isSessionActive = false;
            if(yogaCanvas) yogaCanvas.SetActive(false);
            if (currentPoseInstance != null) Destroy(currentPoseInstance);
        }

        void SpawnStep(int index)
        {
            // Delete previous pose instance
            if (currentPoseInstance != null) Destroy(currentPoseInstance);

            // Check if session is complete
            if (index >= currentSession.steps.Count)
            {
                if(timerText) timerText.text = "Session Complete!";
                isSessionActive = false;
                return;
            }

            YogaStep step = currentSession.steps[index];

            // Positioning 
            // Using player head position to spawn the pose in front of the playrt
            Vector3 forward = new Vector3(playerHead.forward.x, 0, playerHead.forward.z).normalized;
            Vector3 spawnPos = playerHead.position + (forward * spawnDistance);
            
            spawnPos.y = spawnHeightOffset; 

            
            currentPoseInstance = Instantiate(step.posePrefab, spawnPos, Quaternion.identity);
            
            // Rotation to player
            Vector3 lookTarget = playerHead.position;
            lookTarget.y = currentPoseInstance.transform.position.y;
            currentPoseInstance.transform.LookAt(lookTarget);

            // Get the YogaPosePrefab script from the spawned instance for later use
            currentPoseScript = currentPoseInstance.GetComponent<YogaPosePrefab>();
            currentHoldTimer = 0;
        }

        void Update()
        {
            if (!isSessionActive || currentPoseScript == null) return;

            // Check if the pose is currently correct
            if (currentPoseScript.IsPoseValid())
            {
                // timer counts up
                currentHoldTimer += Time.deltaTime;
                float remainingTime = currentPoseScript.holdDuration - currentHoldTimer;

                // UI Update
                if(timerText) timerText.text = remainingTime.ToString("F1");

                // if finsihed, go to next step
                if (currentHoldTimer >= currentPoseScript.holdDuration)
                {
                    currentStepIndex++;
                    SpawnStep(currentStepIndex);
                }
            }
            else
            {
                // If not in correct pose, reset timer and update UI
                if(timerText) timerText.text = "Pose einnehmen!";
                currentHoldTimer = 0; // Timer reset if pose is not correct (optional)
            }
        }
    }
}