using UnityEngine;

namespace SquatGame
{
    public class SquatScoreTrigger : MonoBehaviour
    {
        public SquatManager manager;

        void OnTriggerEnter(Collider other)
        {
            
            if (other.GetComponent<SquatWall>())
            {
                
                if(manager != null) manager.AddScore();
                
                
                Destroy(other.gameObject); 
            }
        }
    }
}