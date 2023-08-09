using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    // This script is used to get a function to call the animation according to the button used.
    public class GetButtonFunc : MonoBehaviour
    {
        private GameObject thisButton;
        private Canvas thisCanvas;
        private Button btnAct;
        private GameObject mainPlayer;
        private PlayerNetworkBehaviour playerNet;
        //public bool activeButton = true;
        public int typeButtton;

        // Start is called before the first frame update
        void Start()
        {
            //thisCanvas = GetComponentInParent<Canvas>();
            thisButton = GetComponentInParent<PlayerNetworkBehaviour>().gameObject;
            //mainPlayer = GameObject.FindGameObjectWithTag("Player");
            btnAct = this.gameObject.GetComponent<Button>();

            //btnAct.onClick.AddListener(() => thisButton.gameObject.GetComponent<PlayerNetworkBehaviour>().AnimationButton(typeButtton));

            btnAct.onClick.AddListener(() => thisButton.gameObject.GetComponent<UiCanvas>().AnimationButton(typeButtton));
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                Debug.Log(thisButton.gameObject.name);
            }
        }
    }
}


