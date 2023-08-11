using Mirror;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace StarterAssets
{
    // This script is used to set the main UI and some values for using the Group Selfie feature, especially for opening and closing panels.
    public class GroupSelfieCanvas : NetworkBehaviour
    {
        //public PlayerNetworkBehaviour playerNetBehave;
        [SerializeField]
        private ValueScript valueScript;
        [SerializeField]
        private GameObjectScript gameObjectScript;
        [SerializeField]
        private GroupSelfieGameObj groupSelfieObj;
        [SerializeField]
        private GroupSelfieManager groupSelfieManager;

        // Start is called before the first frame update
        void Start()
        {
            groupSelfieObj.SelfieCanvas.gameObject.SetActive(false);
            groupSelfieObj.ExitButton.gameObject.SetActive(false);

            if (isLocalPlayer)
            {
                groupSelfieObj.buttonSelfieCanvas.gameObject.SetActive(true);
                groupSelfieObj.closeSelfieCanvas.gameObject.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {
            groupSelfieObj.SelfieCanvas.transform.LookAt(Camera.main.transform);

            if (!isLocalPlayer)
            {
                groupSelfieObj.buttonSelfieCanvas.SetActive(false);
            }
        }

        // Open close selfie canvas and close selfie canvas
        public void SelfieButton()
        {
            if (isLocalPlayer)
            {
                groupSelfieObj.buttonSelfieCanvas.gameObject.SetActive(false);
                groupSelfieObj.closeSelfieCanvas.gameObject.SetActive(true);

                //Debug.Log("Max Player is " + playerNetBehave.maxIndex);
                Debug.Log("Max Player is " + groupSelfieManager.maxIndex);
                SelfieButtonFunc();
            }
        }

        // Reset value for client in the group
        public void GetAllGroupID(float speed, int num, float sprintSpeed)
        {
            GameObject thisObj = this.gameObject;
            
            foreach (GameObject playerNumObj in groupSelfieManager.SavedPosition)
            {
                playerNumObj.gameObject.GetComponent<GroupSelfieGameObj>().buttonSelfieCanvas.gameObject.SetActive(true);
                playerNumObj.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = speed;
                playerNumObj.gameObject.GetComponent<ThirdPersonController>().SprintSpeed = sprintSpeed;
                //playerNumObj.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = num;
                playerNumObj.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex = num;

                playerNumObj.gameObject.GetComponent<GroupSelfieCanvas>().CloseSelfieBool();
                playerNumObj.gameObject.GetComponent<GroupSelfieCanvas>().CmdReset(num);
            }
        }

        // Call Command Function to open group selfie canvas
        public void SelfieButtonFunc()
        {
            if (isLocalPlayer)
            {
                valueScript.localeSelfieID = this.gameObject.GetComponent<NetworkIdentity>().netId;
                valueScript.GroupID = Convert.ToInt32(valueScript.localeSelfieID);
                //gameObjectScript.joinButtonCanvas.gameObject.SetActive(false);
                groupSelfieObj.joinButtonCanvas.gameObject.SetActive(false);
                //gameObjectScript.ExitButton.gameObject.SetActive(false);
                groupSelfieObj.ExitButton.gameObject.SetActive(false);
                //Debug.Log("You max index is " + playerNetBehave.maxIndex);
                Debug.Log("You max index is " + groupSelfieManager.maxIndex);
                //CmdSelfieLocalePanel(valueScript.localeSelfieID, valueScript.GroupID, playerNetBehave.maxIndex);
                CmdSelfieLocalePanel(valueScript.localeSelfieID, valueScript.GroupID, groupSelfieManager.maxIndex);
            }
        }

        // Command function to call RPC function for open the selfie froup canvas.
        [Command]
        void CmdSelfieLocalePanel(uint localNetID, int groupID, int maxIndex)
        {
            NetworkIdentity localeNetID = NetworkServer.spawned[localNetID];
            localeNetID.gameObject.GetComponent<ValueScript>().GroupID = groupID;
            
            for (int i = 0; i < maxIndex; i++)
            {
                localeNetID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[i].gameObject.SetActive(true);
            }
            
            //localeNetID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[valueScript.localNets].gameObject.SetActive(true);

            localeNetID.gameObject.GetComponent<GroupSelfieManager>().maxIndex = maxIndex;
            localeNetID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 0;
            localeNetID.gameObject.GetComponent<ThirdPersonController>().SprintSpeed = 0;
            //GetMaxIndex();
            RpcSelfieLocal(localeNetID.connectionToClient, localeNetID, groupID, maxIndex);
        }

        // Open selfie group panel in local player and disable client move
        [TargetRpc]
        void RpcSelfieLocal(NetworkConnectionToClient netConID, NetworkIdentity localNetID, int groupID, int maxIndex)
        {
            localNetID.gameObject.GetComponent<GroupSelfieGameObj>().SelfieCanvas.gameObject.SetActive(true);
            localNetID.gameObject.GetComponent<ValueScript>().GroupID = groupID;
            
            for (int i = 0; i < maxIndex; i++)
            {
                localNetID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[i].gameObject.SetActive(true);
            }
            
            //localNetID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[valueScript.localNets].gameObject.SetActive(true);

            localNetID.gameObject.GetComponent<GroupSelfieManager>().maxIndex = maxIndex;
            localNetID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 0;
            localNetID.gameObject.GetComponent<ThirdPersonController>().SprintSpeed = 0;

            CmdSelfiePanelOther(localNetID, maxIndex);
        }

        // Call RPC function to display Selfie Panel from Server
        [Command]
        void CmdSelfiePanelOther(NetworkIdentity localId, int maxIndex)
        {
            RpcPanelSelfie(localId, maxIndex);
        }

        // Display locale client selfie panel to all others client
        [ClientRpc]
        void RpcPanelSelfie(NetworkIdentity localeID, int maxIndex)
        {
            localeID.gameObject.GetComponent<GroupSelfieGameObj>().SelfieCanvas.gameObject.SetActive(true);
            localeID.gameObject.GetComponent<GroupSelfieManager>().maxIndex = maxIndex;
            //localeID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[valueScript.localNets].gameObject.SetActive(true);
            
            for (int i = 0; i < maxIndex; i++)
            {
                localeID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[i].gameObject.SetActive(true);
            }
            
            if (!isLocalPlayer)
            {
                groupSelfieObj.joinButtonCanvas.gameObject.SetActive(true);
            }
        }

        // Reset bool variable in server
        [Command(requiresAuthority = false)]
        void CloseSelfieBool()
        {
            valueScript.ResetBool();

            RpcCloseSelfieBool();
        }

        // Reset bool variable in client
        [ClientRpc]
        void RpcCloseSelfieBool()
        {
            valueScript.ResetBool();
        }

        // Command function to reset selfie pos index
        [Command(requiresAuthority = false)]
        void CmdReset(int num)
        {
            gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex = num;

            RpcReset(num);
        }

        // Reset selfie pos index
        [ClientRpc]
        void RpcReset(int num)
        {
            gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex = num;
        }

        // Call command to join the group selfie and open the join group request canvas
        public void JoinSelfie()
        {
            var other = FindObjectOfType<ThirdPersonController>().GetComponent<NetworkIdentity>();
            var thisObj = GetComponent<NetworkIdentity>();

            GameObject localPlayer = NetworkClient.localPlayer.gameObject;
            Debug.Log(localPlayer.name);
            Debug.Log(thisObj.gameObject.name);
            valueScript.testClientID = thisObj.netId;
            valueScript.testID = localPlayer.gameObject.GetComponent<NetworkIdentity>().netId;

            CmdSelfieReqPanel(valueScript.testID, valueScript.testClientID);
        }

        // Call RPC function to open join group request panel for the clicnt who clicked to button from server
        [Command(requiresAuthority = false)]
        void CmdSelfieReqPanel(uint otherID, uint ClientID)
        {
            NetworkIdentity otherClientID = NetworkServer.spawned[otherID];
            valueScript.testID = otherID;
            valueScript.testClientID = ClientID;
            RpcSelfieReqPanel(otherClientID.connectionToClient, otherClientID, ClientID);
        }

        // Open Join Group Selfie panel in client who clicked the join button 
        [TargetRpc]
        void RpcSelfieReqPanel(NetworkConnectionToClient netConID, NetworkIdentity netOtherID, uint clientID)
        {
            //netOtherID.gameObject.GetComponent<GameObjectScript>().SelfieReqPanel.gameObject.SetActive(true);
            netOtherID.gameObject.GetComponent<GroupSelfieGameObj>().SelfieReqPanel.gameObject.SetActive(true);
            netOtherID.gameObject.GetComponent<ValueScript>().testClientID = clientID;
            valueScript.testID = netOtherID.gameObject.GetComponent<NetworkIdentity>().netId;
        }

        // Call Command Function to exit group and open exit group request panel
        public void ExitGroupButton()
        {
            var thisObj = GetComponent<NetworkIdentity>();

            GameObject localPlayer = NetworkClient.localPlayer.gameObject;

            valueScript.testClientID = thisObj.netId;
            valueScript.testID = localPlayer.gameObject.GetComponent<NetworkIdentity>().netId;
            CmdExitReqPanel(valueScript.testID, valueScript.testClientID);
        }

        // Call RPC function to open exit group request panel in client who clicked the exit button from server
        [Command(requiresAuthority = false)]
        void CmdExitReqPanel(uint otherID, uint ClientID)
        {
            NetworkIdentity otherClientID = NetworkServer.spawned[otherID];
            valueScript.testID = otherID;
            valueScript.testClientID = ClientID;
            RpcExitReqPanel(otherClientID.connectionToClient, otherClientID, ClientID);
        }

        // Open exit group request panel in client who clicked the exit button
        [TargetRpc]
        void RpcExitReqPanel(NetworkConnectionToClient netConID, NetworkIdentity netOtherID, uint clientID)
        {
            netOtherID.gameObject.GetComponent<GroupSelfieGameObj>().ExitRequestPanel.gameObject.SetActive(true);
            netOtherID.gameObject.GetComponent<ValueScript>().testClientID = clientID;
            valueScript.testID = netOtherID.gameObject.GetComponent<NetworkIdentity>().netId;
        }

        // Open selfie canvas and close the close selfie canvas
        public void CloseSelfie()
        {
            if (isLocalPlayer)
            {
                groupSelfieObj.buttonSelfieCanvas.gameObject.SetActive(true);
                groupSelfieObj.closeSelfieCanvas.gameObject.SetActive(false);
                CloseSelfieGroup();
            }
        }

        // Function to call Ommand Function to Close Group Selfie
        public void CloseSelfieGroup()
        {
            if (isLocalPlayer)
            {
                valueScript.localeSelfieID = this.gameObject.GetComponent<NetworkIdentity>().netId;
                CmdSlefieCloselocalePanel(valueScript.localeSelfieID);
            }
        }

        // Command function to call RPC function for closing the group selfie
        [Command]
        void CmdSlefieCloselocalePanel(uint localNetID)
        {
            NetworkIdentity localeNetID = NetworkServer.spawned[localNetID];
            int reset = 0;

            int selfCircle = localeNetID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;
            Debug.Log(selfCircle);
            //localeNetID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[selfCircle].gameObject.SetActive(false);
            for (int i = 0; i <= localeNetID.gameObject.GetComponent<ValueScript>().maxIndex; i++)
            {
                localeNetID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[i].gameObject.SetActive(false);
            }

            //RpcLocaleSelfAll(localeNetID.connectionToClient, localeNetID, selfCircle);
            localeNetID.gameObject.GetComponent<ValueScript>().ResetValue();
            //localeNetID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[groupSelfieManager.selfiePosIndex].gameObject.SetActive(true);
            
            localeNetID.gameObject.GetComponent<GroupSelfieManager>().loc = reset;
            localeNetID.gameObject.GetComponent<GroupSelfieManager>().SavedPosition.Clear();

            GetAllGroupID(localeNetID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed, localeNetID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex, localeNetID.gameObject.GetComponent<ThirdPersonController>().SprintSpeed);
            //ResetLoc(reset);
            RpcCLose(localeNetID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed, localeNetID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex, localeNetID.gameObject.GetComponent<ThirdPersonController>().SprintSpeed);
            RpcSelfieLocalClose(localeNetID.connectionToClient, localeNetID, selfCircle);
        }

        // Call rpc function to close circle indicator in local client
        [TargetRpc]
        void RpcLocaleSelfAll(NetworkConnectionToClient netCon, NetworkIdentity netID, int selfInt)
        {
            //int selfCircle = localNetID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;
            //Debug.Log(selfCircle);
            netID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[selfInt].gameObject.SetActive(false);
            RpcCircleCloseAll(netID, selfInt);
        }

        // Comman function to close circle indicator in server side
        [Command]
        void RpcCircleCloseAll(NetworkIdentity netID, int selfInt)
        {
            RpcCloseLocaleSelfCircleAll(netID, selfInt);
        }

        // Rpc function to close circle indicator for local client in all other client view
        [ClientRpc]
        void RpcCloseLocaleSelfCircleAll(NetworkIdentity netID, int selfInt)
        {
            netID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[selfInt].gameObject.SetActive(false);
        }

        // Call Reset value function for all client
        [ClientRpc]
        void RpcCLose(float speed, int num, float sprintSpeed)
        {
            GetAllGroupID(speed, num, sprintSpeed);
        }

        // Close selfie group and enable client move
        [TargetRpc]
        void RpcSelfieLocalClose(NetworkConnectionToClient netConID, NetworkIdentity localNetID, int selfID)
        {
            localNetID.gameObject.GetComponent<ValueScript>().ResetValue();
            localNetID.gameObject.GetComponent<GroupSelfieGameObj>().buttonSelfieCanvas.gameObject.SetActive(true);

            //localNetID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[selfID].gameObject.SetActive(false);

            for (int i = 0; i <= localNetID.gameObject.GetComponent<ValueScript>().maxIndex; i++)
            {
                localNetID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[i].gameObject.SetActive(false);
            }

            localNetID.gameObject.GetComponent<GroupSelfieManager>().SavedPosition.Clear();
            localNetID.gameObject.GetComponent<GroupSelfieGameObj>().joinButtonCanvas.gameObject.SetActive(true);
            CmdClosePanelOther(localNetID, selfID);
        }

        // Call RPC function to closse Selfie Panel from Server
        [Command]
        void CmdClosePanelOther(NetworkIdentity localId, int selfID)
        {
            RpcPanelSelfieClose(localId, selfID);
        }

        // Close selfie canvas for local client
        [ClientRpc]
        void RpcPanelSelfieClose(NetworkIdentity localeID, int selfID)
        {
            localeID.gameObject.GetComponent<GroupSelfieGameObj>().SelfieCanvas.gameObject.SetActive(false);
            localeID.gameObject.GetComponent<GroupSelfieGameObj>().ExitButton.gameObject.SetActive(false);

            //localeID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[selfID].gameObject.SetActive(false);

            for (int i = 0; i <= localeID.gameObject.GetComponent<ValueScript>().maxIndex; i++)
            {
                localeID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[i].gameObject.SetActive(false);
            }

            localeID.gameObject.GetComponent<ValueScript>().countText.SetText(localeID.gameObject.GetComponent<GroupSelfieManager>().indexNum.ToString());
            localeID.gameObject.GetComponent<ValueScript>().currentText.SetText(localeID.gameObject.GetComponent<GroupSelfieManager>().indexNum.ToString());
            //localeID.gameObject.GetComponent<GameObjectScript>().unityGameObjects.Clear();
            localeID.gameObject.GetComponent<GroupSelfieManager>().SavedPosition.Clear();
        }
    }
}
