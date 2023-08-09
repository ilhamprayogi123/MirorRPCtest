using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is used to get data from the canvas which will be used as a place for the answer button to appear.
public class RequestButton : MonoBehaviour
{
    CanvasGroup _canvasGroup;
    [SerializeField]
    private GameObject buttonCanvas;

    void Awake()
    {
        _canvasGroup = this.GetComponent<CanvasGroup>();

        this.transform.SetParent(GameObject.Find("RequestPanel").GetComponent<Transform>(), false);
    }

    // Start is called before the first frame update
    void Start()
    {
        buttonCanvas = GameObject.Find("GreetRequestCanvas");
    }

    public void OffCanvas()
    {
        buttonCanvas.gameObject.SetActive(false);
    }
}
