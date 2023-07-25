using StarterAssets;
using System.Collections;
using System.Collections.Generic;
//using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class ButtonScript : MonoBehaviour
{
    [SerializeField]
    private PlayerNetworkBehaviour playerNetBehave;
    [SerializeField]
    private GameObjectScript gameObjectScript;

    [SerializeField]
    private UiCanvas uiCanvasObj;

    // Start is called before the first frame update
    void Start()
    {
        //uiCanvasObj.SelfieCanvas.gameObject.SetActive(false);
        gameObjectScript.SelfieCanvas.gameObject.SetActive(false);
        //uiCanvasObj.ExitButton.gameObject.SetActive(false);
        gameObjectScript.ExitButton.gameObject.SetActive(false);
    }
}
