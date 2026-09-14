using UnityEngine;
using UnityEngine.AI;

public class MovementController : MonoBehaviour
{

    public NavMeshAgent agent;
    GameObject playerCamera;
    public AgentMovementMode movementMode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCamera = Camera.main.gameObject;
    }

    public void Update()
    {
        if (movementMode == AgentMovementMode.STICKWITHPLAYER && Vector3.Distance(playerCamera.transform.position, this.transform.position) > 5)
        {
            MoveToPlayer();
        }
    }

    public void TurnTowardsPlayer()
    {
        agent.destination = Vector3.Lerp(agent.transform.position, playerCamera.transform.position, 0.05f);
    }

    public void MoveToPlayer()
    {
        agent.destination = playerCamera.transform.position;
    }

    public void MoveToPlayerFOV()
    {
        agent.destination = playerCamera.transform.position + playerCamera.transform.forward * 2f;
    }
}

public enum AgentMovementMode
{
    NONE, STICKWITHPLAYER
}