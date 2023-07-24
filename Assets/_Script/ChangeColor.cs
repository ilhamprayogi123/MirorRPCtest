using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace StarterAssets
{
    public class ChangeColor : NetworkBehaviour
    {
        private Color objectColor;

        GameObject gimObject;

        void OnMouseDown()
        {
            objectColor = new Color(Random.value, Random.value, Random.value, Random.value);
            gimObject = this.gameObject;

            //NetworkClient.localPlayer.GetComponent<PlayerNetworkBehaviour>().CmdChangeColor(gimObject, objectColor);
        }
    }
}


