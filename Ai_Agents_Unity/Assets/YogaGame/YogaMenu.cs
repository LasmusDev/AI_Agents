using UnityEngine;
using UnityEngine.InputSystem;

namespace YogaGame
{
    public class SummonMenu : MonoBehaviour
    {
        [Header("Settings")]
        public GameObject menuCanvas; 
        public InputActionProperty toggleButton; 
        public Transform playerHead;
        public float distanceFromPlayer = 1.5f;

       
        private void OnEnable()
        {
            if (toggleButton.action != null) toggleButton.action.Enable();
        }

        private void OnDisable()
        {
            if (toggleButton.action != null) toggleButton.action.Disable();
        }

        void Start()
        {
            //hide at start
            if(menuCanvas) menuCanvas.SetActive(false);
        }

        void Update()
        {
            //check for button press
            if (toggleButton.action == null) return;

            if (toggleButton.action.WasPressedThisFrame())
            {
                ToggleMenu();
            }
        }

        public void ToggleMenu()
        {
            if (menuCanvas == null) return;

            bool isActive = !menuCanvas.activeSelf;
            
            if (isActive)
            {
                menuCanvas.SetActive(true);
                
                //position in front of player
                if (playerHead)
                {
                    Vector3 spawnPos = playerHead.position + (playerHead.forward * distanceFromPlayer);
                    spawnPos.y = playerHead.position.y; //same height as head
                    menuCanvas.transform.position = spawnPos;
                    
                    //face the player
                    menuCanvas.transform.LookAt(playerHead);
                    menuCanvas.transform.Rotate(0, 180, 0);
                }
            }
            else
            {
                menuCanvas.SetActive(false);
            }
        }
    }
}