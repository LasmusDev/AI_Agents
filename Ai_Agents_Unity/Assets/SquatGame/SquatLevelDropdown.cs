using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace SquatGame
{
    [System.Serializable]
    public class SquatLevel
    {
        public string levelName = "New Level";
        public SquatPoseMap poseMap;
        public AudioClip song;
    }

    public class SquatLevelDropdown : MonoBehaviour
    {
        [Tooltip("Drag your SquatManager here")]
        public SquatManager manager;
        
        [Tooltip("Drag your Dropdown here")]
        public TMP_Dropdown dropdown;

        [Tooltip("Add Levels")]
        public List<SquatLevel> availableLevels;
        private string savedLevelName = "SavedSquatLevel";

        void Start()
        {
            if (manager == null || dropdown == null || availableLevels.Count == 0)
            {
                Debug.LogWarning("Missing references ");
                return;
            }

            dropdown.ClearOptions();
            List<string> levelNames = new List<string>();
            foreach (SquatLevel level in availableLevels)
            {
                levelNames.Add(level.levelName); 
            }
            
            dropdown.AddOptions(levelNames);
            dropdown.onValueChanged.AddListener(OnLevelSelected);
            int savedIndex = PlayerPrefs.GetInt(savedLevelName, 0);
            if(savedIndex >= availableLevels.Count)
            {
                savedIndex = 0;
            }

            dropdown.value = savedIndex;
            OnLevelSelected(savedIndex);
        }

        void Update()
        {
            // Set dropdown back to active if the start button is active again
            if (manager != null && manager.startButton != null)
            {
                if (!dropdown.gameObject.activeSelf && manager.startButton.activeSelf)
                {
                    dropdown.gameObject.SetActive(true); 
                }
            }
        }

        public void OnLevelSelected(int index)
        {
            if (manager != null && manager.isRunning) 
            {
                Debug.Log("Game is running. Cannot change level now.");
                return;
            }
            if (index >= 0 && index < availableLevels.Count)
            {
                manager.currentPoseMap = availableLevels[index].poseMap;
                manager.gameMusic = availableLevels[index].song;
                Debug.Log("New level loaded into SquatManager: " + availableLevels[index].levelName);
                PlayerPrefs.SetInt(savedLevelName, index);
                PlayerPrefs.Save();
            }
        }
    }
}