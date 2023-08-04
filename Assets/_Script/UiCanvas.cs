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
    public class UiCanvas : NetworkBehaviour
    {
        public PlayerNetworkBehaviour playerNetBehave;
        [SerializeField]
        private ValueScript valueScript;
        [SerializeField]
        private GameObjectScript gameObjectScript;

        [SerializeField]
        public string playName;

        public bool isButtonActive = false;

        public void Start()
        {
            gameObjectScript.ToggleButton = GameObject.FindGameObjectWithTag("ToggleButton").GetComponent<Button>();
            gameObjectScript.ToggleButton.onClick.AddListener(() => ButtonActivate());

            gameObjectScript.SelfieCanvas.gameObject.SetActive(false);
            gameObjectScript.ExitButton.gameObject.SetActive(false);

            gameObjectScript.inputPrefab = GameObject.FindGameObjectWithTag("Input");
            gameObjectScript.inputData = gameObjectScript.inputPrefab.transform.GetComponentInChildren<InputHandler>();

            playName = gameObjectScript.inputData.InputText;

            if (isLocalPlayer)
            {
                gameObjectScript.buttonSelfieCanvas.gameObject.SetActive(true);
                gameObjectScript.closeSelfieCanvas.gameObject.SetActive(false);
                gameObjectScript.RequestCanvas.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            gameObjectScript.floatingInfo.transform.LookAt(Camera.main.transform);
            gameObjectScript.SelfieCanvas.transform.LookAt(Camera.main.transform);

            if (!isLocalPlayer)
            {
                gameObjectScript.buttonSelfieCanvas.SetActive(false);
            }
            
            if (isButtonActive == true && gameObjectScript.inputButton == null)
            {
                gameObjectScript.inputButton = GameObject.FindGameObjectWithTag("InputButton").GetComponent<Button>();
                gameObjectScript.inputButton.onClick.AddListener(() => playerNetBehave.InputName());
                isButtonActive = false;
            }
        }

        // Call true foe button active boolean to activated the input button
        public void ButtonActivate()
        {
            isButtonActive = true;
        }

        // Open close selfie canvas and close selfie canvas
        public void SelfieButton()
        {
            if (isLocalPlayer)
            {
                gameObjectScript.buttonSelfieCanvas.gameObject.SetActive(false);
                gameObjectScript.closeSelfieCanvas.gameObject.SetActive(true);

                Debug.Log("Max Player is " + playerNetBehave.maxIndex);
                SelfieButtonFunc();
            }
        }

        // Reset loc value
        public void ResetLoc(int index)
        {
            GameObject[] playerTarget = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject playerNum in playerTarget)
            {
                playerNum.gameObject.GetComponent<PlayerNetworkBehaviour>().loc = index;
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

        // Reset value for client in the group
        public void GetAllGroupID(float speed, int num, float sprintSpeed)
        {
            GameObject thisObj = this.gameObject;

            foreach (GameObject playerNumObj in gameObjectScript.unityGameObjects)
            {
                //playerNumObj.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 2;
                playerNumObj.gameObject.GetComponent<GameObjectScript>().buttonSelfieCanvas.gameObject.SetActive(true);
                playerNumObj.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = speed;
                playerNumObj.gameObject.GetComponent<ThirdPersonController>().SprintSpeed = sprintSpeed;
                playerNumObj.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = num;

                playerNumObj.gameObject.GetComponent<UiCanvas>().CloseSelfieBool();
                playerNumObj.gameObject.GetComponent<UiCanvas>().CmdReset(num);
            }
        }

        // Command function to reset selfie pos index
        [Command(requiresAuthority = false)]
        void CmdReset(int num)
        {
            gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = num;

            RpcReset(num);
        }

        // Reset selfie pos index
        [ClientRpc]
        void RpcReset(int num)
        {
            gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = num;
        }

        // Open selfie canvas and close the close selfie canvas
        public void CloseSelfie()
        {
            if (isLocalPlayer)
            {
                gameObjectScript.buttonSelfieCanvas.gameObject.SetActive(true);
                gameObjectScript.closeSelfieCanvas.gameObject.SetActive(false);
                CloseSelfieGroup();
            }
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
            netOtherID.gameObject.GetComponent<GameObjectScript>().SelfieReqPanel.gameObject.SetActive(true);
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
            netOtherID.gameObject.GetComponent<GameObjectScript>().ExitRequestPanel.gameObject.SetActive(true);
            netOtherID.gameObject.GetComponent<ValueScript>().testClientID = clientID;
            valueScript.testID = netOtherID.gameObject.GetComponent<NetworkIdentity>().netId;
        }

        // Deactivate all animation button
        void ButtonDeactivated()
        {
            GameObject[] btnTarget = GameObject.FindGameObjectsWithTag("ButtonInst");
            foreach (GameObject btnDestroyed in btnTarget)
                GameObject.Destroy(btnDestroyed);
        }

        // Function to call animation request panel and deactivated the animation button
        public void AnimationButton(int index)
        {
            ButtonDeactivated();
            CmdLocaleAnimPanel(valueScript.localID);
            valueScript.varIndex = index;
            CmdONpanel(valueScript.idNet, valueScript.varIndex);
        }

        // Command function to call RPC function for open the animation request panel in other client
        [Command]
        void CmdONpanel(uint idNet, int varIndexAnim)
        {
            NetworkIdentity otherClientID = NetworkServer.spawned[idNet];
            valueScript.varIndex = varIndexAnim;
            RpcONpanel(otherClientID.connectionToClient, otherClientID, valueScript.varIndex);
        }

        // Open animation request panel and instantiate decision button (Yes / No)
        [TargetRpc]
        void RpcONpanel(NetworkConnectionToClient otherID, NetworkIdentity otherClientID, int varIndexAnim)
        {
            gameObjectScript.WaitCanvas.gameObject.SetActive(false);
            gameObjectScript.RequestCanvas.gameObject.SetActive(true);
            valueScript.varIndex = varIndexAnim;
            playerNetBehave.animRequest.text = "Do you want to " + playerNetBehave.animDatas[valueScript.varIndex].AnimState + " ?";
            for (int i = 0; i < playerNetBehave.ReqButton.Count; i++)
            {
                Instantiate(playerNetBehave.ReqButton[i].gameObject);
            }
        }

        // Command function to sall RPC function for close chose animation panel and open wait panel
        [Command]
        void CmdLocaleAnimPanel(uint localeID)
        {
            NetworkIdentity localePlayer = NetworkServer.spawned[localeID];
            RpcLocaleAnimPanel(localePlayer.connectionToClient, localePlayer);
        }

        // Close animation panel amd open Wait panel in local client
        [TargetRpc]
        void RpcLocaleAnimPanel(NetworkConnectionToClient netCon, NetworkIdentity netID)
        {
            netID.gameObject.GetComponent<GameObjectScript>().AnimationCanvas.gameObject.SetActive(false);
            netID.gameObject.GetComponent<GameObjectScript>().WaitCanvas.gameObject.SetActive(true);
        }

        // Call Command Function to open group selfie canvas
        public void SelfieButtonFunc()
        {
            if (isLocalPlayer)
            {
                valueScript.localeSelfieID = this.gameObject.GetComponent<NetworkIdentity>().netId;
                valueScript.GroupID = Convert.ToInt32(valueScript.localeSelfieID);
                gameObjectScript.joinButtonCanvas.gameObject.SetActive(false);
                gameObjectScript.ExitButton.gameObject.SetActive(false);
                Debug.Log("You max index is " + playerNetBehave.maxIndex);
                CmdSelfieLocalePanel(valueScript.localeSelfieID, valueScript.GroupID, playerNetBehave.maxIndex);
            }
        }

        // Command function to call RPC function for open the selfie froup canvas.
        [Command]
        void CmdSelfieLocalePanel(uint localNetID, int groupID, int maxIndex)
        {
            NetworkIdentity localeNetID = NetworkServer.spawned[localNetID];
            localeNetID.gameObject.GetComponent<ValueScript>().GroupID = groupID;
            localeNetID.gameObject.GetComponent<PlayerNetworkBehaviour>().maxIndex = maxIndex;
            localeNetID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 0;
            localeNetID.gameObject.GetComponent<ThirdPersonController>().SprintSpeed = 0;
            //GetMaxIndex();
            RpcSelfieLocal(localeNetID.connectionToClient, localeNetID, groupID, maxIndex);
        }

        // Open selfie group panel in local player and disable client move
        [TargetRpc]
        void RpcSelfieLocal(NetworkConnectionToClient netConID, NetworkIdentity localNetID, int groupID, int maxIndex)
        {
            localNetID.gameObject.GetComponent<GameObjectScript>().SelfieCanvas.gameObject.SetActive(true);
            localNetID.gameObject.GetComponent<ValueScript>().GroupID = groupID;
            localNetID.gameObject.GetComponent<PlayerNetworkBehaviour>().maxIndex = maxIndex;
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
            localeID.gameObject.GetComponent<GameObjectScript>().SelfieCanvas.gameObject.SetActive(true);
            localeID.gameObject.GetComponent<PlayerNetworkBehaviour>().maxIndex = maxIndex;
            
            if (!isLocalPlayer)
            {
                gameObjectScript.joinButtonCanvas.gameObject.SetActive(true);
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

            localeNetID.gameObject.GetComponent<ValueScript>().ResetValue();
            localeNetID.gameObject.GetComponent<PlayerNetworkBehaviour>().loc = reset;
            localeNetID.gameObject.GetComponent<GameObjectScript>().unityGameObjects.Clear();

            GetAllGroupID(localeNetID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed, localeNetID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex, localeNetID.gameObject.GetComponent<ThirdPersonController>().SprintSpeed);
            //ResetLoc(reset);
            RpcCLose(localeNetID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed, localeNetID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex, localeNetID.gameObject.GetComponent<ThirdPersonController>().SprintSpeed);
            RpcSelfieLocalClose(localeNetID.connectionToClient, localeNetID);
        }

        // Call Reset value function for all client
        [ClientRpc]
        void RpcCLose(float speed, int num, float sprintSpeed)
        {
            GetAllGroupID(speed, num, sprintSpeed);
        }

        // Close selfie group and enable client move
        [TargetRpc]
        void RpcSelfieLocalClose(NetworkConnectionToClient netConID, NetworkIdentity localNetID)
        {
            localNetID.gameObject.GetComponent<ValueScript>().ResetValue();
            localNetID.gameObject.GetComponent<GameObjectScript>().buttonSelfieCanvas.gameObject.SetActive(true);
            
            localNetID.gameObject.GetComponent<GameObjectScript>().unityGameObjects.Clear();
            localNetID.gameObject.GetComponent<GameObjectScript>().joinButtonCanvas.gameObject.SetActive(true);
            CmdClosePanelOther(localNetID);
        }

        // Call RPC function to closse Selfie Panel from Server
        [Command]
        void CmdClosePanelOther(NetworkIdentity localId)
        {
            RpcPanelSelfieClose(localId);
        }

        // Close selfie canvas for local client
        [ClientRpc]
        void RpcPanelSelfieClose(NetworkIdentity localeID)
        {
            localeID.gameObject.GetComponent<GameObjectScript>().SelfieCanvas.gameObject.SetActive(false);
            localeID.gameObject.GetComponent<GameObjectScript>().ExitButton.gameObject.SetActive(false);
            localeID.gameObject.GetComponent<ValueScript>().countText.SetText(localeID.gameObject.GetComponent<PlayerNetworkBehaviour>().indexNum.ToString());
            localeID.gameObject.GetComponent<ValueScript>().currentText.SetText(localeID.gameObject.GetComponent<PlayerNetworkBehaviour>().indexNum.ToString());
            localeID.gameObject.GetComponent<GameObjectScript>().unityGameObjects.Clear();
        }
    }
}
