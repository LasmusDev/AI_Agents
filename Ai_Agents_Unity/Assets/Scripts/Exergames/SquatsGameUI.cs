using PlayerPoseEngine.Scripts;
using TMPro;
using UnityEngine;

namespace Exergames {
    
    public class SquatsGameUI : MonoBehaviour
    {
        public TMP_Text scoreText;
        public TMP_Text comboText;
        public SquatsGame game;

        public void Update()
        {
            scoreText.text = game.score.ToString();
            comboText.text = game.squats.ToString();
        }
    }
    
}
