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

        public List<GameObject> SavedPosition = new List<GameObject>();

        public GameObject[] selfiePos;

        // Start is called before the first frame update
        void Start()
        {

        }

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
            }

            if (valueScript.changeIndex == true)
            {
                if (countNum >= maxIndex)
                {
                    valueScript.anySpace = false;
                    valueScript.isContinue = false;
                    valueScript.isMax = true;
                    //saveIndex();
                    Debug.Log("Is Full");

                    if (valueScript.isMax == true)
                    {
                        Debug.Log("Test Debug");
                        MaxIndex();
                        valueScript.changeIndex = false;
                    }
                }
                
                if (valueScript.anySpace == true && countNum == currentIndex)
                {
                    valueScript.isContinue = true;
                    valueScript.anySpace = true;
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

        // Cammand function to call Rpc for close the join button
        [Command(requiresAuthority = false)]
        void CmdJoinClose()
        {
            RpcJoinCLose();
        }

        // Close hoin button canvas for all client
        [ClientRpc]
        void RpcJoinCLose()
        {
            //gameObjectScript.joinButtonCanvas.gameObject.SetActive(false);
            groupSelfieObj.joinButtonCanvas.gameObject.SetActive(false);
        }
    }
}
