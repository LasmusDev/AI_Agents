using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TwoWayPortal : MonoBehaviour
{
    public TwoWayPortal linkedPortal;
    public Transform teleportTarget;

    private UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportationProvider;
    private bool isTeleporting = false;

    private void Awake()
    {
        teleportationProvider = FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>();
    }

    public void TeleportPlayer(SelectEnterEventArgs args)
    {
        if (!isTeleporting && linkedPortal != null && teleportTarget != null)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor interactor = args.interactorObject;
            if (interactor != null)
            {
                linkedPortal.StartTeleport();
                UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest request = new UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest()
                {
                    destinationPosition = linkedPortal.teleportTarget.position,
                };
                teleportationProvider.QueueTeleportRequest(request);
                Invoke(nameof(EndTeleport), 1f); // Cooldown to prevent immediate re-teleport
            }
        }
    }

    private void StartTeleport()
    {
        isTeleporting = true;
    }

    private void EndTeleport()
    {
        isTeleporting = false;
        linkedPortal.isTeleporting = false;
    }
}