using UnityEngine;
using UnityEngine.XR; 
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; 
#endif
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlayerPoseEngine.Scripts 
{
    public class PlayerPoseRecorder : MonoBehaviour
    {
        [Header("Settings")]
        public string poseName = "NewPose";
        public float poseTolerance = 0.15f;
        
        [Header("What to record")]
        public bool recordHead = true;
        public bool recordHands = true;
        public bool recordFeet = false;

        [Header("References")]
        public GameObject root;
        public GameObject headObject;
        public GameObject lHandObject;
        public GameObject rHandObject;
        public GameObject lFootObject;
        public GameObject rFootObject;
        public Transform playerHead;

        
        private bool wasPressedLastFrame = false;

        void Update()
        {
           
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                RecordPoseSnapshot();
            }
#endif

            
            CheckVRInput();
        }

        void CheckVRInput()
        {
            
            UnityEngine.XR.InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            if (device.isValid)
            {
                bool isPressed = false;
                
              
                if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out isPressed))
                {
                    
                    if (isPressed && !wasPressedLastFrame)
                    {
                        RecordPoseSnapshot();
                    }
                    
                    wasPressedLastFrame = isPressed;
                }
            }
        }

        public void RecordPoseSnapshot()
        {
            Debug.Log($"<color=green>SNAPSHOT!</color> Speichere Pose: {poseName}");

            PlayerPose playerPose = ScriptableObject.CreateInstance<PlayerPose>();
            playerPose.recordedLookAngleY = playerHead.eulerAngles.y;
            playerPose.Init();
            
            
            if(headObject != null)
            {
                
                playerPose.intendedHeight = headObject.transform.position.y;
                
               
                if(root != null) 
                {
                    playerPose.intendedHeight = headObject.transform.position.y - root.transform.position.y;
                }
            }

         
            if (recordHead && headObject != null)
            {
                AddLimb(playerPose, Limb.HEAD, headObject);
            }
         
            if (recordHands && rHandObject != null && lHandObject != null)
            {
                AddLimb(playerPose, Limb.RHAND, rHandObject);
                AddLimb(playerPose, Limb.LHAND, lHandObject);
            }
           
            if (recordFeet && rFootObject != null && lFootObject != null)
            {
                AddLimb(playerPose, Limb.RFOOT, rFootObject);
                AddLimb(playerPose, Limb.LFOOT, lFootObject);
            }

#if UNITY_EDITOR
            
            if (!AssetDatabase.IsValidFolder("Assets/Poses"))
            {
                AssetDatabase.CreateFolder("Assets", "Poses");
            }

            string path = "Assets/Poses/" + poseName + ".asset";
           
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            
            AssetDatabase.CreateAsset(playerPose, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"Gespeichert unter: {path} (Höhe: {playerPose.intendedHeight:F2}m)");
#endif
        }

       
        void AddLimb(PlayerPose pose, Limb limbType, GameObject obj)
        {
            LimbRequirement req = new LimbRequirement();
            req.limb = limbType;
            
          
            if (root != null)
                req.relativePos = root.transform.InverseTransformPoint(obj.transform.position);
            else
                req.relativePos = obj.transform.position;

            req.tolerance = poseTolerance;
            pose.limbRequirements.Add(req);
        }
    }
} 




