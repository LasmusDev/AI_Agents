using DG.Tweening;
using PlayerPoseEngine.Scripts;
using System.Collections;
using UnityEngine;

namespace Exergames {
    
    public class SpeakerVFX : MonoBehaviour
    {
        public float bpm;
        public Vector3 startingScale;

        public PosemapPlayer player;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            startingScale = transform.localScale;
            player.OnGameStarted += StartPulse;
            player.OnGameEnded += EndPulse;
        }

        public void EndPulse()
        {
            this.StopAllCoroutines();
            transform.localScale = startingScale;
        }

        public void StartPulse()
        {
            bpm = player.BPM;
            StartCoroutine(Pulse());
        }

        public IEnumerator Pulse()
       {
            while (true)
            {

                Sequence scaleSeq = DOTween.Sequence()
                    .Append(transform.DOScale(startingScale * 1.2f, 60f/bpm));
                scaleSeq.Play();
                yield return scaleSeq.WaitForCompletion();
                transform.localScale = startingScale;
            }
       }
    }
    
}
