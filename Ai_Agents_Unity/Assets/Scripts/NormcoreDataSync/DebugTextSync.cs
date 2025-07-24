using UnityEngine;

namespace NormcoreDataSync {
    
    public class DebugTextSync : MonoBehaviour
    {
        public SynchronizedText toTestOn;

    
        // Update is called once per frame
        void Update()
        {
            toTestOn.SetText("DEBBIE!");
        }
    }
    
}
