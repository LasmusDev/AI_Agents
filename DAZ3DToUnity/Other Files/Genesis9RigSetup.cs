using UnityEngine;
using UnityEditor;
using UnityEngine.Animations.Rigging;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using System.Reflection;

namespace Editor {
    
    public class Genesis9RigSetup : EditorWindow
    {
        [MenuItem("Tools/Setup Genesis 9 VR Rig Full")]
        public static void SetupRig()
        {
            GameObject avatarRoot = Selection.activeGameObject;
    
            if (avatarRoot == null)
            {
                EditorUtility.DisplayDialog("Fehler", "Bitte wähle zuerst das Genesis 9 Avatar-Root-Objekt aus.", "OK");
                return;
            }
    
            Undo.RegisterCreatedObjectUndo(avatarRoot, "Setup VR Rig Full");
    
            //CHARACTER CONTROLLER 
            CharacterController cc = avatarRoot.GetComponent<CharacterController>();
            if (cc == null) cc = avatarRoot.AddComponent<CharacterController>();
            cc.slopeLimit = 45f;
            cc.stepOffset = 0.3f;
            cc.skinWidth = 0.08f;
            cc.minMoveDistance = 0.001f;
            cc.center = Vector3.zero;
            cc.radius = 0.5f;
            cc.height = 2f;
    
            //KNOCHEN FINDEN 
            Transform lUpperArm = FindDeepChild(avatarRoot.transform, "l_upperarm");
            Transform lForearm = FindDeepChild(avatarRoot.transform, "l_forearm");
            Transform lHand = FindDeepChild(avatarRoot.transform, "l_hand");
            Transform rUpperArm = FindDeepChild(avatarRoot.transform, "r_upperarm");
            Transform rForearm = FindDeepChild(avatarRoot.transform, "r_forearm");
            Transform rHand = FindDeepChild(avatarRoot.transform, "r_hand");
            Transform lThigh = FindDeepChild(avatarRoot.transform, "l_thigh");
            Transform lShin = FindDeepChild(avatarRoot.transform, "l_shin");
            Transform lFoot = FindDeepChild(avatarRoot.transform, "l_foot");
            Transform rThigh = FindDeepChild(avatarRoot.transform, "r_thigh");
            Transform rShin = FindDeepChild(avatarRoot.transform, "r_shin");
            Transform rFoot = FindDeepChild(avatarRoot.transform, "r_foot");
            Transform head = FindDeepChild(avatarRoot.transform, "head");
    
            if (lHand == null || rHand == null || head == null)
            {
                EditorUtility.DisplayDialog("Fehler", "Konnte Genesis 9 Knochen nicht finden.", "OK");
                return;
            }
    
            //RIG CONTAINER
            GameObject rigObj = new GameObject("VR IK Rig");
            rigObj.transform.SetParent(avatarRoot.transform, false); 
            
            GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
            float rigY = (xrOrigin != null) ? 0.25f : 0f;
            rigObj.transform.localPosition = new Vector3(0, rigY, 0);
            rigObj.transform.localRotation = Quaternion.identity;
            rigObj.transform.localScale = Vector3.one;
    
            Rig rigComponent = rigObj.AddComponent<Rig>();
            Undo.RegisterCreatedObjectUndo(rigObj, "Create Rig Container");
    
            //BONE RENDERER
            if (avatarRoot.GetComponent<BoneRenderer>() == null)
            {
                BoneRenderer boneRenderer = avatarRoot.AddComponent<BoneRenderer>();
                Transform hips = FindDeepChild(avatarRoot.transform, "hip");
                List<Transform> allBones = new List<Transform>();
                if (hips != null) { allBones.Add(hips); AddAllChildrenRecursive(hips, allBones); }
                else { AddAllChildrenRecursive(avatarRoot.transform, allBones); }
                boneRenderer.transforms = allBones.ToArray();
            }
    
            //IK SETUP
            var leftArmParts = SetupTwoBoneIK(rigObj.transform, "Left Arm IK", lUpperArm, lForearm, lHand, true);
            var rightArmParts = SetupTwoBoneIK(rigObj.transform, "Right Arm IK", rUpperArm, rForearm, rHand, true);
            var leftLegParts = SetupTwoBoneIK(rigObj.transform, "Left Leg IK", lThigh, lShin, lFoot, false);
            var rightLegParts = SetupTwoBoneIK(rigObj.transform, "Right Leg IK", rThigh, rShin, rFoot, false);
            var headParts = SetupHeadIK(rigObj.transform, head);
    
            //FOOT SOLVER SCRIPT
            Component leftSolver = AddScriptIfFound(leftLegParts.target, "IKFootSolver", null);
            Component rightSolver = AddScriptIfFound(rightLegParts.target, "IKFootSolver", null);
            if (leftSolver != null && rightSolver != null)
            {
                int defaultLayerMask = 1 << LayerMask.NameToLayer("Default"); //Damit layer direkt auf Default gesetzt werden kannn (Unity LayerMask Setup)
    
                //Linker Fuß
                SetField(leftSolver, "Body", avatarRoot.transform);
                SetField(leftSolver, "OtherFoot", rightSolver);
                SetField(leftSolver, "TerrainLayer", defaultLayerMask); //Das eigentliche Setzen des Terrain Layers auf Defaut (für linken Fuß)
                SetField(leftSolver, "Speed", 4f);
                SetField(leftSolver, "StepDistance", 0.2f);
                SetField(leftSolver, "StepLength", 0.2f);
                SetField(leftSolver, "StepHeight", 0.3f);
                SetField(leftSolver, "FootOffset", Vector3.zero);
    
                //Rechter Fuß
                SetField(rightSolver, "Body", avatarRoot.transform);
                SetField(rightSolver, "OtherFoot", leftSolver);
                SetField(rightSolver, "TerrainLayer", defaultLayerMask); //Das eigentliche Setzen des Terrain Layers auf Defaut (für rechten Fuß)
                SetField(rightSolver, "Speed", 4f);
                SetField(rightSolver, "StepDistance", 0.2f);
                SetField(rightSolver, "StepLength", 0.2f);
                SetField(rightSolver, "StepHeight", 0.3f);
                SetField(rightSolver, "FootOffset", Vector3.zero);
            }
    
            //AUTO SETUP & WERTE ÜBERSCHREIBEN
            AutoAlignConstraint(leftArmParts.constraint);
            AutoAlignConstraint(rightArmParts.constraint);
            AutoAlignConstraint(leftLegParts.constraint);
            AutoAlignConstraint(rightLegParts.constraint);
        
            ApplyHardcodedTransforms(leftArmParts, rightArmParts, leftLegParts, rightLegParts, headParts);
    
            //RIG BUILDER 
            RigBuilder rigBuilder = avatarRoot.GetComponent<RigBuilder>();
            if (rigBuilder == null) rigBuilder = avatarRoot.AddComponent<RigBuilder>();
            rigBuilder.layers = new List<RigLayer> { new RigLayer(rigComponent) };
    
            //IK FOLLOW SCRIPT
            var followScript = avatarRoot.GetComponent<IKTargetFollowVRRig>();
            if (followScript == null) followScript = avatarRoot.AddComponent<IKTargetFollowVRRig>();
    
            followScript.turnSmoothness = 0.1f;
            //Head Body Offset 
            followScript.headBodyPositionOffset = new Vector3(0, -0.6f, 0); 
    
            followScript.head = new VRMap(); 
            followScript.head.ikTarget = headParts.target.transform;
            //Tracking Offset 
            followScript.head.trackingPositionOffset = new Vector3(0, 0, -0.12f); 
            if (Camera.main != null) followScript.head.vrTarget = Camera.main.transform;
    
            followScript.leftHand = new VRMap();
            followScript.leftHand.ikTarget = leftArmParts.target.transform;
            GameObject realLeft = GameObject.Find("Left Controller");
            if (realLeft) followScript.leftHand.vrTarget = realLeft.transform;
    
            followScript.rightHand = new VRMap();
            followScript.rightHand.ikTarget = rightArmParts.target.transform;
            GameObject realRight = GameObject.Find("Right Controller");
            if (realRight) followScript.rightHand.vrTarget = realRight.transform;
    
            //ANIMATE ON INPUT Script 
            AddScriptIfFound(avatarRoot, "AnimateOnInput", (animInputComp) => {
                SerializedObject so = new SerializedObject(animInputComp);
                
                SerializedProperty animProp = so.FindProperty("animator");
                if (animProp == null) animProp = so.FindProperty("Animator");
                if (animProp == null) animProp = so.FindProperty("handAnimator");
                if (animProp != null) animProp.objectReferenceValue = avatarRoot.GetComponent<Animator>();
    
                SerializedProperty inputsList = so.FindProperty("AnimationInputs");
                if (inputsList == null) inputsList = so.FindProperty("animationInputs");
    
                if (inputsList != null)
                {
                    inputsList.arraySize = 4;
                    SetInputEntry(inputsList, 0, "Right Pinch", "Right", "Activate Value");
                    SetInputEntry(inputsList, 1, "Right Grab", "Right", "Select Value");
                    SetInputEntry(inputsList, 2, "Left Pinch", "Left", "Activate Value");
                    SetInputEntry(inputsList, 3, "Left Grab", "Left", "Select Value");
                }
                so.ApplyModifiedProperties();
            }); 
    
            Debug.Log("Genesis 9 VR Rig komplett eingerichtet!");
        }
    
        
    
