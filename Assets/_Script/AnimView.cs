using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimView : MonoBehaviour
{
    public int animID;
    public string animState;

    public List<CoupleAnimationData> animateData;

    public void ClapAnim(int animIndex)
    {
        animID = animateData[animIndex].animateID;
        animState = animateData[animIndex].AnimState;
    }
}
