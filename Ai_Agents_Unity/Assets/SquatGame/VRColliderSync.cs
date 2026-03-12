using UnityEngine;

public class VRColliderSync : MonoBehaviour
{
        [Tooltip("Head")]
        public Transform vrHeadset; 
        
        [Tooltip("Offset for the top of the head to prevent the capsule from ending at the eyes")]
        public float topOfHeadOffset = 0.15f;

        private CharacterController characterController;

        void Start()
        {
            characterController = GetComponent<CharacterController>();
        }

    void Update()
    {
        if (vrHeadset == null || characterController == null) return;

        // calculate the height of the head in world space
        float headHeightInWorld = vrHeadset.position.y - transform.position.y;
            
        // add the offset to get the total height of the capsule
        float totalHeight = headHeightInWorld + topOfHeadOffset;

        // Check if the total height is less than the minimum
        //  height of the capsule (which is twice the radius). 
        float minHeight = characterController.radius * 2f;
        characterController.height = Mathf.Max(minHeight, totalHeight);

            
        Vector3 headsetLocalPos = transform.InverseTransformPoint(vrHeadset.position);
            
        // Set the center of the capsule to be at the position of the headset, but with the Y value set to half of the total height.
        Vector3 newCenter = new Vector3(
            headsetLocalPos.x, 
            characterController.height / 2f, 
            headsetLocalPos.z
            );

            characterController.center = newCenter;
    }
}