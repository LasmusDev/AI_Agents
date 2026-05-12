using UnityEngine;
using UnityEngine.InputSystem;

public class ChangePosition : MonoBehaviour
{
    [SerializeField] private InputActionReference RightControllerPosition;

    void Update()
    {
        Vector3 controllerPosition = RightControllerPosition.action.ReadValue<Vector3>();
        transform.position = controllerPosition;
        
    }
}