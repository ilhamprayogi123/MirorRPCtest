using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiButton : MonoBehaviour
{
    CanvasGroup _canvasGroup;

    void Awake()
    {
        _canvasGroup = this.GetComponent<CanvasGroup>();

        this.transform.SetParent(GameObject.Find("AnimationPanel").GetComponent<Transform>(), false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
