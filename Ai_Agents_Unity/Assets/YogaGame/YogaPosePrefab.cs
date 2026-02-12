using UnityEngine;
using TMPro; 
using System.Collections.Generic;

namespace YogaGame
{
    public class YogaPosePrefab : MonoBehaviour
    {
        [Header("Pose Daten")]
        public float holdDuration = 10f; // Timer for the pose
        public string instructionTextContent; // Instructions to show on the prefab

        [Header("Referenzen")]
        public TextMeshProUGUI instructionLabel; // Optional: A label on the prefab to show instructions
        public List<YogaTriggerZone> triggerZones; // List of trigger zones that must be active for the pose to be correct

        void Start()
        {
            // Setting the instruction text on the prefab if there's a label assigned
            if (instructionLabel) instructionLabel.text = instructionTextContent;
        }

        // Actual pose validation logic: checks if all trigger zones are active
        public bool IsPoseValid()
        {
            foreach (var zone in triggerZones)
            {
                if (!zone.isTriggered) return false;
            }
            return true;
        }
    }
}