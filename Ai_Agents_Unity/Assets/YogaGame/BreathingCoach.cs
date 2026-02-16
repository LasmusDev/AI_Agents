using UnityEngine;
using UnityEngine.XR; // Für Haptik

namespace YogaGame
{
    public class BreathingCoach : MonoBehaviour
    {
        [Header("Settings")]
        public float breatheDuration = 4.0f; // 4 sec in, 4 sec out
        public bool isActive = false;

        [Header("Visuals")]
        public Transform visualSphere; // Sphere grows and shrinks
        public float minSize = 0.5f;
        public float maxSize = 1.5f;

        private float timer;

        public void ToggleBreathing(bool state)
        {
            isActive = state;
            if(visualSphere) visualSphere.gameObject.SetActive(state);
        }

        void Update()
        {
            if (!isActive) return;

            timer += Time.deltaTime;

            // Calculate phase of breathing cycle
            // Sinusoidal for smooth in and out
            float phase = (Mathf.Sin(timer * (2 * Mathf.PI / (breatheDuration * 2))) + 1f) / 2f;
            
            // Sphere breathing visual
            if (visualSphere)
            {
                float currentScale = Mathf.Lerp(minSize, maxSize, phase);
                visualSphere.localScale = Vector3.one * currentScale;
            }

            // Haptic: vibrate based on breathing
            // phase 0 = exhaled (shrunk), phase 1 = inhaled (full)
            
            float hapticStrength = phase * 0.5f; // Max 50% strength
            TriggerHaptics(hapticStrength);
        }

        void TriggerHaptics(float strength)
        {
            // Send haptic feedback to both controllers
            SendImpulse(XRNode.LeftHand, strength);
            SendImpulse(XRNode.RightHand, strength);
        }

        void SendImpulse(XRNode node, float amplitude)
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(node);
            if (device.isValid)
            {
                // short impulse
                device.SendHapticImpulse(0, amplitude, 0.05f); 
            }
        }
        

        //To cancel the breathing exercise 
        public void AbortSession()
        {
            if (!isActive) return;

            Debug.Log("Breathing exercise cancelled manually.");
            
            // Flag Session as inactive to stop update loop and haptics
            isActive = false;
            
            // Stop Sphere visuals
            if (visualSphere) visualSphere.gameObject.SetActive(false);
            
            // Reset timer for next session
            timer = 0;
        }
    }
}