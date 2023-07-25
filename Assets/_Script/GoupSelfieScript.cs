using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public class GoupSelfieScript : NetworkBehaviour
    {
        [SerializeField]
        private PlayerNetworkBehaviour playerNet;
        [SerializeField]
        private ValueScript valueScript;
        [SerializeField]
        private UiCanvas uiCanvasObj;
        [SerializeField]
        private GameObjectScript gameObjectScript;

        // Call function after client agree to join Group Selfie
        public void YesJoin()
        {
            //uiCanvasObj.SelfieReqPanel.gameObject.SetActive(false);
            gameObjectScript.SelfieReqPanel.gameObject.SetActive(false);
            valueScript.otherClientIDs = NetworkClient.localPlayer.gameObject.GetComponent<NetworkIdentity>().netId;
            GameObject thisObject = NetworkClient.localPlayer.gameObject;
            CmdYesjoin(valueScript.otherClientIDs, valueScript.testClientID);
        }

        // Command Function to change position and rotation in Server Side
        [Command]
        void CmdYesjoin(uint corePosID, uint centerPosID)
        {
            NetworkIdentity localID = NetworkServer.spawned[corePosID];
            NetworkIdentity posID = NetworkServer.spawned[centerPosID];

            localID.gameObject.GetComponent<Transform>().position = posID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePos[playerNet.selfiePosIndex].transform.position;
            localID.gameObject.GetComponent<Transform>().rotation = posID.gameObject.GetComponent<Transform>().rotation;

            localID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 0;
            RpcYesJoin(localID.connectionToClient, localID, posID);
        }

        // RPC Function to change / transform position and rotation in Client Side
        [TargetRpc]
        void RpcYesJoin(NetworkConnectionToClient netConID, NetworkIdentity netID, NetworkIdentity localPosID)
        {
            //uiCanvasObj.buttonSelfieCanvas.gameObject.SetActive(false);
            gameObjectScript.buttonSelfieCanvas.gameObject.SetActive(false);

            netID.gameObject.GetComponent<Transform>().position = localPosID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePos[playerNet.selfiePosIndex].transform.position;
            netID.gameObject.GetComponent<Transform>().rotation = localPosID.gameObject.GetComponent<Transform>().rotation;

            if (isLocalPlayer)
            {
                //localPosID.gameObject.GetComponent<UiCanvas>().joinButtonCanvas.gameObject.SetActive(false);
                localPosID.gameObject.GetComponent<GameObjectScript>().joinButtonCanvas.gameObject.SetActive(false);
                //localPosID.gameObject.GetComponent<UiCanvas>().ExitButton.gameObject.SetActive(true);
                localPosID.gameObject.GetComponent<GameObjectScript>().ExitButton.gameObject.SetActive(true);
            }

            netID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 0;
            CmdChangeIndex(netID, localPosID);
            //ChangeIndexNum();
        }

        // Command Function to call function to change Index foe Group Selfie
        [Command]
        void CmdChangeIndex(NetworkIdentity netID, NetworkIdentity localID)
        {
            ChangeIndexNum();
        }

        // Change indez for clients for join the group selfie
        void ChangeIndexNum()
        {
            GameObject[] playerTarget = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject playerNum in playerTarget)
            {
                playerNum.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex++;
                playerNum.gameObject.GetComponent<PlayerNetworkBehaviour>().maxIndex = playerNum.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex;
            }
        }

        // Call function after client want to ext the Group Selfie
        public void YesExit()
        {
            //uiCanvasObj.ExitRequestPanel.gameObject.SetActive(false);
            gameObjectScript.ExitRequestPanel.gameObject.SetActive(false);
            valueScript.otherClientIDs = NetworkClient.localPlayer.gameObject.GetComponent<NetworkIdentity>().netId;
            GameObject thisObject = NetworkClient.localPlayer.gameObject;
            CmdExitGroup(valueScript.otherClientIDs, valueScript.testClientID);
        }

        // Command function to cal RPC funtion for exit the group and enable client move in server side
        [Command(requiresAuthority = false)]
        void CmdExitGroup(uint corePosID, uint centerPosID)
        {
            NetworkIdentity localID = NetworkServer.spawned[corePosID];
            NetworkIdentity posID = NetworkServer.spawned[centerPosID];

            localID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 2;
            RpcYesExitGroup(localID.connectionToClient, localID, posID);
        }

        // Enable clients control after choosing to exit the group selfie, also aneble the selfie button and join button
        [TargetRpc]
        void RpcYesExitGroup(NetworkConnectionToClient netConID, NetworkIdentity netID, NetworkIdentity localPosID)
        {
            //uiCanvasObj.buttonSelfieCanvas.gameObject.SetActive(true);
            gameObjectScript.buttonSelfieCanvas.gameObject.SetActive(true);

            if (isLocalPlayer)
            {
                //localPosID.gameObject.GetComponent<UiCanvas>().joinButtonCanvas.gameObject.SetActive(true);
                localPosID.gameObject.GetComponent<GameObjectScript>().joinButtonCanvas.gameObject.SetActive(true);
                //localPosID.gameObject.GetComponent<UiCanvas>().ExitButton.gameObject.SetActive(false);
                localPosID.gameObject.GetComponent<GameObjectScript>().ExitButton.gameObject.SetActive(false);
            }
            netID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 2;
        }
    }
}
