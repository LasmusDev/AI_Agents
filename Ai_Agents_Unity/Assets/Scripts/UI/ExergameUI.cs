using System.Collections;
using UnityEngine;

namespace UI {
    
    public abstract class ExergameUI : MonoBehaviour
    {
        [ContextMenu("DebugCombo")]
        public void StartDebugCombo()
        {
            StartCoroutine(DebugCombo());
        }


        public virtual IEnumerator DebugCombo()
        {
            yield return ShowComboMessage_CR("GREAT!", Color.green);
            yield return ShowComboMessage_CR("MASTER!", Color.yellow);
            yield return ShowComboMessage_CR("INSANE!!!", new Color(1f, 0.5f, 0f)); // Orange
            yield return ShowComboMessage_CR("GODLIKE!!!", Color.cyan);
            yield return ShowComboMessage_CR("MISS", Color.grey);
        }

        public abstract IEnumerator ShowComboMessage_CR(string text, Color color);
    }
    
}
