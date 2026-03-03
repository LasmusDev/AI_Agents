using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PlayerPoseEngine.Scripts;
using System.Collections.Generic;

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

        [Header("Visual Feedback (Orbs)")]
        public GameObject headTargetSphere;
        public GameObject lHandTargetSphere;
        public GameObject rHandTargetSphere;

        [Header("Yoga Sessions")]
        public List<YogaSession> availableSessions;
        
        private YogaSession currentSession;
        private int currentStepIndex = 0;
        private bool isSessionActive = false;
        private float currentHoldTimer = 0f;
        
        private GameObject currentTeacherVisual;

        //The Anchoring Points for the targets
        private Vector3 lockedFloorPos;
        private Quaternion lockedLookRot;

        public void StartSession(YogaSession session)
        {
            if (session == null || session.steps.Count == 0) return;
            currentSession = session;
            currentStepIndex = 0;
            isSessionActive = true;
            if(yogaCanvas) yogaCanvas.SetActive(true);
            
            ShowStep(currentStepIndex);
        }

        public void StopSession()
        {
            isSessionActive = false;
            if(yogaCanvas) yogaCanvas.SetActive(false);
            if(currentTeacherVisual) Destroy(currentTeacherVisual);

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

            //Anchor of player 
            lockedFloorPos = new Vector3(playerHead.position.x, playerRoot.position.y, playerHead.position.z);
            lockedLookRot = Quaternion.Euler(0, playerHead.eulerAngles.y, 0);

            if (step.teacherVisualPrefab != null)
            {
                //Visual Yoga Prefab Pose 
                Vector3 spawnPos = lockedFloorPos + (lockedLookRot * Vector3.forward * 2.0f);
                currentTeacherVisual = Instantiate(step.teacherVisualPrefab, spawnPos, Quaternion.identity);
                currentTeacherVisual.transform.LookAt(lockedFloorPos);
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

            if (headTargetSphere) headTargetSphere.SetActive(false);
            if (lHandTargetSphere) lHandTargetSphere.SetActive(false);
            if (rHandTargetSphere) rHandTargetSphere.SetActive(false);

            // Head alignment part for pose fixing
            Vector3 recordedHeadFloorOffset = Vector3.zero;
            foreach (var req in pose.limbRequirements)
            {
                if (req.limb == Limb.HEAD)
                {
                    recordedHeadFloorOffset = new Vector3(req.relativePos.x, 0, req.relativePos.z);
                    break;
                }
            }

            
            //Rotation fix for pose matching
            Quaternion recordedRot = Quaternion.Euler(0, pose.recordedLookAngleY, 0);

            foreach (var req in pose.limbRequirements)
            {
                Transform playerLimb = GetPlayerLimb(req.limb);
                GameObject visSphere = GetVisSphere(req.limb);

                if (playerLimb == null) continue;

               Vector3 centeredOriginalPos = req.relativePos - recordedHeadFloorOffset;
                
               Vector3 localPos = Quaternion.Inverse(recordedRot) * centeredOriginalPos;

               Vector3 finalWorldPos = lockedFloorPos + (lockedLookRot * localPos);

                if (visSphere != null)
                {
                    visSphere.SetActive(true);
                    
                    // Orb spawn position with corretion for head movement and rotation
                    visSphere.transform.position = finalWorldPos; 
                    
                    // Orb rotation to always face the player
                    visSphere.transform.rotation = lockedLookRot;
                    
                    visSphere.transform.localScale = Vector3.one * (req.tolerance * 2);

                    float dist = Vector3.Distance(playerLimb.position, visSphere.transform.position);
                    bool isLimbCorrect = dist <= req.tolerance;

                    if (!isLimbCorrect) allFulfilled = false; 

                    Renderer r = visSphere.GetComponent<Renderer>();
                    if (r != null)
                    {
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
        
        //Start Session by Index or Name 
        public void StartSession(string sessionName)
        {
            YogaSession session = availableSessions.Find(s => s.name == sessionName || s.sessionName == sessionName);
            if (session != null) StartSession(session);
        }

        public void StartSession(int index)
        {
            if (index >= 0 && index < availableSessions.Count) StartSession(availableSessions[index]);
        }
    }
}