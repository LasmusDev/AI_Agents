/* using UMA;
using UnityEditor;
using UnityEngine;

namespace PlayerPoseEngine.Scripts {
    
    public class PlayerPoseRecorder : MonoBehaviour
    {
        public bool recordHead;
        public bool recordHands;
        public bool recordFeet;
        public float poseTolerance;
        public string poseName;

        public GameObject root;
        public GameObject headObject;
        public GameObject lHandObject;
        public GameObject rHandObject;
        public GameObject lFootObject;
        public GameObject rFootObject;
    
        public GameObject UMABase;
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }
    
        // Update is called once per frame
        void Update()
        {
            
        }
    
        /// <summary>
        /// Automatically finds and assigns references for an UMA-character, provided that UMA-Base is set.
        /// </summary>
        public void AutoSetupForUMA()
        {
            headObject = UMABase.transform.FindRecursive("Head").gameObject;
            lHandObject = UMABase.transform.FindRecursive("LeftHand").gameObject;
            rHandObject = UMABase.transform.FindRecursive("RightHand").gameObject;
            lFootObject = UMABase.transform.FindRecursive("LeftFoot").gameObject;
            rFootObject = UMABase.transform.FindRecursive("RightFoot").gameObject;
        }
        
        /// <summary>
        /// Records the current player pose and stores it in the Poses folder. Intended to be triggered by Editorbutton, but also works in code
        /// </summary>
        public void RecordPoseSnapshot()
        {
            PlayerPose playerPose = ScriptableObject.CreateInstance<PlayerPose>();
            playerPose.Init();
            if (recordHead)
            {
                LimbRequirement headRequirement = new LimbRequirement();
                headRequirement.limb = Limb.HEAD;
                headRequirement.relativePos = root.transform.InverseTransformPoint(headObject.transform.position);
                headRequirement.tolerance = poseTolerance;
                playerPose.limbRequirements.Add(headRequirement);
            }
            if (recordHands)
            {
                LimbRequirement rHandReq = new LimbRequirement();
                rHandReq.limb = Limb.RHAND;
                rHandReq.relativePos = root.transform.InverseTransformPoint(rHandObject.transform.position);
                rHandReq.tolerance = poseTolerance;
                playerPose.limbRequirements.Add(rHandReq);
                LimbRequirement lHandReq = new LimbRequirement();
                lHandReq.limb = Limb.LHAND;
                lHandReq.relativePos = root.transform.InverseTransformPoint(lHandObject.transform.position);
                lHandReq.tolerance = poseTolerance;
                playerPose.limbRequirements.Add(lHandReq);
            }
            if (recordFeet)
            {
                LimbRequirement rFootReq = new LimbRequirement();
                rFootReq.limb = Limb.RFOOT;
                rFootReq.relativePos = root.transform.InverseTransformPoint(rFootObject.transform.position);
                rFootReq.tolerance = poseTolerance;
                playerPose.limbRequirements.Add(rFootReq);
                LimbRequirement lFootReq = new LimbRequirement();
                lFootReq.limb = Limb.LFOOT;
                lFootReq.relativePos = root.transform.InverseTransformPoint(lFootObject.transform.position);
                lFootReq.tolerance = poseTolerance;
                playerPose.limbRequirements.Add(lFootReq);
            }
            //Ignore asset database warning. It wont work outside of the editor, but it wont need to.
           if(AssetDatabase.AssetPathExists("Assets/Poses/" + poseName + ".asset"))
            {
                poseName += "_";
            } 
            AssetDatabase.CreateAsset(playerPose, "Assets/Poses/" + poseName + ".asset");
        }
    }
    
}
  */









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




