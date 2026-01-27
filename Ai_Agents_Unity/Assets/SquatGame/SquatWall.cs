using UnityEngine;

namespace SquatGame
{
    public class SquatWall : MonoBehaviour
    {
        public float speed = 5.0f;
        public SquatManager manager; 
        
        private bool hasHitPlayer = false;

        void Start()
        {
            
            Destroy(gameObject, 8.0f);
        }

        void Update()
        {
            
            transform.Translate(Vector3.back * speed * Time.deltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            if (hasHitPlayer) return; 

            
            if (other.CompareTag("Player"))
            {
                hasHitPlayer = true;
              
                if (manager != null) manager.PlayerHit();
                
                if(GetComponent<Renderer>())
                   GetComponent<Renderer>().material.color = Color.red;
                
                
            }
        }
    }
}