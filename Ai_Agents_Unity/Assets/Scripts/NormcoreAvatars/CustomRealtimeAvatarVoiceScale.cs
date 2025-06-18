using UnityEngine;
using Normal.Realtime;

namespace NormcoreAvatars
{
    [RequireComponent(typeof(CustomRealtimeAvatarVoice))]
    public class CustomRealtimeAvatarVoiceScale : MonoBehaviour {
        private CustomRealtimeAvatarVoice _voice;

        void Awake() {
            // Get a reference to the CustomRealtimeAvatarVoice component
            _voice = GetComponent<CustomRealtimeAvatarVoice>();
        }

        void Update() {
            // Get the voice volume
            float voiceVolume = _voice.voiceVolume;

            // Use the voice volume to calculate the scale of our head (between 1.0f and 4.0f)
            float scale = 1.0f + voiceVolume * 3.0f;

            // Apply the scale to the this game object
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
