using UnityEngine;

namespace YogaGame
{
    // This class is used to identify the body part of the player that is being tracked by the game
    public enum BodyPartType 
    { 
        Head, 
        LeftHand, 
        RightHand 
    }

    public class YogaBodyPart : MonoBehaviour
    {
        public BodyPartType myType;
    }
}