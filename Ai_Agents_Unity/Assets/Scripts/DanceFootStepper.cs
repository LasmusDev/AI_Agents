using UnityEngine;

namespace DancingGame
{
    public class DanceFootStepper : MonoBehaviour
    {
        public Transform bodyRoot;
        public float stepDistance = 0f;
        public float stepHeight = 0.3f;
        public float stepSpeed = 1f;

        public DanceFootStepper otherFoot;

        private Vector3 currentPos;
        private Vector3 idealLocalOffset;
        public bool isStepping { get; private set; }
        private float stepProgress;
        private Vector3 targetPos;

        void Start()
        {
            // Attempt to find the other foot if not assigned
            if (otherFoot == null && bodyRoot != null)
            {
                Transform foundOther = bodyRoot.FindRecursive(child => 
                    child != this.transform && child.GetComponent<DanceFootStepper>() != null
                );
                
                if (foundOther != null)
                {
                    otherFoot = foundOther.GetComponent<DanceFootStepper>();
                }
            }

            if (bodyRoot != null)
            {
                idealLocalOffset = bodyRoot.InverseTransformPoint(transform.position);
                idealLocalOffset.y = 0f; 
            }
            currentPos = transform.position;
        }

        void Update()
        {
            if (bodyRoot == null) return;

            Vector3 idealPos = bodyRoot.TransformPoint(idealLocalOffset);
            idealPos.y = 0f; 

            float distance = Vector3.Distance(new Vector3(currentPos.x, 0, currentPos.z), new Vector3(idealPos.x, 0, idealPos.z));
            bool canStep = otherFoot == null || !otherFoot.isStepping;

            if (distance > stepDistance && !isStepping && canStep)
            {
                isStepping = true;
                targetPos = idealPos;
                stepProgress = 0f;
            }

            if (isStepping)
            {
                stepProgress += Time.deltaTime * stepSpeed;
                float easedProgress = Mathf.SmoothStep(0f, 1f, stepProgress);
                Vector3 lerpedPos = Vector3.Lerp(currentPos, targetPos, easedProgress);
                
                lerpedPos.y += Mathf.Sin(stepProgress * Mathf.PI) * stepHeight;
                transform.position = lerpedPos;

                if (stepProgress >= 1f)
                {
                    isStepping = false;
                    currentPos = targetPos; 
                }
            }
            else
            {
                transform.position = currentPos;
            }
        }
    }
}