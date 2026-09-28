using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

namespace UI {
    
    public class UiAnimationManager : MonoBehaviour
    {
        private static UiAnimationManager _instance;
        public static UiAnimationManager Instance {
            get {
                if (_instance == null)
                {
                    Instance = GameObject.FindFirstObjectByType<UiAnimationManager>();
                }
                return _instance;
            }
            set {
                _instance = value;
            }
        }
        

        public IEnumerator PlayComboAnimation(GameObject go, float moveSpeed = 5, float animationDuration = 0.5f, float scaleSize = 1.5f, bool wobble = true)
        {
            go.SetActive(true);
            Vector3 startPos = go.transform.position;
            Vector3 startScale = go.transform.localScale;
            Vector3 movementVector = go.transform.position + Vector3.up * moveSpeed * animationDuration; 
            movementVector = wobble ? movementVector + new Vector3(Random.Range(-0.2f, 0.2f), 0, 0) : movementVector;   
            Sequence moveSeq = DOTween.Sequence()
                .Append(go.transform.DOMove(movementVector, animationDuration));
            moveSeq.Play();
            Sequence scaleSeq = DOTween.Sequence()
                .Append(go.transform.DOScale(new Vector3(scaleSize, scaleSize, scaleSize), animationDuration));
            scaleSeq.Play();
            yield return moveSeq.WaitForCompletion();
            yield return scaleSeq.WaitForCompletion();
            Sequence  fadeSeq = DOTween.Sequence()
                .Append(go.GetComponent<TMP_Text>().DOFade(0, animationDuration));
            fadeSeq.Play();
            yield return fadeSeq.WaitForCompletion();
            go.SetActive(false);
            go.transform.position = startPos;
            go.transform.localScale = startScale;
            go.GetComponent<TMP_Text>().color = new Color(go.GetComponent<TMP_Text>().color.r, go.GetComponent<TMP_Text>().color.g, go.GetComponent<TMP_Text>().color.b, 1);
        }

        public IEnumerator PlayComboAnimation(GameObject go, Vector3 posChange, float animationDuration, float scaleSize)
        {
            go.SetActive(true);
            Vector3 startPos = go.transform.position;
            Vector3 startScale = go.transform.localScale;
            Sequence moveSeq = DOTween.Sequence()
                .Append(go.transform.DOMove(go.transform.position + posChange, animationDuration));
            moveSeq.Play();
            Sequence scaleSeq = DOTween.Sequence()
                .Append(go.transform.DOScale(new Vector3(scaleSize, scaleSize, scaleSize), animationDuration));
            scaleSeq.Play();
            yield return moveSeq.WaitForCompletion();
            yield return scaleSeq.WaitForCompletion();
            Sequence fadeSeq = DOTween.Sequence()
                .Append(go.GetComponent<TMP_Text>().DOFade(0, animationDuration/2));
            fadeSeq.Play();
            yield return fadeSeq.WaitForCompletion();
            go.SetActive(false);
            go.transform.position = startPos;
            go.transform.localScale = startScale;
            go.GetComponent<TMP_Text>().color = new Color(go.GetComponent<TMP_Text>().color.r, go.GetComponent<TMP_Text>().color.g, go.GetComponent<TMP_Text>().color.b, 1);
        }
    }
    
}
