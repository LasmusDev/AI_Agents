using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace AIAgent{
    
    public class PlayerEventStorage : MonoBehaviour
    {
        public List<PlayerEventArgs> playerEvents = new List<PlayerEventArgs>();

        private static PlayerEventStorage _instance;
        public static PlayerEventStorage Instance
        {
            get
            {
                if (_instance == null)
                {
                    Instance = GameObject.FindFirstObjectByType<PlayerEventStorage>();
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        public void AddPlayerEvent(PlayerEventArgs playerEvent)
        {
            playerEvents.Add(playerEvent);
        }

        public void AddPlayerEvent(PlayerEventType playerEvent, float pContextFloat, string pContextString)
        {
            playerEvents.Add(new PlayerEventArgs(playerEvent, pContextFloat, pContextString));
        }
    }
    public struct PlayerEventArgs 
    {
        public PlayerEventType eventType;
        public float additionalContextFloat;
        public string additionalContextString;
        public PlayerEventArgs(PlayerEventType eventType, float pContextFloat, string pContextString)
        {
            this.eventType = eventType;
            this.additionalContextFloat = pContextFloat;
            this.additionalContextString = pContextString;
        }
    }
}

public enum PlayerEventType
{
    PLAYERISMOVING,
    PLAYERSTARTEDGAME,
    PLAYERSTOPPEDGAME,
    PLAYERFINISHEDGAME,
    PLAYERSCOREREACHED,
    PLAYERMISSED,
    PLAYERSCOREDCOMBO
}