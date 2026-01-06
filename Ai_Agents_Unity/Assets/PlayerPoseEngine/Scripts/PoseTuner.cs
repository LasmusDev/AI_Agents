using UnityEngine;
using System.Collections.Generic;

namespace PlayerPoseEngine.Scripts 
{
    public class PoseTuner : MonoBehaviour
    {
        [Header("Setup")]
        public Transform playerRoot;
        public Transform headCamera; 

        [Header("Editor")]
        public PlayerPose poseToEdit; 
        public float visualSize = 0.15f; 

        
        private GameObject g_Head;
        private GameObject g_LHand;
        private GameObject g_RHand;

        void Update()
        {
            if (poseToEdit == null || playerRoot == null) return;

          
            if (g_Head == null) g_Head = CreateGhost("GHOST_Head", Color.yellow);
            if (g_LHand == null) g_LHand = CreateGhost("GHOST_LHand", Color.cyan);
            if (g_RHand == null) g_RHand = CreateGhost("GHOST_RHand", Color.magenta);

            
            g_Head.SetActive(false);
            g_LHand.SetActive(false);
            g_RHand.SetActive(false);

            
            foreach (var req in poseToEdit.limbRequirements)
            {
                Vector3 targetPos = CalculatePosition(req);

                if (req.limb == Limb.HEAD) PositionGhost(g_Head, targetPos);
                if (req.limb == Limb.LHAND) PositionGhost(g_LHand, targetPos);
                if (req.limb == Limb.RHAND) PositionGhost(g_RHand, targetPos);
            }
        }

        
        Vector3 CalculatePosition(LimbRequirement req)
        {
            Vector3 finalPos = req.relativePos;

            finalPos = playerRoot.TransformPoint(req.relativePos);

            
            if (poseToEdit.intendedHeight > 0.1f && headCamera != null)
            {
                float playerCurrentHeight = headCamera.position.y;
                 
                
                if (playerCurrentHeight < 1.0f) playerCurrentHeight = 1.75f;

                float scaleFactor = playerCurrentHeight / poseToEdit.intendedHeight;

                
                float floorY = playerRoot.position.y;
                float heightFromFloor = finalPos.y - floorY;
                
                finalPos.y = floorY + (heightFromFloor * scaleFactor);
            }

            return finalPos;
        }

        void PositionGhost(GameObject ghost, Vector3 pos)
        {
            ghost.SetActive(true);
            ghost.transform.position = pos;
            ghost.transform.localScale = Vector3.one * visualSize;
        }

        GameObject CreateGhost(string name, Color col)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            Destroy(go.GetComponent<Collider>()); 
            
          
            Renderer r = go.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Standard"));
            r.material.color = new Color(col.r, col.g, col.b, 0.5f); 
            
            go.transform.parent = this.transform;
            return go;
        }
    }
}