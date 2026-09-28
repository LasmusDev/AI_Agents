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
            yield return ShowComboMessage("GREAT!", Color.green);
            yield return ShowComboMessage("MASTER!", Color.yellow);
            yield return ShowComboMessage("INSANE!!!", new Color(1f, 0.5f, 0f)); // Orange
            yield return ShowComboMessage("GODLIKE!!!", Color.cyan);
            yield return ShowComboMessage("MISS", Color.grey);
        }

        public abstract IEnumerator ShowComboMessage(string text, Color color);
    }
    
}
