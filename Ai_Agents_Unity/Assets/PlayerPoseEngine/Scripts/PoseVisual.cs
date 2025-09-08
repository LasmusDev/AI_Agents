using UnityEngine;

namespace PlayerPoseEngine.Scripts {

  
    public class PoseVisual : MonoBehaviour
    {
        public Material startingMat;
        public Material fulfilledMat;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            startingMat = GetComponent<Renderer>().material;
        }

        public void ToggleFulfilled(bool fulfilled)
        {
            GetComponent<Renderer>().material = fulfilled ? fulfilledMat : startingMat;
        }
    }
    
}
