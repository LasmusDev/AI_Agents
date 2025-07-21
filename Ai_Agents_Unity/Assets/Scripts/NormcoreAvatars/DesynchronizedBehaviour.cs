using UnityEngine;

namespace NormcoreAvatars{
    
    //This superscript is intended to be the parent of behaviours that only run on either client or remote, but not both.
    public class DesynchronizedBehaviour : MonoBehaviour
    {
        private ControlState state;
        public MonoBehaviour controlStateOverride;

        public void SetControlState(ControlState newState, MonoBehaviour sender)
        {
            state = newState;
            controlStateOverride = sender;
        }

    }
    public enum ControlState{
        UNDEFINED, LOCAL, REMOTE_COPY_TAKES_OVER, REMOTE_COPY_CONTROLS
    }
}
