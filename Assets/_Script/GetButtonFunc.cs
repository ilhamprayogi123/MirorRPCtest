using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
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
            
            if (typeButtton == 1)
            {
                btnAct.onClick.AddListener(() => thisButton.gameObject.GetComponent<PlayerNetworkBehaviour>().GreetButton());
            }
            else if (typeButtton == 2)
            {
                btnAct.onClick.AddListener(() => thisButton.gameObject.GetComponent<PlayerNetworkBehaviour>().ClapButton());
            }
            else if (typeButtton == 3)
            {
                btnAct.onClick.AddListener(() => thisButton.gameObject.GetComponent<PlayerNetworkBehaviour>().DanceBUtton());
            }
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


