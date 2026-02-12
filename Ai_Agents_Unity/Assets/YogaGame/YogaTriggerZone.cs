using UnityEngine;

namespace YogaGame
{
    public class YogaTriggerZone : MonoBehaviour
    {
        [Header("Einstellung")]
        public string requiredTag = "PlayerLeftHand"; 
        public bool isTriggered = false;

        private Renderer rend;
        private Color defaultColor;
        public Color successColor = Color.green;

        void Awake()
        {
            rend = GetComponent<Renderer>();
            if(rend) defaultColor = rend.material.color;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(requiredTag))
            {
                isTriggered = true;
                if(rend) rend.material.color = successColor;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(requiredTag))
            {
                isTriggered = false;
                if(rend) rend.material.color = defaultColor;
            }
        }
    }
}
