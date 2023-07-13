using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace StarterAssets
{
    public class ClickScript : NetworkBehaviour
    {
        GameObject gimObject;

        private void OnMouseDown()
        {
            if (!isLocalPlayer)
            {
                //gimObject = this.gameObject;

                //NetworkClient.localPlayer.GetComponent<PlayerNetworkBehaviour>().CmdClick(gimObject);
            }
        }
    }
}


