using UnityEngine;
using SquatGame;

public class SitupWallLogic : MonoBehaviour
{
    //References to the top and bottom blocks
    public Collider topBlock;
    public Collider bottomBlock;
    // Reference to the SquatManager 
    private SquatManager manager;
    private bool topTouched = false;
    private bool hasEvaluated = false;
    
    private float closestDistance = 9999f; 
    
    //Reference to the parent collider (the wall itself)
    private Collider parentCollider; 

    void Start()
    {
        // Find the SquatManager in the scene and get the reference to the parent collider
        manager = FindFirstObjectByType<SquatManager>();
        parentCollider = GetComponent<Collider>();
        
        //Initially, we disable the parent collider to prevent scoring before the player has touched the top block
        //To prevent the player from scoring without actually playing the game 
        if (parentCollider != null)
        {
            parentCollider.enabled = false;
        }
    }
    void Update()
    {
        // If we've already evaluated the player's performance or if the manager or player head is null, we exit early
        if (hasEvaluated || manager == null || manager.playerHead == null) return;

        Vector3 headPos = manager.playerHead.position;

        //Top block touched? (Success: Player started the sit-up) 
        if (!topTouched && topBlock != null && topBlock.gameObject.activeInHierarchy)
        {
            if (topBlock.bounds.Contains(headPos))
            {
                topTouched = true;
                topBlock.gameObject.SetActive(false); 
                
                //Activate the parent collider
                if (parentCollider != null)
                {
                    parentCollider.enabled = true;
                }
            }
        }

        // Second check: Did the player hit the bottom block? (Failure: Player didn't complete the sit-up)
        if (bottomBlock != null && bottomBlock.gameObject.activeInHierarchy)
        {
            if (bottomBlock.bounds.Contains(headPos))
            {
                manager.PlayerHit();
                hasEvaluated = true;
                Destroy(gameObject); 
                return;
            }
        }

        // 3. Third check: If the player is too far away from the wall, we consider it a failure (MISS)
        float currentDistance = Vector3.Distance(transform.position, headPos);
        
        if (currentDistance < closestDistance)
        {
            closestDistance = currentDistance;
        }
        
        // If the player has moved away from the wall beyond a certain threshold, we consider it a failure (MISS)
        if (currentDistance > closestDistance + 0.2f)
        {
            if (!topTouched)
            {
                // If the player never touched the top block, we consider it a failure (MISS)
                manager.PlayerHit();
                Destroy(gameObject);
            }
            
            hasEvaluated = true;
        }
    }
}