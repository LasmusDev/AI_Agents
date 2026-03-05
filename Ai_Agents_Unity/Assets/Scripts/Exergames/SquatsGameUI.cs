using UnityEngine;
using TMPro;
using SquatGame; 

namespace Exergames 
{
    public class SquatsGameUI : MonoBehaviour
    {
        public TMP_Text scoreText;
        public TMP_Text comboText;
        public SquatManager game; 

        public void Update()
        {
             if (game == null) return;
             scoreText.text = game.score.ToString();
             comboText.text = game.combo.ToString();
        }
    }
} 