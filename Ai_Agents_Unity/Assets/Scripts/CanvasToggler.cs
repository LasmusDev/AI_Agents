using UnityEngine;

public class CanvasToggler : MonoBehaviour
{
    // Drag the canvas you want to toggle into this field in the Inspector
    public GameObject canvasToToggle;

    // This function will be called by the button
    public void ToggleCanvas()
    {
        if (canvasToToggle != null)
        {
            // Toggles the active state of the canvas GameObject
            canvasToToggle.SetActive(!canvasToToggle.activeSelf);
        }
    }
}