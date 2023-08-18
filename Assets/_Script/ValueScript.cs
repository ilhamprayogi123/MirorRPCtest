using Mirror;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace StarterAssets
{
    // This script is used to store the main variables or values that will be used in the project.
    public class ValueScript : NetworkBehaviour
    {
        [SerializeField]
        private PlayerNetworkBehaviour playerNet;

        public List<int> numIndex = new List<int>();

        [SyncVar]
        public int animID;
        [SyncVar]
        public int intIndexAnim;
        [SyncVar]
        public int varIndex;

        [SyncVar]
        public int maxIndex;
        [SyncVar]
        public int indexNum;
        [SyncVar]
        public int limitIndex;
        [SyncVar]
        public int limit;

        [SyncVar]
        public int indexSaved;
        [SyncVar]
        public int indexContinue;
        [SyncVar]
        public int currentIndex;

        [SyncVar]
        public int varIndexInt = 0;

        [SyncVar]
        public int GroupID;

        public TMP_Text countText;
        public TMP_Text currentText;
        public TMP_Text maxText;

        [SyncVar]
        public uint localID;
        [SyncVar]
        public uint locID;
        [SyncVar]
        public uint localeSelfieID;
        [SyncVar]
        public uint otherClientIDs;
        [SyncVar]
        public uint testClientID;
        [SyncVar]
        public uint idNet;
        [SyncVar]
        public uint objId;
        [SyncVar]
        public uint testID;
        [SyncVar]
        public uint forHeightLocalID;

        [SyncVar]
        public int localNets;
        [SyncVar]
        public int locNets;

        public bool isMax = false;
        public bool changeIndex = false;
        public bool readyChange = false;
        public bool isMin = false;
        public bool isFull = false;
        public bool anySpace = false;
        public bool isContinue = false;
        public bool empty = false;
        public bool isNext;

        // Increase count in count client panel
        public void IncreaseCount()
        {
            this.gameObject.GetComponent<GroupSelfieManager>().indexNum++;
            //countText.SetText(playerNet.indexNum.ToString());
            this.gameObject.GetComponent<ValueScript>().countText.SetText(this.gameObject.GetComponent<GroupSelfieManager>().indexNum.ToString());
            //playerNet.maxIndex = playerNet.indexNum;
            this.gameObject.GetComponent<GroupSelfieManager>().maxIndex = this.gameObject.GetComponent<GroupSelfieManager>().indexNum;

            GameObject thisObject = NetworkClient.localPlayer.gameObject;
            limit = thisObject.gameObject.GetComponent<GroupSelfieManager>().maxIndex;
        }

        // Decrease count in count client panel
        public void DecreaseCount()
        {
            this.gameObject.GetComponent<GroupSelfieManager>().indexNum--;

            if (this.gameObject.GetComponent<GroupSelfieManager>().indexNum <= 0)
            {
                this.gameObject.GetComponent<GroupSelfieManager>().indexNum = 0;
            }

            this.gameObject.GetComponent<ValueScript>().countText.SetText(this.gameObject.GetComponent<GroupSelfieManager>().indexNum.ToString());
            this.gameObject.GetComponent<GroupSelfieManager>().maxIndex = this.gameObject.GetComponent<GroupSelfieManager>().indexNum;

            GameObject thisObject = NetworkClient.localPlayer.gameObject;
            limit = thisObject.gameObject.GetComponent<GroupSelfieManager>().maxIndex;
        }

        // Reset Value
        public void ResetValue()
        {
            Debug.Log("Reset Func");

            gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 2;
            gameObject.GetComponent<ThirdPersonController>().SprintSpeed = 5.335f;
            gameObject.GetComponent<ThirdPersonController>().enabled = true;
            gameObject.GetComponent<GroupSelfieManager>().indexNum = 0;
            gameObject.GetComponent<GroupSelfieManager>().countNum = 0;
            gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex = 0;
            gameObject.GetComponent<GroupSelfieManager>().loc = 0;

            ResetBool();
        }

        // Reset bool
        public void ResetBool()
        {
            Debug.Log("Reset bool");

            gameObject.GetComponent<ValueScript>().isContinue = false;
            gameObject.GetComponent<ValueScript>().anySpace = false;
            gameObject.GetComponent<ValueScript>().isNext = false;
            gameObject.GetComponent<ValueScript>().isMax = false;
        }
    }
}
