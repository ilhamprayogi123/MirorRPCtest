using Mirror;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using UnityEngine.XR;

namespace StarterAssets
{
    public class UiCanvas : NetworkBehaviour
    {
        public PlayerNetworkBehaviour playerNetBehave;

        public Button inputButton;
        public Button ToggleButton;

        public GameObject ExitButton;

        [SerializeField]
        public InputHandler inputData;

        [SerializeField]
        public GameObject inputPrefab;

        [SerializeField]
        private GameObject requestPanel;

        [SyncVar]
        public uint localeSelfieID;
        [SyncVar]
        public uint testID;
        [SyncVar]
        public uint testClientID;

        //public Button assignButton;

        public GameObject RequestCanvas;

        public GameObject WaitCanvas;
        public GameObject AnimationCanvas;
        public GameObject NoAnimCanvas;

        public GameObject SelfieReqPanel;
        public GameObject ExitRequestPanel;
        public GameObject SelfieCanvas;
        public GameObject joinButtonCanvas;
        public GameObject buttonSelfieCanvas;
        public GameObject closeSelfieCanvas;

        [SerializeField]
        public string playName;

        public GameObject floatingInfo;

        public bool isWin = false;
        public bool isDefeat = false;
        public bool isButtonActive = false;

        public void Start()
        {
            ToggleButton = GameObject.FindGameObjectWithTag("ToggleButton").GetComponent<Button>();
            ToggleButton.onClick.AddListener(() => ButtonActivate());

            SelfieCanvas.gameObject.SetActive(false);
            ExitButton.gameObject.SetActive(false);

            inputPrefab = GameObject.FindGameObjectWithTag("Input");
            inputData = inputPrefab.transform.GetComponentInChildren<InputHandler>();

            playName = inputData.InputText;

            if (isLocalPlayer)
            {
                buttonSelfieCanvas.gameObject.SetActive(true);
                closeSelfieCanvas.gameObject.SetActive(false);

                RequestCanvas.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            floatingInfo.transform.LookAt(Camera.main.transform);
            SelfieCanvas.transform.LookAt(Camera.main.transform);

            if (isButtonActive == true && inputButton == null)
            {
                inputButton = GameObject.FindGameObjectWithTag("InputButton").GetComponent<Button>();
                inputButton.onClick.AddListener(() => playerNetBehave.InputName());
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
                buttonSelfieCanvas.gameObject.SetActive(false);
                closeSelfieCanvas.gameObject.SetActive(true);

                playerNetBehave.SelfieButtonFunc();
                //SelfieButtonFunc();
            }
        }

        // Open selfie canvas and close the close selfie canvas
        public void CloseSelfie()
        {
            if (isLocalPlayer)
            {
                buttonSelfieCanvas.gameObject.SetActive(true);
                closeSelfieCanvas.gameObject.SetActive(false);

                playerNetBehave.CloseSelfieGroup();
                //CloseSelfieGroup();
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
            testClientID = thisObj.netId;
            testID = localPlayer.gameObject.GetComponent<NetworkIdentity>().netId;
            //Debug.Log(testID);
            CmdSelfieReqPanel(testID, testClientID);
        }

        // Call RPC function to open join group request panel for the clicnt who clicked to button from server
        [Command(requiresAuthority = false)]
        void CmdSelfieReqPanel(uint otherID, uint ClientID)
        {
            NetworkIdentity otherClientID = NetworkServer.spawned[otherID];
            testID = otherID;
            testClientID = ClientID;
            RpcSelfieReqPanel(otherClientID.connectionToClient, otherClientID, ClientID);
        }

        // Open Join Group Selfie panel in client who clicked the join button 
        [TargetRpc]
        void RpcSelfieReqPanel(NetworkConnectionToClient netConID, NetworkIdentity netOtherID, uint clientID)
        {
            netOtherID.gameObject.GetComponent<UiCanvas>().SelfieReqPanel.gameObject.SetActive(true);
            netOtherID.gameObject.GetComponent<PlayerNetworkBehaviour>().testClientID = clientID;
            testID = netOtherID.gameObject.GetComponent<NetworkIdentity>().netId;
        }

        // Call Command Function to exit group and open exit group request panel
        public void ExitGroupButton()
        {
            //var other = FindObjectOfType<ThirdPersonController>().GetComponent<NetworkIdentity>();
            var thisObj = GetComponent<NetworkIdentity>();

            GameObject localPlayer = NetworkClient.localPlayer.gameObject;

            testClientID = thisObj.netId;
            testID = localPlayer.gameObject.GetComponent<NetworkIdentity>().netId;

            CmdExitReqPanel(testID, testClientID);
        }

        // Call RPC function to open exit group request panel in client who clicked the exit button from server
        [Command(requiresAuthority = false)]
        void CmdExitReqPanel(uint otherID, uint ClientID)
        {
            NetworkIdentity otherClientID = NetworkServer.spawned[otherID];
            testID = otherID;
            testClientID = ClientID;
            RpcExitReqPanel(otherClientID.connectionToClient, otherClientID, ClientID);
        }

        // Open exit group request panel in client who clicked the exit button
        [TargetRpc]
        void RpcExitReqPanel(NetworkConnectionToClient netConID, NetworkIdentity netOtherID, uint clientID)
        {
            netOtherID.gameObject.GetComponent<UiCanvas>().ExitRequestPanel.gameObject.SetActive(true);
            netOtherID.gameObject.GetComponent<PlayerNetworkBehaviour>().testClientID = clientID;
            testID = netOtherID.gameObject.GetComponent<NetworkIdentity>().netId;
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
            CmdLocaleAnimPanel(playerNetBehave.localID);
            playerNetBehave.varIndex = index;
            CmdONpanel(playerNetBehave.idNet, playerNetBehave.varIndex);
        }

        // Command function to call RPC function for open the animation request panel in other client
        [Command]
        void CmdONpanel(uint idNet, int varIndexAnim)
        {
            NetworkIdentity otherClientID = NetworkServer.spawned[idNet];
            playerNetBehave.varIndex = varIndexAnim;
            RpcONpanel(otherClientID.connectionToClient, otherClientID, playerNetBehave.varIndex);
        }

        // Open animation request panel and instantiate decision button (Yes / No)
        [TargetRpc]
        void RpcONpanel(NetworkConnectionToClient otherID, NetworkIdentity otherClientID, int varIndexAnim)
        {
            WaitCanvas.gameObject.SetActive(false);
            RequestCanvas.gameObject.SetActive(true);
            playerNetBehave.varIndex = varIndexAnim;
            playerNetBehave.animRequest.text = "Do you want to " + playerNetBehave.animDatas[playerNetBehave.varIndex].AnimState + " ?";
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
            netID.gameObject.GetComponent<UiCanvas>().AnimationCanvas.gameObject.SetActive(false);
            netID.gameObject.GetComponent<UiCanvas>().WaitCanvas.gameObject.SetActive(true);
        }
    }
}


