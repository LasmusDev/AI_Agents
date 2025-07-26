using UnityEngine;
using System.Collections; // Required for Coroutines

public class PortalAreaTeleporter : MonoBehaviour
{
    public PortalAreaTeleporter linkedPortal;
    public Transform teleportTarget;

    private UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportationProvider;
    private bool isDeactivated = false; // This flag will handle the cooldown

    void Awake()
    {
        // Find the TeleportationProvider in the scene
        teleportationProvider = FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>();
    }

    public void Update()
    {
        BoxCollider bc = GetComponent<BoxCollider>();
        Bounds colliderBounds = bc.bounds;
        if (IsInXZBounds(Camera.main.transform.position, colliderBounds.min, colliderBounds.max))
        {

            // Create the teleport request
            var request = new UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest()
            {
                destinationPosition = linkedPortal.teleportTarget.position,

            };
            // Send the teleport request to the provider
            teleportationProvider.QueueTeleportRequest(request);
            // Activate the cooldown on the portal the player is arriving at
            linkedPortal.ActivateCooldown(5f);
        }
    }

    public bool IsInXZBounds(Vector3 obj, Vector3 min, Vector3 max)
    {
        return obj.x > min.x && obj.x < max.x && obj.z > min.z && obj.z < max.z;
    }

    private void OnTriggerEnter(Collider other)
    {

    }

    // This public method can be called to start the cooldown
    public void ActivateCooldown(float duration)
    {
        StartCoroutine(CooldownCoroutine(duration));
    }

    // This coroutine manages the timed deactivation
    private IEnumerator CooldownCoroutine(float duration)
    {
        isDeactivated = true; // Deactivate this portal
        yield return new WaitForSeconds(duration); // Wait for 10 seconds
        isDeactivated = false; // Reactivate this portal
    }
}