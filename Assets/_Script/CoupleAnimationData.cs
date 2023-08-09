using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This script is used to create scriptable objects
[CreateAssetMenu(fileName = "New Animation", menuName = "ScriptableOject/Animation Set", order = 1)]
public class CoupleAnimationData : ScriptableObject
{
    public int animateID;
    public string AnimState;
    //public Sprite iconImage;
    public Button button;
    //public List<AnimData> dataAnimation;
}
