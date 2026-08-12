using PlayerPoseEngine.Scripts;
using TMPro;
using UnityEngine;

namespace Exergames {
    
    public class DanceGameUI : MonoBehaviour
    {
        public TMP_Text scoreText;
        public TMP_Text comboText;
        public PosemapPlayer player;

        // New fields for the floating combo message
        public TMP_Text comboMessageText; 
        public float displayTime = 1.5f;
        public float floatSpeed = 50f;
        public float popScale = 1.5f;
        public float scaleSpeed = 10f;
        //helper variables to manage the floating message
        private float currentTimer = 0f;
        private Vector3 startLocalPos;
        private Color startColor;

        void Start()
        {
            // Initialize the combo message text to be only visible when the Dancing Game is running
            if (comboMessageText != null)
            {
                startLocalPos = comboMessageText.rectTransform.localPosition;
                startColor = comboMessageText.color;
                
                comboMessageText.color = new Color(startColor.r, startColor.g, startColor.b, 0);
                comboMessageText.gameObject.SetActive(false);
            }
        }

        public void Update()
        {
            // Update the score and combo text based on the player's current score and combo
            if (player != null)
            {
                if (scoreText != null) scoreText.text = player.score.ToString();
                if (comboText != null) comboText.text = player.combo.ToString();
            }

            //Animate the floating combo message if it's currently active
            if (currentTimer > 0 && comboMessageText != null)
            {
                currentTimer -= Time.deltaTime;
                // Floating upwards
                comboMessageText.rectTransform.localPosition += Vector3.up * floatSpeed * Time.deltaTime;
                // Scaling effect (pop effect)
                comboMessageText.rectTransform.localScale = Vector3.Lerp(comboMessageText.rectTransform.localScale, Vector3.one, Time.deltaTime * scaleSpeed);
                // Fade-Out
                if (currentTimer < 0.5f)
                {
                    float alpha = currentTimer / 0.5f;
                    comboMessageText.color = new Color(comboMessageText.color.r, comboMessageText.color.g, comboMessageText.color.b, alpha);
                }
                
                if (currentTimer <= 0)
                {
                    comboMessageText.gameObject.SetActive(false);
                }
            }
        }

        // Method to show the combo message with a specific text and color
        public void ShowComboMessage(string message, Color textColor)
        {
            if (comboMessageText == null) return;

            comboMessageText.text = message;
            comboMessageText.color = textColor;
            comboMessageText.gameObject.SetActive(true);
            
            comboMessageText.rectTransform.localPosition = startLocalPos;
            comboMessageText.rectTransform.localScale = Vector3.one * popScale;
            
            currentTimer = displayTime;
        }
    }
}