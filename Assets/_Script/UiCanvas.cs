using Mirror;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
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
            //testID = otherID;
            valueScript.testID = otherID;
            //testClientID = ClientID;
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
                Debug.Log("You max index is " + playerNetBehave.maxIndex);
                CmdSelfieLocalePanel(valueScript.localeSelfieID);
            }
        }

        // Command function to call RPC function for open the selfie froup canvas.
        [Command]
        void CmdSelfieLocalePanel(uint localNetID)
        {
            NetworkIdentity localeNetID = NetworkServer.spawned[localNetID];
            localeNetID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 0;
            RpcSelfieLocal(localeNetID.connectionToClient, localeNetID);
        }

        // Open selfie group panel in local player and disable client move
        [TargetRpc]
        void RpcSelfieLocal(NetworkConnectionToClient netConID, NetworkIdentity localNetID)
        {
            localNetID.gameObject.GetComponent<GameObjectScript>().SelfieCanvas.gameObject.SetActive(true);
            localNetID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 0;
            gameObjectScript.joinButtonCanvas.gameObject.SetActive(false);
            CmdSelfiePanelOther(localNetID);
        }

        // Call RPC function to display Selfie Panel from Server
        [Command]
        void CmdSelfiePanelOther(NetworkIdentity localId)
        {
            RpcPanelSelfie(localId);
        }

        // Display locale client selfie panel to all others client
        [ClientRpc]
        void RpcPanelSelfie(NetworkIdentity localeID)
        {
            localeID.gameObject.GetComponent<GameObjectScript>().SelfieCanvas.gameObject.SetActive(true);

            if (isLocalPlayer)
            {
                gameObjectScript.joinButtonCanvas.gameObject.SetActive(false);
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
            localeNetID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 2;
            RpcSelfieLocalClose(localeNetID.connectionToClient, localeNetID);
        }

        // Close selfie group and enable client move
        [TargetRpc]
        void RpcSelfieLocalClose(NetworkConnectionToClient netConID, NetworkIdentity localNetID)
        {
            localNetID.gameObject.GetComponent<GameObjectScript>().SelfieCanvas.gameObject.SetActive(false);
            localNetID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 2;
            gameObjectScript.joinButtonCanvas.gameObject.SetActive(true);
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
        }
    }
}
