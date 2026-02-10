using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PlayerPoseEngine.Scripts; 
using System.Collections.Generic;

namespace YogaGame
{
    public class YogaManager : MonoBehaviour
    {
        [Header("Player Setup")]
        public Transform playerHead;
        public Transform playerLeftHand;
        public Transform playerRightHand;

        [Header("Ghost (Lehrer)")]
        public Transform ghostRoot; 
        public Transform ghostHead;
        public Transform ghostLeftHand;
        public Transform ghostRightHand;
        
        [Header("UI")]
        public GameObject yogaCanvas;
        public TextMeshProUGUI instructionText;
        public TextMeshProUGUI timerText;
        public Image progressCircle;

        [Header("Settings")]
        public float allowedError = 0.20f; 
        
        private YogaSession currentSession;
        private int currentStepIndex = 0;
        private bool isSessionActive = false;
        private float currentHoldTimer = 0f;
        
        public void StartSession(YogaSession session)
        {
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
            HideGhost();
        }

        void ShowStep(int index)
        {
            if (index >= currentSession.steps.Count)
            {
                if(instructionText) instructionText.text = "Namaste! Session beendet.";
                isSessionActive = false;
                HideGhost();
                return;
            }

            YogaStep step = currentSession.steps[index];
            if(instructionText) instructionText.text = step.instruction;
            currentHoldTimer = 0;

            //Ghost positioning
            if (ghostRoot)
            {
                Vector3 playerFloorPos = playerHead.position;
                playerFloorPos.y = 0; //ground level
                
                ghostRoot.position = playerFloorPos;
                //Ghost turn to face player
                ghostRoot.rotation = Quaternion.LookRotation(new Vector3(playerHead.forward.x, 0, playerHead.forward.z));
                ghostRoot.gameObject.SetActive(true);
            }

            // gets porobably changed due to avatar replacement
            if(ghostHead) 
            {
                ghostHead.localPosition = GetLimbPosition(step.pose, Limb.HEAD);
                ghostHead.gameObject.SetActive(true);
            }
            if(ghostLeftHand)
            {
                ghostLeftHand.localPosition = GetLimbPosition(step.pose, Limb.LHAND);
                ghostLeftHand.gameObject.SetActive(true);
            }
            if(ghostRightHand)
            {
                ghostRightHand.localPosition = GetLimbPosition(step.pose, Limb.RHAND);
                ghostRightHand.gameObject.SetActive(true);
            }
        }
        
        
        Vector3 GetLimbPosition(PlayerPose pose, Limb limbType)
        {
           
            foreach (var req in pose.limbRequirements)
            {
                if (req.limb == limbType)
                {
                    return req.relativePos;
                }
            }
           
            return Vector3.zero; 
        }

        void HideGhost()
        {
            if(ghostRoot) ghostRoot.gameObject.SetActive(false);
        }

        void Update()
        {
            if (!isSessionActive) return;

            YogaStep currentStep = currentSession.steps[currentStepIndex];
            
            
            bool isInPose = CheckPose();

            if (isInPose)
            {
                currentHoldTimer += Time.deltaTime;
                if(timerText) timerText.text = (currentStep.holdDuration - currentHoldTimer).ToString("F1") + "s";
                
                if (progressCircle) 
                    progressCircle.fillAmount = currentHoldTimer / currentStep.holdDuration;

                if (currentHoldTimer >= currentStep.holdDuration)
                {
                    currentStepIndex++;
                    ShowStep(currentStepIndex);
                }
            }
            else
            {
                if(timerText) timerText.text = "Hold the pose!";
                if (progressCircle) progressCircle.color = Color.red;
            }

            if (isInPose && progressCircle) progressCircle.color = Color.green;
        }

        bool CheckPose()
        {
            
            
            float distL = Vector3.Distance(playerLeftHand.position, ghostLeftHand.position);
            float distR = Vector3.Distance(playerRightHand.position, ghostRightHand.position);
         
            float distHead = Vector3.Distance(playerHead.position, ghostHead.position);

            return distL < allowedError && distR < allowedError && distHead < allowedError;
        }
    }
}