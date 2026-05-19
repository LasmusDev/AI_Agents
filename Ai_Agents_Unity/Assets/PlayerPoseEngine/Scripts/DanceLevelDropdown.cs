using UnityEngine;
using TMPro;
using System.Collections.Generic;
using PlayerPoseEngine.Scripts; 

namespace DancingGame
{
    public class DanceLevelDropdown : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Add your PosemapPlayer here")]
        public PosemapPlayer playerScript;
        
        [Tooltip("Add your Dropdown Menu here")]
        public TMP_Dropdown dropdown;

        [Header("The Levels")]
        [Tooltip("Add the Posemaps")]
        public List<Posemap> availableLevels;

        // The Audiosource, to later check if the music is playing
        private AudioSource playerAudio;
        private string savedLevelKey = "SavedDanceLevel";

        void Start()
        {
            if (playerScript == null || dropdown == null || availableLevels.Count == 0)
            {
                Debug.LogWarning("DanceLevelSelector: Something is missing in the Inspector!");
                return;
            }

            // Taking the Audiosource 
            playerAudio = playerScript.GetComponent<AudioSource>();

            dropdown.ClearOptions();
            
            // Go through list of levels and add their names to the dropdown options
            List<string> levelNames = new List<string>();
            foreach (Posemap level in availableLevels)
            {
                levelNames.Add(level.name); 
            }
            
            dropdown.AddOptions(levelNames);
            dropdown.onValueChanged.AddListener(OnLevelSelected);
            int savedIndex = PlayerPrefs.GetInt(savedLevelKey, 0);
            if(savedIndex >= availableLevels.Count)
            {
                savedIndex = 0;
            }

            // Load first level by default
            dropdown.value = savedIndex;
            OnLevelSelected(savedIndex);
        }
        void Update()
        {
            // check if Start Button is visible 
            if (playerScript != null && playerScript.startMenuButton != null)
            {
                // if Strat button is visible reactivate drop down 
                if (!dropdown.gameObject.activeSelf && playerScript.startMenuButton.activeSelf)
                {
                    dropdown.gameObject.SetActive(true); // reactivating drop down 
                }
            }
        }

        public void OnLevelSelected(int index)
        {
            // check if music is playing already
            if (playerAudio != null && playerAudio.isPlaying) 
            {
                Debug.Log("Game is already running (music is playing), level change blocked.");
                return;
            }

            // Load the selected level into the PosemapPlayer
            if (index >= 0 && index < availableLevels.Count)
            {
                playerScript.poseMap = availableLevels[index];
                Debug.Log("New level loaded into PosemapPlayer: " + availableLevels[index].name);
                PlayerPrefs.SetInt(savedLevelKey, index);
                PlayerPrefs.Save();
            }
        }
    }
}