        static void AutoAlignConstraint(TwoBoneIKConstraint constraint)
        {
            if (constraint.data.root != null && constraint.data.tip != null && constraint.data.target != null)
            {
                constraint.data.target.position = constraint.data.tip.position;
                constraint.data.target.rotation = constraint.data.tip.rotation;
                Vector3 a = constraint.data.root.position;
                Vector3 b = constraint.data.mid.position;
                Vector3 c = constraint.data.tip.position;
                Vector3 cross = Vector3.Cross(c - a, b - a);
                if (cross != Vector3.zero)
                {
                    Vector3 hintPos = b + Vector3.Cross(cross, c - a).normalized;
                    constraint.data.hint.position = hintPos;
                }
            }
        }
    
        static void ApplyHardcodedTransforms(
            (GameObject go, GameObject target, GameObject hint, TwoBoneIKConstraint constraint) lArm,
            (GameObject go, GameObject target, GameObject hint, TwoBoneIKConstraint constraint) rArm,
            (GameObject go, GameObject target, GameObject hint, TwoBoneIKConstraint constraint) lLeg,
            (GameObject go, GameObject target, GameObject hint, TwoBoneIKConstraint constraint) rLeg,
            (GameObject go, GameObject target) head)
        {
            //HEAD 
            head.target.transform.localPosition = new Vector3(0f, 1.533122f, -0.006061039f);
            head.target.transform.localRotation = Quaternion.identity;
    
            //RIGHT ARM 
            rArm.target.transform.localPosition = new Vector3(0.5047787f, 1.000019f, 0.02151215f);
            rArm.target.transform.localRotation = Quaternion.identity;
            rArm.hint.transform.localPosition = new Vector3(0.228f, 0f, -0.014f); //Dami die Hints korrekt sind
    
            //LEFT ARM 
            lArm.target.transform.localPosition = new Vector3(-0.5047652f, 1.000019f, 0.02151293f);
            lArm.target.transform.localRotation = Quaternion.identity;
            lArm.hint.transform.localPosition = new Vector3(-0.228f, 0f, -0.014f); //Damir die Hints korrekt sind
    
            //RIGHT LEG
            rLeg.target.transform.localPosition = new Vector3(0.1100601f, -0.185502f, -0.04647938f);
            rLeg.target.transform.localRotation = Quaternion.identity;
            rLeg.hint.transform.localPosition = new Vector3(0.258f, -0.035f, 1.903f);
    
            //LEFT LEG 
            lLeg.target.transform.localPosition = new Vector3(-0.1100601f, -0.1855021f, -0.04647938f);
            lLeg.target.transform.localRotation = Quaternion.identity;
            lLeg.hint.transform.localPosition = new Vector3(-0.209f, 0.014f, 1.956f);
        }
    
