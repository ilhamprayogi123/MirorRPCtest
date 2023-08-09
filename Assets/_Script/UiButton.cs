using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Get data panel / canvas position for button prefab.
public class UiButton : MonoBehaviour
{
    CanvasGroup _canvasGroup;

    void Awake()
    {
        _canvasGroup = this.GetComponent<CanvasGroup>();

        this.transform.SetParent(GameObject.Find("AnimationPanel").GetComponent<Transform>(), false);
    }
}
