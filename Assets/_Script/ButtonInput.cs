using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace StarterAssets
{
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
