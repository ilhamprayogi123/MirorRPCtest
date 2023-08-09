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
    // This script is used to set the UI Canvas for the couple animation feature
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

            gameObjectScript.inputPrefab = GameObject.FindGameObjectWithTag("Input");
            gameObjectScript.inputData = gameObjectScript.inputPrefab.transform.GetComponentInChildren<InputHandler>();

            playName = gameObjectScript.inputData.InputText;

            if (isLocalPlayer)
            {
                gameObjectScript.RequestCanvas.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            gameObjectScript.floatingInfo.transform.LookAt(Camera.main.transform);
            
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
        
        // Reset loc value
        public void ResetLoc(int index)
        {
            GameObject[] playerTarget = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject playerNum in playerTarget)
            {
                playerNum.gameObject.GetComponent<GroupSelfieManager>().loc = index;
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
    }
}
