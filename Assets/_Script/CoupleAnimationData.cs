using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Animation", menuName = "ScriptableOject/Animation Set", order = 1)]
public class CoupleAnimationData : ScriptableObject
{
    public int animateID;
    public string AnimState;
    //public Sprite iconImage;
    public Button button;
    //public List<AnimData> dataAnimation;
}
