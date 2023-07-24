using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
