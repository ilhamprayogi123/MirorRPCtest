using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SocialPlatforms;

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
            Debug.Log(playerNet.loc);
            gameObjectScript.SelfieReqPanel.gameObject.SetActive(false);
            valueScript.otherClientIDs = NetworkClient.localPlayer.gameObject.GetComponent<NetworkIdentity>().netId;
            GameObject thisObject = NetworkClient.localPlayer.gameObject;
            CmdYesjoin(valueScript.otherClientIDs, valueScript.testClientID, playerNet.loc);
        }

        // Command Function to change position and rotation in Server Side
        [Command]
        void CmdYesjoin(uint corePosID, uint centerPosID, int loc)
        {
            NetworkIdentity localID = NetworkServer.spawned[corePosID];
            NetworkIdentity posID = NetworkServer.spawned[centerPosID];

            int intLoc = posID.gameObject.GetComponent<PlayerNetworkBehaviour>().loc;

            if (posID.gameObject.GetComponent<ValueScript>().anySpace == true)
            {
                localID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = posID.gameObject.GetComponent<ValueScript>().indexSaved;
                posID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = posID.gameObject.GetComponent<ValueScript>().numIndex[intLoc];
                valueScript.localNets = posID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex;
                localID.gameObject.GetComponent<Transform>().position = posID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePos[valueScript.localNets].transform.position;
                localID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = valueScript.localNets;
            }
            else if (posID.gameObject.GetComponent<ValueScript>().isContinue == true && posID.gameObject.GetComponent<ValueScript>().anySpace == true)
            {
                localID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = posID.gameObject.GetComponent<ValueScript>().indexContinue;
                posID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = posID.gameObject.GetComponent<ValueScript>().indexContinue;
                localID.gameObject.GetComponent<Transform>().position = posID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePos[playerNet.selfiePosIndex].transform.position;
            }
            else
            {
                localID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = posID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex;
                localID.gameObject.GetComponent<Transform>().position = posID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePos[playerNet.selfiePosIndex].transform.position;
            }

            localID.gameObject.GetComponent<Transform>().rotation = posID.gameObject.GetComponent<Transform>().rotation;
            localID.gameObject.GetComponent<ValueScript>().GroupID = posID.gameObject.GetComponent<ValueScript>().GroupID;
            posID.gameObject.GetComponent<GameObjectScript>().unityGameObjects.Add(localID.gameObject);

            localID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 0;
            localID.gameObject.GetComponent<ThirdPersonController>().SprintSpeed = 0;
            
            RpcYesJoin(localID.connectionToClient, localID, posID, intLoc);
        }

        // RPC Function to change / transform position and rotation in Client Side.
        [TargetRpc]
        void RpcYesJoin(NetworkConnectionToClient netConID, NetworkIdentity netID, NetworkIdentity localPosID, int intNet)
        {
            gameObjectScript.buttonSelfieCanvas.gameObject.SetActive(false);

            if (localPosID.gameObject.GetComponent<ValueScript>().anySpace == true)
            {
                netID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = localPosID.gameObject.GetComponent<ValueScript>().indexSaved;
                localPosID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = localPosID.gameObject.GetComponent<ValueScript>().numIndex[intNet];
                valueScript.localNets = localPosID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex;
                netID.gameObject.GetComponent<Transform>().position = localPosID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePos[valueScript.localNets].transform.position;
                netID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = valueScript.localNets;
                CmdIndexArc(localPosID, intNet);
            }
            else if (localPosID.gameObject.GetComponent<ValueScript>().isContinue == true && localPosID.gameObject.GetComponent<ValueScript>().anySpace == true)
            {
                netID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = localPosID.gameObject.GetComponent<ValueScript>().indexContinue;
                localPosID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = localPosID.gameObject.GetComponent<ValueScript>().indexContinue;
                netID.gameObject.GetComponent<Transform>().position = localPosID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePos[playerNet.selfiePosIndex].transform.position;
                localPosID.gameObject.GetComponent<ValueScript>().isContinue = false;
            }
            else
            {
                Debug.Log("Test 3");
                netID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = localPosID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex;
                netID.gameObject.GetComponent<Transform>().position = localPosID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePos[playerNet.selfiePosIndex].transform.position;
            }

            netID.gameObject.GetComponent<Transform>().rotation = localPosID.gameObject.GetComponent<Transform>().rotation;

            if (isLocalPlayer)
            {
                localPosID.gameObject.GetComponent<GameObjectScript>().joinButtonCanvas.gameObject.SetActive(false);
                localPosID.gameObject.GetComponent<GameObjectScript>().ExitButton.gameObject.SetActive(true);
            }

            netID.gameObject.GetComponent<ValueScript>().GroupID = localPosID.gameObject.GetComponent<ValueScript>().GroupID;
            localPosID.gameObject.GetComponent<GameObjectScript>().unityGameObjects.Add(netID.gameObject);
            netID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 0;
            netID.gameObject.GetComponent<ThirdPersonController>().SprintSpeed = 0;
            CmdChangeIndex(netID, localPosID);
        }

        // Increase loc for client who provide group selfie
        [Command(requiresAuthority = false)]
        void CmdIndexArc(NetworkIdentity netID, int index)
        {
            Debug.Log("Testing : " + index);
            index++;
            netID.gameObject.GetComponent<PlayerNetworkBehaviour>().loc = index;
            RpcIndexArc(netID, index);
        }

        // Change loc value for client who provide group selfie in other client side
        [ClientRpc]
        void RpcIndexArc(NetworkIdentity netID, int index)
        {
            Debug.Log(index);
            netID.gameObject.GetComponent<PlayerNetworkBehaviour>().loc = index;
            //CmdIntIndex(netID, index);
        }

        // Command Function to call function to change Index foe Group Selfie and add gameobject to list
        [Command]
        void CmdChangeIndex(NetworkIdentity netID, NetworkIdentity localID)
        {
            localID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex++;
            localID.gameObject.GetComponent<PlayerNetworkBehaviour>().countNum++;

            RpcTargetChangeIndex(localID.connectionToClient, localID);
            RpcGroup(localID.connectionToClient, netID, localID);
        }

        // Change index bool in local client
        [TargetRpc]
        void RpcTargetChangeIndex(NetworkConnectionToClient netCon, NetworkIdentity netID)
        {
            netID.gameObject.GetComponent<ValueScript>().changeIndex = true;
            CmdIndexTex(netID);
        }

        // Rpc Function to add gameobject to list for group selfie
        [TargetRpc]
        void RpcGroup(NetworkConnectionToClient netCon, NetworkIdentity localID, NetworkIdentity netID)
        {
            netID.gameObject.GetComponent<GameObjectScript>().unityGameObjects.Add(localID.gameObject);
        }

        // Call Rpc function to change count text
        [Command(requiresAuthority = false)]
        void CmdIndexTex(NetworkIdentity netID)
        {
            RpcTextIndex(netID);
        }

        // Change count text for other client
        [ClientRpc]
        void RpcTextIndex(NetworkIdentity netID)
        {
            netID.gameObject.GetComponent<ValueScript>().changeIndex = true;
            netID.gameObject.GetComponent<PlayerNetworkBehaviour>().currentText.SetText(netID.gameObject.GetComponent<PlayerNetworkBehaviour>().countNum.ToString());
        }

        // Call function after client want to exit the Group Selfie
        public void YesExit()
        {
            gameObjectScript.ExitRequestPanel.gameObject.SetActive(false);
            valueScript.otherClientIDs = NetworkClient.localPlayer.gameObject.GetComponent<NetworkIdentity>().netId;

            valueScript.GroupID = Convert.ToInt32(valueScript.otherClientIDs);
            GameObject thisObject = NetworkClient.localPlayer.gameObject;
            valueScript.indexSaved = NetworkClient.localPlayer.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex;
             
            CmdIndexSaved(valueScript.otherClientIDs, valueScript.testClientID, valueScript.indexSaved);
            CmdExitGroup(valueScript.otherClientIDs, valueScript.testClientID, valueScript.GroupID);
        }

        // Command function to save index for data position
        [Command]
        void CmdIndexSaved(uint corePosID, uint centerPosID, int indexSaved)
        {
            NetworkIdentity localID = NetworkServer.spawned[corePosID];
            NetworkIdentity posID = NetworkServer.spawned[centerPosID];

            if (posID.gameObject.GetComponent<ValueScript>().readyChange == true)
            {
                if (posID.gameObject.GetComponent<ValueScript>().isNext == false)
                {
                    posID.gameObject.GetComponent<ValueScript>().anySpace = true;
                    posID.gameObject.GetComponent<ValueScript>().indexSaved = indexSaved;
                    posID.gameObject.GetComponent<ValueScript>().currentIndex = posID.gameObject.GetComponent<PlayerNetworkBehaviour>().countNum;
                    posID.gameObject.GetComponent<PlayerNetworkBehaviour>().currentIndex = posID.gameObject.GetComponent<ValueScript>().currentIndex;

                    posID.gameObject.GetComponent<ValueScript>().indexContinue = posID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex;
                    posID.gameObject.GetComponent<ValueScript>().isNext = true;
                }
                else if (posID.gameObject.GetComponent<ValueScript>().isNext == true)
                {
                    posID.gameObject.GetComponent<ValueScript>().anySpace = true;
                    posID.gameObject.GetComponent<ValueScript>().indexSaved = indexSaved;
                }

                int continueInt = posID.gameObject.GetComponent<ValueScript>().indexContinue;
                int currentint = posID.gameObject.GetComponent<ValueScript>().currentIndex;

                posID.gameObject.GetComponent<ValueScript>().numIndex.Add(indexSaved);

                RpcLocalSave(localID.connectionToClient, posID, indexSaved, continueInt, currentint);
            }
        }

        // Rpc function to save data position in local client
        [TargetRpc]
        void RpcLocalSave(NetworkConnectionToClient netConn, NetworkIdentity netID, int indexSaved, int continueIndex, int currentIndex)
        {
            Debug.Log("Local Save");

            if (netID.gameObject.GetComponent<ValueScript>().isNext == false)
            {
                netID.gameObject.GetComponent<ValueScript>().indexSaved = indexSaved;
                netID.gameObject.GetComponent<ValueScript>().currentIndex = currentIndex;
                netID.gameObject.GetComponent<PlayerNetworkBehaviour>().currentIndex = currentIndex;
                netID.gameObject.GetComponent<ValueScript>().indexContinue = continueIndex;
                netID.gameObject.GetComponent<ValueScript>().anySpace = true;
                netID.gameObject.GetComponent<ValueScript>().isNext = true;
            }
            else if (netID.gameObject.GetComponent<ValueScript>().isNext == true)
            {
                netID.gameObject.GetComponent<ValueScript>().anySpace = true;
                netID.gameObject.GetComponent<ValueScript>().indexSaved = indexSaved;
            }
            CmdAllSaved(netID, indexSaved, continueIndex, currentIndex);
        }

        // Call Rpc Fuction to save position data in other client side
        [Command]
        void CmdAllSaved(NetworkIdentity netID, int indexSaved, int continueIndex, int currentIndex)
        {
            Debug.Log("Test Save All Local");
            RpcSavedIndex(netID, indexSaved, continueIndex, currentIndex);
        }

        // Rpc function to save position data in clint who provide group selfie for other client side
        [ClientRpc]
        void RpcSavedIndex(NetworkIdentity netID, int indexSaved, int continueIndex, int currentIndex)
        {
            Debug.Log("Rpc Saved");

            if (netID.gameObject.GetComponent<ValueScript>().isNext == false)
            {
                netID.gameObject.GetComponent<ValueScript>().anySpace = true;
                netID.gameObject.GetComponent<ValueScript>().indexSaved = indexSaved;
                netID.gameObject.GetComponent<ValueScript>().currentIndex = currentIndex;
                netID.gameObject.GetComponent<PlayerNetworkBehaviour>().currentIndex = currentIndex;
                netID.gameObject.GetComponent<ValueScript>().indexContinue = continueIndex;

                netID.gameObject.GetComponent<ValueScript>().isNext = true;
            }
            else if (netID.gameObject.GetComponent<ValueScript>().isNext == true)
            {
                netID.gameObject.GetComponent<ValueScript>().anySpace = true;
                netID.gameObject.GetComponent<ValueScript>().indexSaved = indexSaved;
            }
            netID.gameObject.GetComponent<ValueScript>().numIndex.Add(indexSaved);
        }

        // Command function to cal RPC funtion for exit the group and enable client move in server side
        [Command(requiresAuthority = false)]
        void CmdExitGroup(uint corePosID, uint centerPosID, int grouIDs)
        {
            NetworkIdentity localID = NetworkServer.spawned[corePosID];
            NetworkIdentity posID = NetworkServer.spawned[centerPosID];

            localID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 2;
            localID.gameObject.GetComponent<ThirdPersonController>().SprintSpeed = 5.335f;
            localID.gameObject.GetComponent<ValueScript>().GroupID = grouIDs;

            if (posID.gameObject.GetComponent<ValueScript>().readyChange == true)
            {
                if (posID.gameObject.GetComponent<ValueScript>().isNext == false)
                {
                    posID.gameObject.GetComponent<ValueScript>().anySpace = true;
                    posID.gameObject.GetComponent<ValueScript>().indexSaved = localID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex;
                    posID.gameObject.GetComponent<ValueScript>().currentIndex = posID.gameObject.GetComponent<PlayerNetworkBehaviour>().countNum;
                    posID.gameObject.GetComponent<ValueScript>().indexContinue = posID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex;

                    posID.gameObject.GetComponent<ValueScript>().isNext = true;
                }
                else if (posID.gameObject.GetComponent<ValueScript>().isNext == true)
                {
                    posID.gameObject.GetComponent<ValueScript>().anySpace = true;

                    posID.gameObject.GetComponent<ValueScript>().indexSaved = localID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex;
                }
            }

            localID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = 0;
            RpcYesExitGroup(localID.connectionToClient, localID, posID, grouIDs);
        }

        // Enable clients control after choosing to exit the group selfie, also aneble the selfie button and join button
        [TargetRpc]
        void RpcYesExitGroup(NetworkConnectionToClient netConID, NetworkIdentity netID, NetworkIdentity localPosID, int groupIDs)
        {
            netID.gameObject.GetComponent<GameObjectScript>().buttonSelfieCanvas.gameObject.SetActive(true);

            if (isLocalPlayer)
            {
                localPosID.gameObject.GetComponent<GameObjectScript>().joinButtonCanvas.gameObject.SetActive(true);
                localPosID.gameObject.GetComponent<GameObjectScript>().ExitButton.gameObject.SetActive(false);
            }

            netID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 2;
            netID.gameObject.GetComponent<ThirdPersonController>().SprintSpeed = 5.335f;
            netID.gameObject.GetComponent<ValueScript>().GroupID = groupIDs;

            if (localPosID.gameObject.GetComponent<ValueScript>().readyChange == true)
            {
                if (localPosID.gameObject.GetComponent<ValueScript>().isNext == false)
                {
                    localPosID.gameObject.GetComponent<ValueScript>().anySpace = true;
                    localPosID.gameObject.GetComponent<ValueScript>().indexSaved = netID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex;
                    localPosID.gameObject.GetComponent<ValueScript>().currentIndex = localPosID.gameObject.GetComponent<PlayerNetworkBehaviour>().countNum;
                    localPosID.gameObject.GetComponent<ValueScript>().indexContinue = localPosID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex;

                    localPosID.gameObject.GetComponent<ValueScript>().isNext = true;
                }
                else if (localPosID.gameObject.GetComponent<ValueScript>().isNext == true)
                {
                    localPosID.gameObject.GetComponent<ValueScript>().anySpace = true;
                    localPosID.gameObject.GetComponent<ValueScript>().indexSaved = netID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex;
                }
            }

            netID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = 0;
            CmdChangeIndexMinus(netID, localPosID);
        }

        // Command Function to call function to change Index for Group Selfie
        [Command]
        void CmdChangeIndexMinus(NetworkIdentity netID, NetworkIdentity localID)
        {
            if (localID.gameObject.GetComponent<ValueScript>().readyChange == true)
            {
                localID.gameObject.GetComponent<ValueScript>().anySpace = true;
            }

            localID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex--;
            localID.gameObject.GetComponent<PlayerNetworkBehaviour>().countNum--;
            
            RpcTargetChangeIndexMinus(localID.connectionToClient, localID);
        }

        // Rpc function to change index in one of the client who open the group selfie
        [TargetRpc]
        void RpcTargetChangeIndexMinus(NetworkConnectionToClient netCon, NetworkIdentity netID)
        {
            if (netID.gameObject.GetComponent<ValueScript>().readyChange == true)
            {
                netID.gameObject.GetComponent<ValueScript>().anySpace = true;
            }

            //netID.gameObject.GetComponent<PlayerNetworkBehaviour>().MinIndex();
            netID.gameObject.GetComponent<ValueScript>().changeIndex = true;
            CmdIndexTexMin(netID);
        }

        // Command function to call Rpc for changing text
        [Command(requiresAuthority = false)]
        void CmdIndexTexMin(NetworkIdentity netID)
        {
            RpcTextIndexMin(netID);
        }

        // Rpc function that called for change counting text after one of the client is leaving the group and also set bool function when there are player leaves the group
        [ClientRpc]
        void RpcTextIndexMin(NetworkIdentity netID)
        {
            netID.gameObject.GetComponent<GameObjectScript>().SelfieCanvas.gameObject.SetActive(true);
            
            if (netID.gameObject.GetComponent<ValueScript>().readyChange == true)
            {
                netID.gameObject.GetComponent<ValueScript>().anySpace = true;
            }
            
            netID.gameObject.GetComponent<GameObjectScript>().joinButtonCanvas.gameObject.SetActive(true);

            if (isLocalPlayer)
            {
                gameObjectScript.joinButtonCanvas.gameObject.SetActive(false);
            }
            
            netID.gameObject.GetComponent<PlayerNetworkBehaviour>().currentText.SetText(netID.gameObject.GetComponent<PlayerNetworkBehaviour>().countNum.ToString());
        }
    }
}
