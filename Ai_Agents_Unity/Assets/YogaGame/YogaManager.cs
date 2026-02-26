using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PlayerPoseEngine.Scripts;
using System.Collections.Generic;
using JetBrains.Annotations;


namespace YogaGame
{
    public class YogaManager : MonoBehaviour
    {
        [Header("Player Tracking")]
        public Transform playerRoot; 
        public Transform playerHead;
        public Transform playerLeftHand;
        public Transform playerRightHand;
        
        [Header("UI")]
        public GameObject yogaCanvas;
        public TextMeshProUGUI instructionText;
        public TextMeshProUGUI timerText;
        public Image progressCircle;

      
        [Header("Visual Feedback (Optional)")]
        public GameObject headTargetSphere;
        public GameObject lHandTargetSphere;
        public GameObject rHandTargetSphere;
     
        
        private YogaSession currentSession;
        private int currentStepIndex = 0;
        private bool isSessionActive = false;
        private float currentHoldTimer = 0f;
        private GameObject currentTeacherVisual;

        [Header("Yoga Sessions")]
        public List<YogaSession> availableSessions;
        
 
        //Start a session by passing in a YogaSession object
        public void StartSession(YogaSession session)
        {
            if (session == null || session.steps.Count == 0) return;
            currentSession = session;
            currentStepIndex = 0;
            isSessionActive = true;
            if(yogaCanvas) yogaCanvas.SetActive(true);
            
            ShowStep(currentStepIndex);
        }
        //Overload to start session by name
        public void StartSession(string sessionName)
        {
            YogaSession session = availableSessions.Find(s => 
                                  s.name == sessionName || 
                                  s.sessionName == sessionName);
            if (session != null)
            {
                StartSession(session);
            }
            else
                {
                    Debug.LogWarning("Yoga session not found: " + sessionName);
                }
            }
            //Overload to start session by index in the list
            public void StartSession(int index)
            {
                if (index >= 0 && index < availableSessions.Count)
                {
                        StartSession(availableSessions[index]);
                }
                else
                    {
                        Debug.LogWarning("Yoga session index out of range: " + index);
                    }
                
            }
        

        public void StopSession()
        {
            isSessionActive = false;
            if(yogaCanvas) yogaCanvas.SetActive(false);
            if(currentTeacherVisual) Destroy(currentTeacherVisual);

            //If used hide visuals for hands and head
            if (headTargetSphere) headTargetSphere.SetActive(false);
            if (lHandTargetSphere) lHandTargetSphere.SetActive(false);
            if (rHandTargetSphere) rHandTargetSphere.SetActive(false);
        }

        void ShowStep(int index)
        {
            if (currentTeacherVisual != null) Destroy(currentTeacherVisual);

            if (index >= currentSession.steps.Count)
            {
                if(instructionText) instructionText.text = "Session complete!";
                StopSession();
                return;
            }

            YogaStep step = currentSession.steps[index];
            if(instructionText) instructionText.text = step.instruction;
            currentHoldTimer = 0;

            if (step.teacherVisualPrefab != null)
            {
                Vector3 forwardOnFloor = new Vector3(playerHead.forward.x, 0, playerHead.forward.z).normalized;
                Vector3 spawnPos = playerRoot.position + (forwardOnFloor * 2.0f);
                spawnPos.y = playerRoot.position.y; 

                currentTeacherVisual = Instantiate(step.teacherVisualPrefab, spawnPos, Quaternion.identity);
                currentTeacherVisual.transform.LookAt(playerRoot);
            }
        }

        void Update()
        {
            if (!isSessionActive) return;

            YogaStep currentStep = currentSession.steps[currentStepIndex];
            
            bool isInPose = CheckPose(currentStep.poseData);

            if (isInPose)
            {
                currentHoldTimer += Time.deltaTime;
                float remainingTime = currentStep.holdDuration - currentHoldTimer;
                if(timerText) timerText.text = remainingTime.ToString("F1") + "s";
                
                if (progressCircle) 
                {
                    progressCircle.fillAmount = currentHoldTimer / currentStep.holdDuration;
                    progressCircle.color = Color.green;
                }

                if (currentHoldTimer >= currentStep.holdDuration)
                {
                    currentStepIndex++;
                    ShowStep(currentStepIndex);
                }
            }
            else
            {
                if(timerText) timerText.text = "Take the pose!";
                if (progressCircle) 
                {
                    progressCircle.fillAmount = 0;
                    progressCircle.color = Color.red;
                }
            }
        }

        
        bool CheckPose(PlayerPose pose)
        {
            if (pose == null) return false;
            bool allFulfilled = true;

            //Reset visuals for hands and head
            if (headTargetSphere) headTargetSphere.SetActive(false);
            if (lHandTargetSphere) lHandTargetSphere.SetActive(false);
            if (rHandTargetSphere) rHandTargetSphere.SetActive(false);

            foreach (var req in pose.limbRequirements)
            {
                Transform playerLimb = GetPlayerLimb(req.limb);
                GameObject visSphere = GetVisSphere(req.limb);

                if (playerLimb == null) continue;

                //Calculate position of the target in world space
                Vector3 targetWorldPos = playerRoot.TransformPoint(req.relativePos);
                
                //Calculate distance between player's limb and target position
                float dist = Vector3.Distance(playerLimb.position, targetWorldPos);
                bool isLimbCorrect = dist <= req.tolerance;

                if (!isLimbCorrect) 
                {
                    allFulfilled = false; 
                }

                //Set up visuals for this limb
                if (visSphere != null)
                {
                    visSphere.SetActive(true);
                    visSphere.transform.position = targetWorldPos;
                    
                    //Set scale based on tolerance (optional, for better visibility)
                    visSphere.transform.localScale = Vector3.one * (req.tolerance * 2);

                    //Color the sphere green if correct, red if not (with some transparency)
                    Renderer r = visSphere.GetComponent<Renderer>();
                    if (r != null)
                    {
                        //Color the sphere based on whether the limb is correct or not
                        r.material.color = isLimbCorrect ? new Color(0, 1, 0, 0.4f) : new Color(1, 0, 0, 0.4f);
                    }
                }
            }
            return allFulfilled;
        }

        Transform GetPlayerLimb(Limb limbType)
        {
            switch(limbType)
            {
                case Limb.HEAD: return playerHead;
                case Limb.LHAND: return playerLeftHand;
                case Limb.RHAND: return playerRightHand;
                default: return null;
            }
        }

        GameObject GetVisSphere(Limb limbType)
        {
            switch(limbType)
            {
                case Limb.HEAD: return headTargetSphere;
                case Limb.LHAND: return lHandTargetSphere;
                case Limb.RHAND: return rHandTargetSphere;
                default: return null;
            }
        }
    }
}