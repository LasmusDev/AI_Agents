using TMPro;
using UnityEngine;

namespace Exergames {
    
    public class DanceGameUI : MonoBehaviour
    {
        public TMP_Text scoreText;
        public TMP_Text comboText;
        public PosemapPlayer player;

        public void Update()
        {
            scoreText.text = player.score.ToString();
            comboText.text = player.combo.ToString();
        }
    }
    
}
