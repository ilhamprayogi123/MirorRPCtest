using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    public class NoAnimButton : MonoBehaviour
    {
        private GameObject thisButton;
        private Button btnAct;

        // Start is called before the first frame update
        void Start()
        {
            thisButton = GetComponentInParent<PlayerNetworkBehaviour>().gameObject;
            //mainPlayer = GameObject.FindGameObjectWithTag("Player");
            btnAct = this.gameObject.GetComponent<Button>();

            //btnAct.onClick.AddListener(() => thisButton.gameObject.GetComponent<PlayerNetworkBehaviour>().noAnswer());
            btnAct.onClick.AddListener(() => thisButton.gameObject.GetComponent<AnimScript>().noAnswer());
        }
    }
}

