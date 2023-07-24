using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class iconChange : MonoBehaviour
{
    //public SpriteRenderer iconImage;
    public int animIndex;
    private int indexAnim;

    [SerializeField] List<CoupleAnimationData> animationData;

    // Start is called before the first frame update
    private void Start()
    {
        indexAnim = gameObject.GetComponent<iconChange>().animIndex;

        ButtonInput(indexAnim);
    }

    public void ButtonInput(int index)
    {
        //iconImage.sprite = animationData[index].iconImage;
        //gameObject.GetComponentInChildren<Image>().sprite = animationData[index].iconImage;
    }
}
