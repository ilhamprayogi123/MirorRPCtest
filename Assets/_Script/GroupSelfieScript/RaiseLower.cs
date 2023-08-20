using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace StarterAssets
{
    public class RaiseLower : NetworkBehaviour
    {
        //public bool isRaise;

        [SerializeField]
        private ValueScript valueScript;
        [SerializeField]
        private GroupSelfieManager groupSelfieManager;
        [SerializeField]
        private PosRotScript posRotScript;

        public Vector3 originVector;
        public Vector3 newOriginVectopr;

        private void Start()
        {
            //isRaise = false;
        }

        private void Update()
        {
            if (gameObject.transform.position.y >= 0.3)
            {
                gameObject.transform.position = new Vector3(transform.position.x, 0.3f, transform.position.z);
            }

            if (gameObject.transform.position.y <= 0)
            {
                gameObject.transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
            }
        }

        // Function for Raise Stand Button
        public void RaiseButton()
        {
            RaiseFunction();
        }

        // Function for Lower Stand Button
        public void Lowerbutton()
        {
            LowerFunction();
        }

        // Function for Reset Stand Button
        public void BackHeight()
        {
            BackRaiseFunction();
        }

        // Function to call command function for using Raise Stand
        public void RaiseFunction()
        {
            valueScript.forHeightLocalID = this.gameObject.GetComponent<NetworkIdentity>().netId;

            CmdRaiseButton(valueScript.forHeightLocalID);
        }

        // Function to call command function for using Lower Stand
        public void LowerFunction()
        {
            valueScript.forHeightLocalID = this.gameObject.GetComponent<NetworkIdentity>().netId;

            CmdLowerButton(valueScript.forHeightLocalID);
        }

        // Function to call command function for using Reset Stand
        public void BackRaiseFunction()
        {
            valueScript.forHeightLocalID = this.gameObject.GetComponent<NetworkIdentity>().netId;

            CmdBackRaiseButton(valueScript.forHeightLocalID);
        }

        // Command function to call Rpc function for using Raise Stand
        [Command]
        void CmdRaiseButton(uint localeIDs)
        {
            NetworkIdentity targetID = NetworkServer.spawned[localeIDs];

            targetID.gameObject.GetComponent<RaiseLower>().RaisePosFunc();

            RpcRaise(targetID);
        }

        // Command function to call Rpc function for using Lower Stand
        [Command]
        void CmdLowerButton(uint localeIDs)
        {
            NetworkIdentity targetID = NetworkServer.spawned[localeIDs];

            targetID.gameObject.GetComponent<RaiseLower>().LowerPosFunc();

            RpcLower(targetID);
        }

        // Command function to call Rpc function for using Reset Stand Position 
        [Command]
        void CmdBackRaiseButton(uint localeIDs)
        {
            NetworkIdentity targetID = NetworkServer.spawned[localeIDs];

            targetID.gameObject.GetComponent<RaiseLower>().BackRaisePosFunc();

            RpcBackRaise(targetID);
        }

        // Rpc function to call main Raise Function at all others client side
        [ClientRpc]
        void RpcRaise(NetworkIdentity targetID)
        {
            targetID.gameObject.GetComponent<RaiseLower>().RaisePosFunc();
        }

        // Rpc function to call main Lower Function at all others client side
        [ClientRpc]
        void RpcLower(NetworkIdentity targetID)
        {
            targetID.gameObject.GetComponent<RaiseLower>().LowerPosFunc();
        }

        // Rpc function to call main Reset Function at all others client side
        [ClientRpc]
        void RpcBackRaise(NetworkIdentity targetID)
        {
            targetID.gameObject.GetComponent<RaiseLower>().BackRaisePosFunc();
        }

        // Function to raise height position 
        public void RaisePosFunc()
        {
            foreach (GameObject CenterObj in groupSelfieManager.CenterObject)
            {
                if (CenterObj.gameObject.GetComponent<GroupSelfieManager>().isCenterPos == true)
                {
                    CenterObj.gameObject.GetComponent<PosRotScript>().varHeight = CenterObj.gameObject.GetComponent<Transform>().position;

                    CenterObj.gameObject.GetComponent<PosRotScript>().newVarHeight = new Vector3(CenterObj.gameObject.GetComponent<PosRotScript>().varHeight.x, CenterObj.gameObject.GetComponent<PosRotScript>().varHeight.y + 0.3f, CenterObj.gameObject.GetComponent<PosRotScript>().varHeight.z);
                    CenterObj.gameObject.GetComponent<Transform>().gameObject.transform.position = CenterObj.gameObject.GetComponent<PosRotScript>().newVarHeight;
                    CenterObj.gameObject.GetComponent<GroupSelfieManager>().isRaising = true;

                    CenterObj.gameObject.GetComponent<ThirdPersonController>().enabled = false;
                }
            }
        }

        // Function to lower height position 
        public void LowerPosFunc()
        {
            foreach (GameObject CenterObj in groupSelfieManager.CenterObject)
            {
                if (CenterObj.gameObject.GetComponent<GroupSelfieManager>().isCenterPos == true)
                {
                    CenterObj.gameObject.GetComponent<PosRotScript>().varHeight = CenterObj.gameObject.GetComponent<Transform>().position;
                    CenterObj.gameObject.GetComponent<PosRotScript>().newVarHeight = new Vector3(CenterObj.gameObject.GetComponent<PosRotScript>().varHeight.x, CenterObj.gameObject.GetComponent<PosRotScript>().varHeight.y - 0.3f, CenterObj.gameObject.GetComponent<PosRotScript>().varHeight.z);
                    CenterObj.gameObject.GetComponent<Transform>().gameObject.transform.position = CenterObj.gameObject.GetComponent<PosRotScript>().newVarHeight;
                    CenterObj.gameObject.GetComponent<GroupSelfieManager>().isRaising = false;
                    CenterObj.gameObject.GetComponent<ThirdPersonController>().enabled = false;
                }
            }
        }

        // Function to reset position for the member of the selfie group.
        public void BackRaisePosFunc()
        {
            foreach (GameObject CenterObj in groupSelfieManager.CenterObject)
            {
                if (CenterObj.gameObject.GetComponent<GroupSelfieManager>().isCenterPos == true)
                {
                    if (CenterObj.gameObject.GetComponent<GroupSelfieManager>().isRaising == true)
                    {
                        CenterObj.gameObject.GetComponent<Transform>().gameObject.transform.position = CenterObj.gameObject.GetComponent<PosRotScript>().varHeight;
                        CenterObj.gameObject.GetComponent<GroupSelfieManager>().isCenterPos = false;
                        CenterObj.gameObject.GetComponent<GroupSelfieManager>().isRaising = false;
                        CenterObj.gameObject.GetComponent<ThirdPersonController>().enabled = true;
                        Debug.Log("TestDebug");
                    }
                    else if (CenterObj.gameObject.GetComponent<GroupSelfieManager>().isRaising == false)
                    {
                        CenterObj.gameObject.GetComponent<GroupSelfieManager>().isCenterPos = false;
                        CenterObj.gameObject.GetComponent<GroupSelfieManager>().isRaising = false;
                        CenterObj.gameObject.GetComponent<ThirdPersonController>().enabled = true;
                    }
                }
            }
        }
    }
}
