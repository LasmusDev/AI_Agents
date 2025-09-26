using JetBrains.Annotations;
using System.Collections;
using UnityEngine;

namespace Exergames {
    
    public class SquatsGame : MonoBehaviour
    {

        public int squats;
        public int score;
        public float startingSquatDuration;
        public float holdSquatOdds;
        public GameObject boxToAvoid;
        public bool UpOrDown;
        public Vector3 upperSpawn;
        public Vector3 lowerSpawn;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            StartCoroutine(RunSquatGame());
        }
    
        // Update is called once per frame
        void Update()
        {
            
        }

        public IEnumerator RunSquatGame()
        {
            float timer = 0;
            float durationMultiplier = 1.0f;
            while (true)
            {
                timer += Time.deltaTime;
                if(timer > startingSquatDuration * durationMultiplier)
                {
                    timer = 0;
                    durationMultiplier *= 0.99f;
                    GameObject box = Instantiate(boxToAvoid, this.transform);
                    box.transform.position = UpOrDown ? upperSpawn : lowerSpawn;
                    UpOrDown = !UpOrDown;

                    if(Random.Range(0,1f) < holdSquatOdds)
                    {
                        box.transform.localScale = new Vector3(5, 1, 1);
                    } 
                }
                yield return null;
            }
        }

    }
    
}
