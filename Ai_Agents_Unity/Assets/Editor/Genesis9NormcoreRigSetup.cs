using UnityEngine;
using UnityEditor;
using UnityEngine.Animations.Rigging;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using System.Reflection;

namespace Editor {
    
    public class Genesis9NormcoreSetup : EditorWindow
    {
        [MenuItem("Tools/Setup Genesis 9 Normcore Rig (Final)")]
        public static void SetupRig()
        {
            GameObject avatarRoot = Selection.activeGameObject;
    
            if (avatarRoot == null)
            {
                EditorUtility.DisplayDialog("Fehler", "Bitte wähle zuerst das Genesis 9 Avatar-Root-Objekt aus.", "OK");
                return;
            }
    
            Undo.RegisterCreatedObjectUndo(avatarRoot, "Setup Normcore Rig");
    
     
            CharacterController cc = avatarRoot.GetComponent<CharacterController>();
            if (cc == null) cc = avatarRoot.AddComponent<CharacterController>();
            cc.center = Vector3.zero;
            cc.radius = 0.5f;
            cc.height = 2f;

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
    
    
            GameObject rigObj = new GameObject("VR IK Rig");
            rigObj.transform.SetParent(avatarRoot.transform, false); 
            rigObj.transform.localPosition = Vector3.zero; 
    
            Rig rigComponent = rigObj.AddComponent<Rig>();
            Undo.RegisterCreatedObjectUndo(rigObj, "Create Rig Container");
    
          
            if (avatarRoot.GetComponent<BoneRenderer>() == null)
            {
                BoneRenderer boneRenderer = avatarRoot.AddComponent<BoneRenderer>();
                Transform hips = FindDeepChild(avatarRoot.transform, "hip");
                List<Transform> allBones = new List<Transform>();
                if (hips != null) { allBones.Add(hips); AddAllChildrenRecursive(hips, allBones); }
                else { AddAllChildrenRecursive(avatarRoot.transform, allBones); }
                boneRenderer.transforms = allBones.ToArray();
            }
    
            
            var leftArmParts = SetupTwoBoneIK(rigObj.transform, "Left Arm IK", lUpperArm, lForearm, lHand, true);
            var rightArmParts = SetupTwoBoneIK(rigObj.transform, "Right Arm IK", rUpperArm, rForearm, rHand, true);
            var leftLegParts = SetupTwoBoneIK(rigObj.transform, "Left Leg IK", lThigh, lShin, lFoot, false);
            var rightLegParts = SetupTwoBoneIK(rigObj.transform, "Right Leg IK", rThigh, rShin, rFoot, false);
            var headParts = SetupHeadIK(rigObj.transform, head);
    
           
            AutoAlignConstraint(leftArmParts.constraint);
            AutoAlignConstraint(rightArmParts.constraint);
            AutoAlignConstraint(leftLegParts.constraint);
            AutoAlignConstraint(rightLegParts.constraint);
            ApplyHardcodedTransforms(leftArmParts, rightArmParts, leftLegParts, rightLegParts, headParts);
    
            RigBuilder rigBuilder = avatarRoot.GetComponent<RigBuilder>();
            if (rigBuilder == null) rigBuilder = avatarRoot.AddComponent<RigBuilder>();
            rigBuilder.layers = new List<RigLayer> { new RigLayer(rigComponent) };


            Component rtView = AddScriptIfFound(avatarRoot, "Normal.Realtime.RealtimeView", null);
            Component rtTransform = AddScriptIfFound(avatarRoot, "Normal.Realtime.RealtimeTransform", null);
            
           
            
            AddScriptIfFound(headParts.target, "Normal.Realtime.RealtimeTransform", null);
            AddScriptIfFound(leftArmParts.target, "Normal.Realtime.RealtimeTransform", null);
            AddScriptIfFound(rightArmParts.target, "Normal.Realtime.RealtimeTransform", null);
            AddScriptIfFound(leftLegParts.target, "Normal.Realtime.RealtimeTransform", null);
            AddScriptIfFound(rightLegParts.target, "Normal.Realtime.RealtimeTransform", null);


            Component ikFollow = AddScriptIfFound(avatarRoot, "IKTargetFollowVRRig", null);
            
            if (ikFollow != null)
            {
              
                SetField(ikFollow, "realtimeView", rtView);

                SetField(ikFollow, "headBodyPositionOffset", new Vector3(0, -1.65f, 0)); 
                SetField(ikFollow, "turnSmoothness", 0.1f);

                object headMap = GetValue(ikFollow, "head");
                if (headMap != null) {
                    SetField(headMap, "ikTarget", headParts.target.transform);
                   
                    SetField(headMap, "trackingPositionOffset", new Vector3(0, 0, -0.12f));
                }

            
                object leftMap = GetValue(ikFollow, "leftHand");
                if (leftMap != null) {
                    SetField(leftMap, "ikTarget", leftArmParts.target.transform);
                }

              
                object rightMap = GetValue(ikFollow, "rightHand");
                if (rightMap != null) {
                    SetField(rightMap, "ikTarget", rightArmParts.target.transform);
                }
            }
            else
            {
                Debug.LogWarning("Konnte Script 'IKTargetFollowVRRig' nicht finden! Hast du es umbenannt?");
            }

           
            AddScriptIfFound(avatarRoot, "AnimateOnInput", (animInputComp) => {
                SerializedObject so = new SerializedObject(animInputComp);
                SerializedProperty animProp = so.FindProperty("animator");
                if (animProp == null) animProp = so.FindProperty("Animator");
                if (animProp != null) animProp.objectReferenceValue = avatarRoot.GetComponent<Animator>();
                so.ApplyModifiedProperties();
            });

            Debug.Log("Genesis 9 Normcore Rig (mit IKTargetFollow) erfolgreich erstellt!");
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
            head.target.transform.localPosition = new Vector3(0f, 1.533122f, -0.006061039f);
            head.target.transform.localRotation = Quaternion.identity;
    
            rArm.target.transform.localPosition = new Vector3(0.5047787f, 1.000019f, 0.02151215f);
            rArm.target.transform.localRotation = Quaternion.identity;
            rArm.hint.transform.localPosition = new Vector3(0.228f, 0f, -0.014f); 
    
            lArm.target.transform.localPosition = new Vector3(-0.5047652f, 1.000019f, 0.02151293f);
            lArm.target.transform.localRotation = Quaternion.identity;
            lArm.hint.transform.localPosition = new Vector3(-0.228f, 0f, -0.014f);
    
            rLeg.target.transform.localPosition = new Vector3(0.1100601f, -0.185502f, -0.04647938f);
            rLeg.target.transform.localRotation = Quaternion.identity;
            rLeg.hint.transform.localPosition = new Vector3(0.258f, -0.035f, 1.903f);
    
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

        static object GetValue(object instance, string fieldName)
        {
             if (instance == null) return null;
            Type type = instance.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null) return field.GetValue(instance);
            return null;
        }
    
        static void SetField(object instance, string fieldName, object value)
        {
            if (instance == null) return;
            Type type = instance.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) field = type.GetField(char.ToLower(fieldName[0]) + fieldName.Substring(1), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            
            if (field != null) { 
                if (field.FieldType == typeof(LayerMask) && value is int) {
                    LayerMask mask = new LayerMask();
                    mask.value = (int)value;
                    field.SetValue(instance, mask);
                } else {
                    try { field.SetValue(instance, value); } catch { } 
                }
            }
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