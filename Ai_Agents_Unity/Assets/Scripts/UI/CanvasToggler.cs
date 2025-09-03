using UnityEngine;
using System.Collections.Generic; // Required for List<T> if you choose to use it

namespace BiosignalsLabUI {
    
    public class CanvasToggler : MonoBehaviour
    {
        // Option 1: Using an array (fixed size, set in Inspector)
        // Drag all the GameObjects you want to toggle into this array in the Inspector
        public GameObject[] gameObjectsToToggle;
    
        // Option 2: Using a List (dynamic size, more flexible if adding/removing at runtime)
        // Uncomment the line below and the corresponding code in ToggleGameObjects() to use this
        // public List<GameObject> gameObjectsToToggleList = new List<GameObject>();
    
        // This function will be called by the button
        public void ToggleGameObjects()
        {
            // Toggle GameObjects from the array
            if (gameObjectsToToggle != null && gameObjectsToToggle.Length > 0)
            {
                foreach (GameObject obj in gameObjectsToToggle)
                {
                    if (obj != null)
                    {
                        obj.SetActive(!obj.activeSelf);
                    }
                }
            }
            else
            {
                Debug.LogWarning("No GameObjects assigned to 'gameObjectsToToggle' array in CanvasToggler script on " + gameObject.name);
            }
    
        }
    }
    
}
