using UnityEngine;
using TMPro;
using System.Collections.Generic;
using PlayerPoseEngine.Scripts;
using Exergames;

namespace SquatGame
{
    [System.Serializable]
    public class SquatLevel
    {
        public string levelName = "New Level";
        public Posemap poseMap;
    }

    public class SquatLevelDropdown : MonoBehaviour
    {
        [Tooltip("Drag your SquatManager here")]
        public PosemapPlayer player;
        
        [Tooltip("Drag your Dropdown here")]
        public TMP_Dropdown dropdown;

        public SquatsGameUI gameUI;

        [Tooltip("Add Levels")]
        public List<SquatLevel> availableLevels;
        private string savedLevelName = "SavedSquatLevel";

        void Start()
        {
            if (player == null || dropdown == null || availableLevels.Count == 0)
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
            if (player != null && player.startMenuButton != null)
            {
                if (!dropdown.gameObject.activeSelf && player.startMenuButton.activeSelf)
                {
                    dropdown.gameObject.SetActive(true); 
                }
            }
        }

        public void OnLevelSelected(int index)
        {

            player.poseMap = availableLevels[index].poseMap;
            Debug.Log("New level loaded into SquatManager: " + availableLevels[index].levelName);
            PlayerPrefs.SetInt(savedLevelName, index);
            PlayerPrefs.Save();
           
        }
    }
}