        static Type FindType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                Type t = assembly.GetType(typeName);
                if (t != null) return t;
            }
            return null;
        }
    
        static Component AddScriptIfFound(GameObject targetObj, string className, Action<Component> configureAction)
        {
            Type t = FindType(className);
            if (t != null)
            {
                Component comp = targetObj.GetComponent(t);
                if (comp == null) comp = targetObj.AddComponent(t);
                configureAction?.Invoke(comp);
                return comp;
            }
            return null;
        }
    
        static void SetField(object instance, string fieldName, object value)
        {
            if (instance == null) return;
            Type type = instance.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) field = type.GetField(char.ToLower(fieldName[0]) + fieldName.Substring(1), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            
            if (field != null) { 
                // SPECIAL HANDLING FOR LAYERMASK
                if (field.FieldType == typeof(LayerMask) && value is int) {
                    LayerMask mask = new LayerMask();
                    mask.value = (int)value;
                    field.SetValue(instance, mask);
                } else {
                    try { field.SetValue(instance, value); } catch { } 
                }
            }
        }
    
        static void SetInputEntry(SerializedProperty list, int index, string name, string handSide, string actionName)
        {
            SerializedProperty element = list.GetArrayElementAtIndex(index);
            SerializedProperty nameProp = element.FindPropertyRelative("AnimationPropertyName");
            if (nameProp == null) nameProp = element.FindPropertyRelative("animationPropertyName");
            if (nameProp != null) nameProp.stringValue = name;
    
            SerializedProperty actionProp = element.FindPropertyRelative("Action");
            if (actionProp == null) actionProp = element.FindPropertyRelative("action");
            if (actionProp != null)
            {
                InputActionReference actionRef = FindActionReferenceRecursive(handSide, actionName);
                if (actionRef != null) {
                    SerializedProperty useReference = actionProp.FindPropertyRelative("m_UseReference");
                    if (useReference != null) useReference.boolValue = true;
                    SerializedProperty refProp = actionProp.FindPropertyRelative("m_Reference");
                    if (refProp != null) refProp.objectReferenceValue = actionRef;
                }
            }
        }
    
        static InputActionReference FindActionReferenceRecursive(string handSide, string actionName)
        {
            string[] guids = AssetDatabase.FindAssets("t:InputActionReference");
            foreach (string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var obj in assets) {
                    if (obj is InputActionReference actionRef) {
                        string refName = actionRef.name;
                        string actionMap = actionRef.action != null && actionRef.action.actionMap != null ? actionRef.action.actionMap.name : "";
                        bool nameMatch = refName.IndexOf(actionName, StringComparison.OrdinalIgnoreCase) >= 0;
                        bool sideMatch = (actionMap.Contains(handSide) || refName.Contains(handSide));
                        if (nameMatch && sideMatch) return actionRef;
                    }
                }
            }
            return null;
        }
    
        static void AddAllChildrenRecursive(Transform parent, List<Transform> list)
        {
            foreach (Transform child in parent) {
                list.Add(child);
                AddAllChildrenRecursive(child, list);
            }
        }
    
        static Transform FindDeepChild(Transform aParent, string aName)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while (queue.Count > 0) {
                var c = queue.Dequeue();
                if (c.name == aName) return c;
                foreach (Transform t in c) queue.Enqueue(t);
            }
            return null;
        }
    
        static (GameObject go, GameObject target, GameObject hint, TwoBoneIKConstraint constraint) SetupTwoBoneIK(Transform parent, string name, Transform root, Transform mid, Transform tip, bool isArm)
        {
            GameObject ikObj = new GameObject(name);
            ikObj.transform.SetParent(parent, false);
            GameObject target = new GameObject(name + "_target");
            target.transform.SetParent(ikObj.transform, false); 
            GameObject hint = new GameObject(name + "_hint");
            hint.transform.SetParent(ikObj.transform, false); 
            TwoBoneIKConstraint constraint = ikObj.AddComponent<TwoBoneIKConstraint>();
            constraint.data.root = root;
            constraint.data.mid = mid;
            constraint.data.tip = tip;
            constraint.data.target = target.transform;
            constraint.data.hint = hint.transform;
            constraint.data.targetPositionWeight = 1f;
            constraint.data.targetRotationWeight = 1f;
            constraint.data.hintWeight = 1f;
            RigBuilder rb = ikObj.GetComponentInParent<RigBuilder>();
            if(rb) rb.Build();
            return (ikObj, target, hint, constraint);
        }
    
        static (GameObject go, GameObject target) SetupHeadIK(Transform parent, Transform headBone)
        {
            GameObject ikObj = new GameObject("Head IK");
            ikObj.transform.SetParent(parent, false);
            GameObject target = new GameObject("Head Target");
            target.transform.SetParent(ikObj.transform, false);
            MultiParentConstraint constraint = ikObj.AddComponent<MultiParentConstraint>();
            constraint.data.constrainedObject = headBone;
            var sourceObjects = constraint.data.sourceObjects;
            sourceObjects.Add(new WeightedTransform(target.transform, 1f));
            constraint.data.sourceObjects = sourceObjects;
            return (ikObj, target);
        }
    }
} 

