using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

namespace UI {
    
    public class UiAnimationManager : MonoBehaviour
    {
        public static UiAnimationManager Instance { get; private set; }


        public IEnumerator PlayComboAnimation(GameObject go, float moveSpeed, float scaleSize)
        {
            go.SetActive(true);
            Vector3 startPos = go.transform.position;
            Vector3 startScale = go.transform.localScale;
            Sequence moveSeq = DOTween.Sequence()
                .Append(go.transform.DOMove(go.transform.position + Vector3.up * 0.5f + new Vector3(Random.Range(-0.2f, 0.2f), 0, 0), 1/moveSpeed));
            moveSeq.Play();
            Sequence scaleSeq = DOTween.Sequence()
                .Append(go.transform.DOScale(new Vector3(scaleSize, scaleSize, scaleSize), 1/moveSpeed));
            scaleSeq.Play();
            yield return moveSeq.WaitForCompletion();
            yield return scaleSeq.WaitForCompletion();
            Sequence  fadeSeq = DOTween.Sequence()
                .Append(go.GetComponent<TMP_Text>().DOFade(0, 0.5f/moveSpeed));
            fadeSeq.Play();
            yield return fadeSeq.WaitForCompletion();
            go.SetActive(false);
            go.transform.position = startPos;
            go.transform.localScale = startScale;
            go.GetComponent<TMP_Text>().color = new Color(go.GetComponent<TMP_Text>().color.r, go.GetComponent<TMP_Text>().color.g, go.GetComponent<TMP_Text>().color.b, 1);
        }
    }
    
}
