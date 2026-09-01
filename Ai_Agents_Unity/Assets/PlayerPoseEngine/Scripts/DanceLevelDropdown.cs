using UnityEngine;
using UnityEngine.UI;
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

        [Tooltip("Add your Level Difficulty Panel here")]
        public GameObject difficultyButtonsContainer;

        [Header("The Difficulty Levels")]
        [Tooltip("Add the Posemaps")]
        public List<Posemap> easyLevels;
        public List<Posemap> mediumLevels;
        public List<Posemap> hardLevels;

        [Header("Button Highlights")]
        public Button easyButton;
        public Button mediumButton;
        public Button hardButton;
        [Tooltip("Die normale Farbe der nicht-ausgewählten Buttons")]
        public Color normalColor = Color.black;
        [Tooltip("Die Farbe, wenn der Button aktiv ist")]
        public Color activeColor = Color.green;

        // The Audiosource, to later check if the music is playing
        private AudioSource playerAudio;
        // The Key for saving the selected level in PlayerPrefs
        private string savedLevelKey = "SavedDanceLevel";
        // The key for saving the selected difficulty level in PlayerPrefs
        private string savedDiffKey = "SavedDanceDifficulty";
        // The currently active levels based on the selected difficulty
        private List<Posemap> currentActiveLevels;

        void Start()
        {
            if (playerScript == null || dropdown == null || difficultyButtonsContainer == null)
            {
                Debug.LogWarning("DanceLevelSelector: Something is missing in the Inspector!");
                return;
            }

            // Taking the Audiosource 
            playerAudio = playerScript.GetComponent<AudioSource>();
            // Load the saved difficulty level from PlayerPrefs
            int savedDiff = PlayerPrefs.GetInt(savedDiffKey, 0);
            LoadDifficulty(savedDiff, false);
            // Load the saved level index from PlayerPrefs 
            int savedIndex = PlayerPrefs.GetInt(savedLevelKey,0);
            // Check if the saved index is valid for the current active levels
            if(currentActiveLevels == null || savedIndex >= currentActiveLevels.Count)
            {
               savedIndex = 0; 
            }
            dropdown.value = savedIndex;
            OnLevelSelected(savedIndex);
        }    
        void Update()
        {
            // check if Start Button is visible 
            if (playerScript != null && playerScript.startMenuButton != null)
            {
                bool isStartMenuActive = playerScript.startMenuButton.activeSelf;
                // if Strat button is visible reactivate drop down 
                if (dropdown.gameObject.activeSelf != isStartMenuActive)
                {
                    dropdown.gameObject.SetActive(isStartMenuActive); // reactivating drop down 
                }
                // if Start button is visible reactivate difficulty buttons
                if (difficultyButtonsContainer != null && difficultyButtonsContainer.activeSelf != isStartMenuActive)
                {
                    difficultyButtonsContainer.SetActive(isStartMenuActive);
                }
            }
        }
        //Methods called by buttons to load the difficulty levels
        public void SelectEasyDifficulty()
        {
            LoadDifficulty(0, true);
        }
        public void SelectMediumDifficulty()
        {
            LoadDifficulty(1, true);
        }
        public void SelectHardDifficulty()
        {
            LoadDifficulty(2, true);
        }
        private void LoadDifficulty(int difficultyIndex, bool resetLevelIndex)
        {
            // Check if the game is running (music is playing)
           if (playerAudio != null && playerAudio.isPlaying)
            {
               Debug.Log("Game is running, difficulty change blocked.");
               return;  
            } 
            PlayerPrefs.SetInt(savedDiffKey, difficultyIndex);
            // Update button colors based on the selected difficulty
            if (easyButton != null) easyButton.image.color = (difficultyIndex == 0) ? activeColor : normalColor;
            if (mediumButton != null) mediumButton.image.color = (difficultyIndex == 1) ? activeColor : normalColor;
            if (hardButton != null) hardButton.image.color = (difficultyIndex == 2) ? activeColor : normalColor;
            
            switch (difficultyIndex)
            {
                case 0: // Easy
                    currentActiveLevels = easyLevels;
                    break;
                case 1: // Medium
                    currentActiveLevels = mediumLevels;
                    break;
                case 2: // Hard
                    currentActiveLevels = hardLevels;
                    break;
                default:
                    currentActiveLevels = easyLevels;
                    break;
            }
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.ClearOptions();
            // Populate the dropdown with the names of the current active levels
            List<string> levelNames = new List<string>();
            if (currentActiveLevels != null)
            {
               foreach (Posemap level in currentActiveLevels)
                {
                    levelNames.Add(level.name);
                } 
            }
            dropdown.AddOptions(levelNames);
            dropdown.onValueChanged.AddListener(OnLevelSelected);
            // If resetLevelIndex is true, set the dropdown value to 0 and call OnLevelSelected(0)
            if (resetLevelIndex)
            {
              dropdown.value = 0;
              OnLevelSelected(0);  
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
            if (currentActiveLevels != null && index >= 0 && index < currentActiveLevels.Count)
            {
                playerScript.poseMap = currentActiveLevels[index];
                Debug.Log("New level loaded into PosemapPlayer: " + currentActiveLevels[index].name);
                PlayerPrefs.SetInt(savedLevelKey, index);
                PlayerPrefs.Save();
            }
        }
    }
} 