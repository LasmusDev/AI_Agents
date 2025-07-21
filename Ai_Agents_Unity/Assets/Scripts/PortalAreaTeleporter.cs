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

    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player and this portal is not on cooldown
        if (other.CompareTag("Player") && !isDeactivated)
        {
            // Create the teleport request
            var request = new UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest()
            {
                destinationPosition = linkedPortal.teleportTarget.position,
                destinationRotation = linkedPortal.teleportTarget.rotation, 

            };

            // Send the teleport request to the provider
            teleportationProvider.QueueTeleportRequest(request);

            // Activate the cooldown on the portal the player is arriving at
            linkedPortal.ActivateCooldown(10f);
        }
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