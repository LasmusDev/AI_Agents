using UnityEngine;

namespace YogaGame
{
    public class YogaTriggerZone : MonoBehaviour
    {
        [Header("Einstellung")]
        // Enum for the body part that is required to trigger this zone (e.g. left hand, right hand, head)
        public BodyPartType requiredPart = BodyPartType.LeftHand; 
        
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
            // Searching for the YogaBodyPart component in the parent objects of the collider that entered the trigger zone
            YogaBodyPart bodyPart = other.GetComponentInParent<YogaBodyPart>();

            // Logic to check if the correct body part is in the trigger zone:
            if (bodyPart != null)
            {
                // Looking if the body part that entered the trigger zone is the one that is required to trigger this zone
                if (bodyPart.myType == requiredPart)
                {
                    isTriggered = true;
                    if(rend) rend.material.color = successColor;
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            YogaBodyPart bodyPart = other.GetComponentInParent<YogaBodyPart>();

            if (bodyPart != null && bodyPart.myType == requiredPart)
            {
                isTriggered = false;
                if(rend) rend.material.color = defaultColor;
            }
        }
    }
}