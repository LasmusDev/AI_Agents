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


        public IEnumerator PlayComboAnimation_CR(GameObject basis, float moveSpeed = 5, float animationDuration = 0.5f, float scaleSize = 1.5f, bool wobble = true)
        {

            GameObject go = Instantiate(basis, basis.transform.parent);
            go.SetActive(true);
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
            Destroy(go);

        }

        /// <summary>
        /// Variant using speed and anim duration instead of move distance. Currently not in use and therefore not being updated
        /// </summary>
        /// <param name="go"></param>
        /// <param name="posChange"></param>
        /// <param name="animationDuration"></param>
        /// <param name="scaleSize"></param>
        /// <returns></returns>
        public IEnumerator PlayComboAnimation_CR(GameObject basis, Vector3 posChange, float animationDuration, float scaleSize)
        {
            GameObject go = Instantiate(basis, basis.transform.parent);
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
            Destroy(go);
        }
    }
    
}
