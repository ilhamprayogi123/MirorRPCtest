using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace StarterAssets
{
    // This script functions to call a function to save the client name and also change the name that appears in the game scene.
    public class ButtonInput : NetworkBehaviour
    {
        public Button assignButton;

        // Start is called before the first frame update
        void Start()
        {
            assignButton.onClick.AddListener(assignName);
        }

        void assignName()
        {
            NetworkClient.localPlayer.GetComponent<PlayerNetworkBehaviour>().assignAct();
        }
    }
}
