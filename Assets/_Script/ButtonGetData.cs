using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    // This script is useful for getting data from the YesAnswer function so that it can be used by the Yes Button in the Couple Animation Request part.
    public class ButtonGetData : MonoBehaviour
    {
        private GameObject thisButton;
        private Canvas thisCanvas;
        private Button btnAct;
        private GameObject mainPlayer;
        private PlayerNetworkBehaviour playerNet;
        
        // Start is called before the first frame update
        void Start()
        {
            thisButton = GetComponentInParent<PlayerNetworkBehaviour>().gameObject;
            btnAct = this.gameObject.GetComponent<Button>();

            btnAct.onClick.AddListener(() => thisButton.gameObject.GetComponent<AnimScript>().YesAnswer());
        }
    }
}


