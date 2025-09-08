using UnityEngine;
using UnityEngine.XR.OpenXR.Input;

namespace PlayerPoseEngine.Scripts{
    
    public class PlayerPoseVisualizer : MonoBehaviour
    {
        public PlayerPose toVisualize;
        public bool visualizePose;
        public GameObject lHandVisSphere;
        public GameObject rHandVisSphere;
        public GameObject lFootVisSphere;
        public GameObject rFootVisSphere;
        public GameObject headVisSphere;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }
    
        // Update is called once per frame
        void Update()
        {
            if(visualizePose && toVisualize)
            {

            }
        }
    }
    
}
