using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Cinemachine;
using TMPro;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine.Networking.Types;
using System;
using System.Xml.Linq;
using Unity.VisualScripting;

namespace StarterAssets
{
    // This script is used to set if conditions related to the use of the bool variable in the project, which is the main aspect of the join and exit process of the Group Selfie feature.
    public class GroupSelfieManager : NetworkBehaviour
    {
        [SerializeField]
        private PlayerNetworkBehaviour playerNet;
        [SerializeField]
        private ValueScript valueScript;
        [SerializeField]
        private UiCanvas uiCanvasObj;
        [SerializeField]
        private GameObjectScript gameObjectScript;
        [SerializeField]
        private GoupSelfieScript goupSelfie;
        [SerializeField]
        private GroupSelfieGameObj groupSelfieObj;

        [SyncVar(hook = nameof(RpcChangeMaxIndex))]
        public int maxIndex;
        [SyncVar]
        public int indexNum;
        [SyncVar(hook = nameof(RpcChangeMaxIndex))]
        public int currentIndex;
        [SyncVar(hook = nameof(RpcChangeMaxIndex))]
        public int loc;

        [SyncVar(hook = nameof(RpcChangeIndex))]
        public int selfiePosIndex;
        [SyncVar(hook = nameof(RpcChangeIndex))]
        public int countNum;
        [SyncVar(hook = nameof(RpcChangeIndex))]
        public int varIndexInt;

        public bool isCenterPos = false;
        public bool isCenterAvailable = false;
        public bool firstIn = true;
        public bool isRaising = false;

        public List<GameObject> SavedPosition = new List<GameObject>();
        public List<GameObject> CenterObject = new List<GameObject>();
        public List<GameObject> SecondCenterObject = new List<GameObject>();
        public List<GameObject> ThirdCenterObject = new List<GameObject>();

        public GameObject[] selfiePos;

        // Update is called once per frame
        void Update()
        {
            if (countNum > 1)
            {
                valueScript.readyChange = true;
            }

            if (!isServer)
            {
                CmdIndexUpdate();
            }
            
            if (countNum >= maxIndex)
            {
                valueScript.anySpace = false;
                valueScript.isContinue = false;
                //valueScript.isMax = true;
            }
            
            if (isLocalPlayer && valueScript.isMax == false && valueScript.isSelfieActive == true)
            {
                groupSelfieObj.joinButtonCanvas.gameObject.SetActive(false);
                //valueScript.changeIndex = false;
                //MinIndex();
            }

            if (!isLocalPlayer && valueScript.isMax == false && valueScript.isSelfieActive == true)
            {
                groupSelfieObj.joinButtonCanvas.gameObject.SetActive(true);
            } else if (!isLocalPlayer && valueScript.isMax == true && valueScript.isSelfieActive == true)
            {
                groupSelfieObj.joinButtonCanvas.gameObject.SetActive(false);
            }
            
            if (valueScript.changeIndex == true)
            {
                if (countNum >= maxIndex)
                {
                    valueScript.anySpace = false;
                    valueScript.isContinue = false;
                    valueScript.isMax = true;
                    //saveIndex();

                    if (valueScript.isMax == true)
                    {
                        Debug.Log("Test Debug");
                        //MaxIndex();
                        groupSelfieObj.joinButtonCanvas.gameObject.SetActive(false);
                        valueScript.changeIndex = false;
                    }
                }

                if (countNum < maxIndex)
                {
                    valueScript.isMax = false;
                    //Debug.Log("Is Minus");
                    //MinIndex();
                }

                if (valueScript.anySpace == true && countNum == currentIndex)
                {
                    valueScript.isContinue = true;
                    valueScript.anySpace = false;
                }

                if (countNum != currentIndex)
                {
                    valueScript.isContinue = false;
                    //valueScript.anySpace = true;
                }
            }
        }

        // Debug log this player Index
        void RpcChangeIndex(int oldValue, int newValue)
        {
            Debug.Log("Your new Index is : " + newValue);
        }

        // Debug log this player Max Index
        void RpcChangeMaxIndex(int oldValue, int newValue)
        {
            Debug.Log("Your Max Index is : " + newValue);
        }

        // Command function to update index in index panel
        [Command(requiresAuthority = false)]
        void CmdIndexUpdate()
        {
            groupSelfieObj.maxText.SetText(maxIndex.ToString());

            indexPanel();
        }

        // Update max text in index panel using client Rpc
        [ClientRpc]
        public void indexPanel()
        {
            //maxText.SetText(maxIndex.ToString());
            groupSelfieObj.maxText.SetText(maxIndex.ToString());
        }

        // Set isMax bool to false
        public void MaxIndex()
        {
            CmdJoinClose();
        }

        public void MinIndex()
        {
            CmdExitClose();
        }

        // Cammand function to call Rpc for close the join button
        [Command(requiresAuthority = false)]
        void CmdJoinClose()
        {
            valueScript.isMax = true;
            //RpcJoinCLose();
            RpcTargetClose(valueScript.isMax);
        }

        [TargetRpc]
        void RpcTargetClose(bool active)
        {
            valueScript.isMax = active;
            CmdTargetClose(active);
        }

        [Command]
        void CmdTargetClose(bool active)
        {
            RpcJoinCLose(active);
        }

        // Cammand function to call Rpc for close the join button
        [Command(requiresAuthority = false)]
        void CmdExitClose()
        {
            //valueScript.isMax = false;
            RpcTargetExitClose(valueScript.isMax);
        }

        [TargetRpc]
        void RpcTargetExitClose(bool active)
        {
            //valueScript.isMax = active;
            CmdTargetExitClose(active);
        }

        [Command]
        void CmdTargetExitClose(bool active)
        {
            RpcExitCLose(active);
        }

        // Close hoin button canvas for all client
        [ClientRpc]
        void RpcJoinCLose(bool active)
        {
            groupSelfieObj.joinButtonCanvas.gameObject.SetActive(active);
        }

        // Close hoin button canvas for all client
        [ClientRpc]
        void RpcExitCLose(bool active)
        {
            Debug.Log("Is not max");
            groupSelfieObj.joinButtonCanvas.gameObject.SetActive(false);
        }
    }
}
