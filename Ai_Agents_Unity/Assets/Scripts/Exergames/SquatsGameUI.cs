using UnityEngine;
using TMPro;
using SquatGame;
using System.Collections;
using UI;

namespace Exergames 
{
    public class SquatsGameUI : ExergameUI
    {
        public TMP_Text scoreText;
        public TMP_Text comboText;
        public SquatManager game; 
        // New fields for the floating combo message
        public TMP_Text comboMessageText; 
        public float displayTime = 0.25f;
        public float floatSpeed = 2f;
        public float popScale = 1.5f;
        

        public void Start()
        {
            // Initialize the combo message text to be only visible when the Game is running
            if (comboMessageText != null)
            {
                comboMessageText.gameObject.SetActive(false);
            }
        }

        public void Update()
        {
           // Update the score and combo text based on the player's current score and combo
             if (game != null)
             {
                 if (scoreText != null) scoreText.text = game.score.ToString();
                 if (comboText != null) comboText.text = game.combo.ToString();
             }

        }
        // Method to show the combo message with a specific text and color
        public override IEnumerator ShowComboMessage_CR(string message, Color textColor)
        {
            if (comboMessageText == null)
            {
                yield break;
            }
            comboMessageText.text = message;
            comboMessageText.color = textColor;
            yield return StartCoroutine(UiAnimationManager.Instance.PlayComboAnimation_CR(comboMessageText.gameObject, floatSpeed, displayTime, popScale));
        }
    }
} 