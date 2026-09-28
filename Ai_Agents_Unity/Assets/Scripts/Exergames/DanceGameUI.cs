using PlayerPoseEngine.Scripts;
using System.Collections;
using TMPro;
using UI;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace Exergames {
    
    public class DanceGameUI : ExergameUI
    {
        public TMP_Text scoreText;
        public TMP_Text comboText;
        public PosemapPlayer player;

        // New fields for the floating combo message
        public TMP_Text comboMessageText; 
        public float displayTime = 0.25f;
        public float floatSpeed = 2f;
        public float popScale = 1.5f;

        void Start()
        {
            // Initialize the combo message text to be only visible when the Dancing Game is running
            if (comboMessageText != null)
            {
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
        }
        

        // Method to show the combo message with a specific text and color
        public override IEnumerator ShowComboMessage(string message, Color textColor)
        {
            if (comboMessageText == null)
            {
                yield break;
            }

            comboMessageText.text = message;
            comboMessageText.color = textColor;
            yield return StartCoroutine(UiAnimationManager.Instance.PlayComboAnimation(comboMessageText.gameObject, floatSpeed, 0.25f, popScale));
        }
    }
}