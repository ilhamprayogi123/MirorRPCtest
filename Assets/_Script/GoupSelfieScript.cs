using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace StarterAssets
{
    // This script is used to manage features related to Group Selfies, such as join, exit, and close groups.
    public class GoupSelfieScript : NetworkBehaviour
    {
        public bool isRaise;

        [SerializeField]
        private PlayerNetworkBehaviour playerNet;
        [SerializeField]
        private ValueScript valueScript;
        [SerializeField]
        private UiCanvas uiCanvasObj;
        [SerializeField]
        private GameObjectScript gameObjectScript;
        [SerializeField]
        private GroupSelfieGameObj groupSelfieObj;
        [SerializeField]
        private GroupSelfieManager groupSelfieManager;
        [SerializeField]
        private PosRotScript posRotScript;

        public Vector3 originVector;
        public Vector3 newOriginVectopr;
        //public bool firstIn;

        Rigidbody rigid;

        private void Start()
        {
            isRaise = false;
            //firstIn = true;
            rigid = this.gameObject.GetComponent<Collider>().attachedRigidbody;
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

            if (isRaise == true)
            {
                this.gameObject.GetComponent<Transform>().gameObject.transform.position = posRotScript.newVarHeight;
            }
        }

        // Call function after client agree to join Group Selfie
        public void YesJoin()
        {
            Debug.Log(groupSelfieManager.loc);
            groupSelfieObj.SelfieReqPanel.gameObject.SetActive(false);
            valueScript.otherClientIDs = NetworkClient.localPlayer.gameObject.GetComponent<NetworkIdentity>().netId;
            GameObject thisObject = NetworkClient.localPlayer.gameObject;
            CmdYesjoin(valueScript.otherClientIDs, valueScript.testClientID, groupSelfieManager.loc);
        }

        // Command Function to change position and rotation in Server Side
        [Command]
        void CmdYesjoin(uint corePosID, uint centerPosID, int loc)
        {
            NetworkIdentity localID = NetworkServer.spawned[corePosID];
            NetworkIdentity posID = NetworkServer.spawned[centerPosID];

            int intLoc = posID.gameObject.GetComponent<GroupSelfieManager>().loc;

            if (posID.gameObject.GetComponent<ValueScript>().anySpace == true)
            {
                posID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex = posID.gameObject.GetComponent<ValueScript>().numIndex[intLoc];
                valueScript.localNets = posID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;

                if (posID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[valueScript.localNets].GetComponent<TestBool>().isCenter == true)
                {
                    Debug.Log("Test Center");
                    localID.gameObject.GetComponent<GroupSelfieManager>().isCenterPos = true;
                    posID.gameObject.GetComponent<GroupSelfieManager>().isCenterAvailable = true;
                    posID.gameObject.GetComponent<GroupSelfieManager>().CenterObject.Add(localID.gameObject);
                }

                localID.gameObject.GetComponent<Transform>().position = posID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[valueScript.localNets].transform.position;
                
                localID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex = valueScript.localNets;
                
            }
            else if (posID.gameObject.GetComponent<ValueScript>().isContinue == true && posID.gameObject.GetComponent<ValueScript>().anySpace == true)
            {
                posID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex = posID.gameObject.GetComponent<ValueScript>().indexContinue;

                if (posID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[groupSelfieManager.selfiePosIndex].GetComponent<TestBool>().isCenter == true)
                {
                    Debug.Log("Test Center");
                    localID.gameObject.GetComponent<GroupSelfieManager>().isCenterPos = true;
                    posID.gameObject.GetComponent<GroupSelfieManager>().isCenterAvailable = true;
                    posID.gameObject.GetComponent<GroupSelfieManager>().CenterObject.Add(localID.gameObject);
                }

                localID.gameObject.GetComponent<Transform>().position = posID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[groupSelfieManager.selfiePosIndex].transform.position;
            }
            else
            {
                localID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex = posID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;

                if (posID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[groupSelfieManager.selfiePosIndex].GetComponent<TestBool>().isCenter == true)
                {
                    Debug.Log("Test Center");
                    localID.gameObject.GetComponent<GroupSelfieManager>().isCenterPos = true;
                    posID.gameObject.GetComponent<GroupSelfieManager>().isCenterAvailable = true;
                    posID.gameObject.GetComponent<GroupSelfieManager>().CenterObject.Add(localID.gameObject);
                }

                localID.gameObject.GetComponent<Transform>().position = posID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[groupSelfieManager.selfiePosIndex].transform.position;
            }

            localID.gameObject.GetComponent<Transform>().rotation = posID.gameObject.GetComponent<Transform>().rotation;

            localID.gameObject.GetComponent<ValueScript>().GroupID = posID.gameObject.GetComponent<ValueScript>().GroupID;
            //posID.gameObject.GetComponent<GameObjectScript>().unityGameObjects.Add(localID.gameObject);
            posID.gameObject.GetComponent<GroupSelfieManager>().SavedPosition.Add(localID.gameObject);

            localID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 0;
            localID.gameObject.GetComponent<ThirdPersonController>().SprintSpeed = 0;
            localID.gameObject.GetComponent<ThirdPersonController>().enabled = false;
            
            RpcYesJoin(localID.connectionToClient, localID, posID, intLoc);
        }

        // RPC Function to change / transform position and rotation in Client Side.
        [TargetRpc]
        void RpcYesJoin(NetworkConnectionToClient netConID, NetworkIdentity netID, NetworkIdentity localPosID, int intNet)
        {
            groupSelfieObj.buttonSelfieCanvas.gameObject.SetActive(false);

            if (localPosID.gameObject.GetComponent<ValueScript>().anySpace == true)
            {
                netID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex = localPosID.gameObject.GetComponent<ValueScript>().indexSaved;
                localPosID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex = localPosID.gameObject.GetComponent<ValueScript>().numIndex[intNet];
                valueScript.localNets = localPosID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;

                if (localPosID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[valueScript.localNets].GetComponent<TestBool>().isCenter == true)
                {
                    Debug.Log("Test Center");
                    netID.gameObject.GetComponent<GroupSelfieManager>().isCenterPos = true;
                    localPosID.gameObject.GetComponent<GroupSelfieManager>().isCenterAvailable = true;
                    localPosID.gameObject.GetComponent<GroupSelfieManager>().CenterObject.Add(netID.gameObject);
                    CmdGetCenterObjData(localPosID, netID);
                }

                netID.gameObject.GetComponent<Transform>().position = localPosID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[valueScript.localNets].transform.position;
                //netID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = valueScript.localNets;
                netID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex = valueScript.localNets;
                CmdIndexArc(localPosID, intNet);
            }
            else if (localPosID.gameObject.GetComponent<ValueScript>().isContinue == true && localPosID.gameObject.GetComponent<ValueScript>().anySpace == true)
            {
                //netID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = localPosID.gameObject.GetComponent<ValueScript>().indexContinue;
                netID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex = localPosID.gameObject.GetComponent<ValueScript>().indexContinue;
                //localPosID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex = localPosID.gameObject.GetComponent<ValueScript>().indexContinue;
                localPosID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex = localPosID.gameObject.GetComponent<ValueScript>().indexContinue;

                if (localPosID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[groupSelfieManager.selfiePosIndex].GetComponent<TestBool>().isCenter == true)
                {
                    Debug.Log("Test Center");
                    netID.gameObject.GetComponent<GroupSelfieManager>().isCenterPos = true;
                    localPosID.gameObject.GetComponent<GroupSelfieManager>().isCenterAvailable = true;
                    localPosID.gameObject.GetComponent<GroupSelfieManager>().CenterObject.Add(netID.gameObject);
                    CmdGetCenterObjData(localPosID, netID);
                }

                netID.gameObject.GetComponent<Transform>().position = localPosID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[groupSelfieManager.selfiePosIndex].transform.position;
                localPosID.gameObject.GetComponent<ValueScript>().isContinue = false;
            }
            else
            {
                netID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex = localPosID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;

                if (localPosID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[groupSelfieManager.selfiePosIndex].GetComponent<TestBool>().isCenter == true)
                {
                    Debug.Log("Test Center");
                    netID.gameObject.GetComponent<GroupSelfieManager>().isCenterPos = true;
                    localPosID.gameObject.GetComponent<GroupSelfieManager>().isCenterAvailable = true;
                    localPosID.gameObject.GetComponent<GroupSelfieManager>().CenterObject.Add(netID.gameObject);
                    CmdGetCenterObjData(localPosID, netID);
                }

                netID.gameObject.GetComponent<Transform>().position = localPosID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[groupSelfieManager.selfiePosIndex].transform.position;
                //localPosID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[groupSelfieManager.selfiePosIndex].gameObject.SetActive(false);
            }

            netID.gameObject.GetComponent<Transform>().rotation = localPosID.gameObject.GetComponent<Transform>().rotation;

            if (isLocalPlayer)
            {
                localPosID.gameObject.GetComponent<GroupSelfieGameObj>().joinButtonCanvas.gameObject.SetActive(false);
                localPosID.gameObject.GetComponent<GroupSelfieGameObj>().ExitButton.gameObject.SetActive(true);
            }

            netID.gameObject.GetComponent<ValueScript>().GroupID = localPosID.gameObject.GetComponent<ValueScript>().GroupID;
            //localPosID.gameObject.GetComponent<GameObjectScript>().unityGameObjects.Add(netID.gameObject);
            localPosID.gameObject.GetComponent<GroupSelfieManager>().SavedPosition.Add(netID.gameObject);
            netID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 0;
            netID.gameObject.GetComponent<ThirdPersonController>().SprintSpeed = 0;
            netID.gameObject.GetComponent<ThirdPersonController>().enabled = false;
            CmdChangeIndex(netID, localPosID);
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

                    Debug.Log("TestKas");

                    CenterObj.gameObject.GetComponent<ThirdPersonController>().enabled = false;
                    //CenterObj.gameObject.GetComponent<Rigidbody>().isKinematic = true;
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

        // Command function to call rpc function for add the desired client object to special list.
        [Command(requiresAuthority = false)]
        void CmdGetCenterObjData(NetworkIdentity targetID, NetworkIdentity addedID)
        {
            //targetID.gameObject.GetComponent<GroupSelfieManager>().CenterObject.Add(addedID.gameObject);
            RpcGetCenterobjData(targetID, addedID);
        }

        // Rpc function to add the desired client to list in all others client side.
        [ClientRpc]
        void RpcGetCenterobjData(NetworkIdentity targetID, NetworkIdentity addedID)
        {
            targetID.gameObject.GetComponent<GroupSelfieManager>().isCenterAvailable = true;
            targetID.gameObject.GetComponent<GroupSelfieManager>().CenterObject.Add(addedID.gameObject);
        }

        // Increase loc for client who provide group selfie
        [Command(requiresAuthority = false)]
        void CmdIndexArc(NetworkIdentity netID, int index)
        {
            Debug.Log("Testing : " + index);
            index++;
            netID.gameObject.GetComponent<GroupSelfieManager>().loc = index;
            RpcIndexArc(netID, index);
        }

        // Change loc value for client who provide group selfie in other client side
        [ClientRpc]
        void RpcIndexArc(NetworkIdentity netID, int index)
        {
            Debug.Log(index);
            netID.gameObject.GetComponent<GroupSelfieManager>().loc = index;
            //CmdIntIndex(netID, index);
        }

        // Command Function to call function to change Index for Group Selfie and add gameobject to list
        [Command]
        void CmdChangeIndex(NetworkIdentity netID, NetworkIdentity localID)
        {
            int currentInt = localID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;
            //localID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[currentInt].gameObject.SetActive(false);

            localID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex++;
            localID.gameObject.GetComponent<GroupSelfieManager>().countNum++;
            int selfID = localID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;

            RpcTargetChangeIndex(localID.connectionToClient, localID, currentInt, selfID);
            RpcGroup(localID.connectionToClient, netID, localID);
        }

        // Change index bool in local client
        [TargetRpc]
        void RpcTargetChangeIndex(NetworkConnectionToClient netCon, NetworkIdentity netID, int currentInt, int selfID)
        {
            //netID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[currentInt].gameObject.SetActive(false);
            netID.gameObject.GetComponent<ValueScript>().changeIndex = true;
            CmdIndexTex(netID, currentInt, selfID);
        }

        // Rpc Function to add gameobject to list for group selfie
        [TargetRpc]
        void RpcGroup(NetworkConnectionToClient netCon, NetworkIdentity localID, NetworkIdentity netID)
        {
            netID.gameObject.GetComponent<GroupSelfieManager>().SavedPosition.Add(localID.gameObject);
        }

        // Call Rpc function to change count text
        [Command(requiresAuthority = false)]
        void CmdIndexTex(NetworkIdentity netID, int currentInt, int selfID)
        {
            //netID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[groupSelfieManager.selfiePosIndex].gameObject.SetActive(true);
            RpcTextIndex(netID, currentInt, selfID);
        }

        // Change count text for other client
        [ClientRpc]
        void RpcTextIndex(NetworkIdentity netID, int currentInt, int selfID)
        {
            //CmdCloseAllCircle(netID, selfID);
            netID.gameObject.GetComponent<ValueScript>().changeIndex = true;
            //netID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[selfID].gameObject.SetActive(true);
            netID.gameObject.GetComponent<GroupSelfieGameObj>().currentText.SetText(netID.gameObject.GetComponent<GroupSelfieManager>().countNum.ToString());
            //CmdChangeCircle(netID);

        }

        // Command function to Close circle indicator after client transform to this circle indicator position 
        [Command(requiresAuthority = false)]
        void CmdCloseAllCircle(NetworkIdentity netID, int selfID)
        {
            netID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[groupSelfieManager.selfiePosIndex].gameObject.SetActive(false);
            RpcAllCloseCircle(netID, selfID);
        }

        // Rpc Function to close the Circle Indicator that already accupied in other client view
        [ClientRpc]
        void RpcAllCloseCircle(NetworkIdentity netID, int selfID)
        {
            netID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[groupSelfieManager.selfiePosIndex].gameObject.SetActive(false);
            //netID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[selfID].gameObject.SetActive(true);
        }

        // Command function to open new circle indicator after new client join the group and the circle indicator position will updated
        [Command(requiresAuthority = false)]
        void CmdChangeCircle(NetworkIdentity netID)
        {
            Debug.Log(netID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex);
            int selfID = netID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;
            ///netID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[selfID].gameObject.SetActive(true);
        }

        // Rpc function to open new circle indicator after new client join the group and the circle indicator position will updated
        [ClientRpc]
        void RpcChangeCircle(NetworkIdentity netID, int selfID)
        {
            netID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[selfID].gameObject.SetActive(true);
        }

        // Call function after client want to exit the Group Selfie
        public void YesExit()
        {
            groupSelfieObj.ExitRequestPanel.gameObject.SetActive(false);
            valueScript.otherClientIDs = NetworkClient.localPlayer.gameObject.GetComponent<NetworkIdentity>().netId;

            valueScript.GroupID = Convert.ToInt32(valueScript.otherClientIDs);
            GameObject thisObject = NetworkClient.localPlayer.gameObject;
            valueScript.indexSaved = NetworkClient.localPlayer.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;

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

                    int currentCircle = posID.gameObject.GetComponent<ValueScript>().indexSaved;

                    posID.gameObject.GetComponent<ValueScript>().currentIndex = posID.gameObject.GetComponent<GroupSelfieManager>().countNum;
                    posID.gameObject.GetComponent<GroupSelfieManager>().currentIndex = posID.gameObject.GetComponent<ValueScript>().currentIndex;

                    posID.gameObject.GetComponent<ValueScript>().indexContinue = posID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;
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
                //netID.gameObject.GetComponent<PlayerNetworkBehaviour>().currentIndex = currentIndex;
                netID.gameObject.GetComponent<GroupSelfieManager>().currentIndex = currentIndex;
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
                //netID.gameObject.GetComponent<PlayerNetworkBehaviour>().currentIndex = currentIndex;
                netID.gameObject.GetComponent<GroupSelfieManager>().currentIndex = currentIndex;
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

            if (localID.gameObject.GetComponent<GroupSelfieManager>().isRaising == true)
            {
                localID.gameObject.GetComponent<GoupSelfieScript>().originVector = localID.gameObject.GetComponent<Transform>().position;
                localID.gameObject.GetComponent<GoupSelfieScript>().newOriginVectopr = new Vector3(localID.gameObject.GetComponent<GoupSelfieScript>().originVector.x, localID.gameObject.GetComponent<GoupSelfieScript>().originVector.y - 0.6f, localID.gameObject.GetComponent<GoupSelfieScript>().originVector.z);
                localID.gameObject.GetComponent<Transform>().gameObject.transform.position = localID.gameObject.GetComponent<GoupSelfieScript>().newOriginVectopr;

                localID.gameObject.GetComponent<ThirdPersonController>().enabled = true;
                localID.gameObject.GetComponent<GroupSelfieManager>().isRaising = false;
            }

            localID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 2;
            localID.gameObject.GetComponent<ThirdPersonController>().SprintSpeed = 5.335f;
            localID.gameObject.GetComponent<ThirdPersonController>().enabled = true;
            localID.gameObject.GetComponent<GroupSelfieManager>().isCenterPos = false;
            //localID.gameObject.GetComponent<GroupSelfieCanvas>().BackHeight();
            localID.gameObject.GetComponent<ValueScript>().GroupID = grouIDs;

            if (posID.gameObject.GetComponent<ValueScript>().readyChange == true)
            {
                if (posID.gameObject.GetComponent<ValueScript>().isNext == false)
                {
                    posID.gameObject.GetComponent<ValueScript>().anySpace = true;
                    posID.gameObject.GetComponent<ValueScript>().indexSaved = localID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;
                    int currentCircle = posID.gameObject.GetComponent<ValueScript>().indexSaved;
                    //posID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[currentCircle].gameObject.SetActive(true);

                    posID.gameObject.GetComponent<ValueScript>().currentIndex = posID.gameObject.GetComponent<GroupSelfieManager>().countNum;
                    posID.gameObject.GetComponent<ValueScript>().indexContinue = posID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;
                    int nextCircle = posID.gameObject.GetComponent<ValueScript>().indexContinue;
                    //posID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[nextCircle].gameObject.SetActive(false);

                    posID.gameObject.GetComponent<ValueScript>().isNext = true;
                }
                else if (posID.gameObject.GetComponent<ValueScript>().isNext == true)
                {
                    posID.gameObject.GetComponent<ValueScript>().anySpace = true;

                    posID.gameObject.GetComponent<ValueScript>().indexSaved = localID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;
                }
            }

            localID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex = 0;
            RpcYesExitGroup(localID.connectionToClient, localID, posID, grouIDs);
        }

        // Enable clients control after choosing to exit the group selfie, also aneble the selfie button and join button
        [TargetRpc]
        void RpcYesExitGroup(NetworkConnectionToClient netConID, NetworkIdentity netID, NetworkIdentity localPosID, int groupIDs)
        {
            netID.gameObject.GetComponent<GroupSelfieGameObj>().buttonSelfieCanvas.gameObject.SetActive(true);

            if (isLocalPlayer)
            {
                localPosID.gameObject.GetComponent<GroupSelfieGameObj>().joinButtonCanvas.gameObject.SetActive(true);
                Debug.Log("TestJoinButton");
                localPosID.gameObject.GetComponent<GroupSelfieGameObj>().ExitButton.gameObject.SetActive(false);
            }

            if (netID.gameObject.GetComponent<GroupSelfieManager>().isRaising == true)
            {

                netID.gameObject.GetComponent<GoupSelfieScript>().originVector = netID.gameObject.GetComponent<Transform>().position;

                netID.gameObject.GetComponent<GoupSelfieScript>().newOriginVectopr = new Vector3(netID.gameObject.GetComponent<GoupSelfieScript>().originVector.x, netID.gameObject.GetComponent<GoupSelfieScript>().originVector.y - 0.6f, netID.gameObject.GetComponent<GoupSelfieScript>().originVector.z);

                netID.gameObject.GetComponent<Transform>().gameObject.transform.position = netID.gameObject.GetComponent<GoupSelfieScript>().newOriginVectopr;

                netID.gameObject.GetComponent<ThirdPersonController>().enabled = true;
                netID.gameObject.GetComponent<GroupSelfieManager>().isRaising = false;
            }

            netID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 2;
            netID.gameObject.GetComponent<ThirdPersonController>().SprintSpeed = 5.335f;
            netID.gameObject.GetComponent<ThirdPersonController>().enabled = true;
            netID.gameObject.GetComponent<GroupSelfieManager>().isCenterPos = false;
            netID.gameObject.GetComponent<ValueScript>().GroupID = groupIDs;

            if (localPosID.gameObject.GetComponent<ValueScript>().readyChange == true)
            {
                if (localPosID.gameObject.GetComponent<ValueScript>().isNext == false)
                {
                    localPosID.gameObject.GetComponent<ValueScript>().anySpace = true;
                    localPosID.gameObject.GetComponent<ValueScript>().indexSaved = netID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;
                    localPosID.gameObject.GetComponent<ValueScript>().currentIndex = localPosID.gameObject.GetComponent<GroupSelfieManager>().countNum;
                    localPosID.gameObject.GetComponent<ValueScript>().indexContinue = localPosID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;

                    localPosID.gameObject.GetComponent<ValueScript>().isNext = true;
                }
                else if (localPosID.gameObject.GetComponent<ValueScript>().isNext == true)
                {
                    localPosID.gameObject.GetComponent<ValueScript>().anySpace = true;
                    localPosID.gameObject.GetComponent<ValueScript>().indexSaved = netID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;
                }
            }

            netID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex = 0;
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

            //localID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[groupSelfieManager.selfiePosIndex].gameObject.SetActive(false);
            int localSelf = localID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;
            //RpcChangeLocalCircle(localID.connectionToClient, localID, localSelf);

            localID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex--;
            localID.gameObject.GetComponent<GroupSelfieManager>().countNum--;
            localID.gameObject.GetComponent<GroupSelfieManager>().isCenterPos = false;

            int selfID = localID.gameObject.GetComponent<GroupSelfieManager>().selfiePosIndex;
            //localID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[selfID].gameObject.SetActive(true);

            RpcTargetChangeIndexMinus(localID.connectionToClient, localID, selfID, netID);
        }

        // Rpc function to close circle indicator after exit the group
        [TargetRpc]
        void RpcChangeLocalCircle(NetworkConnectionToClient netCon, NetworkIdentity netID, int localSelf)
        {
            netID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[localSelf].gameObject.SetActive(false);
            CmdChangeCircleAll(netID, localSelf);
        }

        // Command function to close circle indicator after exit the group
        [Command(requiresAuthority = false)]
        void CmdChangeCircleAll(NetworkIdentity netID, int localSelf)
        {
            RpcChangeCircleAll(netID, localSelf);
        }

        // Rpc function to close circle indicator after the client exit the group at others players view
        [ClientRpc]
        void RpcChangeCircleAll(NetworkIdentity netID, int localSelf)
        {
            netID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[localSelf].gameObject.SetActive(false);
        }

        // Rpc function to change index in one of the client who open the group selfie
        [TargetRpc]
        void RpcTargetChangeIndexMinus(NetworkConnectionToClient netCon, NetworkIdentity netID, int selfID, NetworkIdentity locNetID)
        {
            if (netID.gameObject.GetComponent<ValueScript>().readyChange == true)
            {
                netID.gameObject.GetComponent<ValueScript>().anySpace = true;
            }

            //netID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[groupSelfieManager.selfiePosIndex].gameObject.SetActive(false);
            netID.gameObject.GetComponent<ValueScript>().changeIndex = true;
            CmdIndexTexMin(netID, selfID, locNetID);
        }

        // Command function to call Rpc for changing text
        [Command(requiresAuthority = false)]
        void CmdIndexTexMin(NetworkIdentity netID, int selfID, NetworkIdentity locNetID)
        {
            RpcTextIndexMin(netID, selfID, locNetID);
        }

        // Rpc function that called for change counting text after one of the client is leaving the group and also set bool function when there are player leaves the group
        [ClientRpc]
        void RpcTextIndexMin(NetworkIdentity netID, int selfID, NetworkIdentity locNetID)
        {
            netID.gameObject.GetComponent<GroupSelfieGameObj>().SelfieCanvas.gameObject.SetActive(true);

            if (netID.gameObject.GetComponent<ValueScript>().readyChange == true)
            {
                netID.gameObject.GetComponent<ValueScript>().anySpace = true;
            }

            if (locNetID.gameObject.GetComponent<GroupSelfieManager>().isRaising == true)
            {
                locNetID.gameObject.GetComponent<GoupSelfieScript>().originVector = locNetID.gameObject.GetComponent<Transform>().position;
                locNetID.gameObject.GetComponent<GoupSelfieScript>().newOriginVectopr = new Vector3(locNetID.gameObject.GetComponent<GoupSelfieScript>().originVector.x, locNetID.gameObject.GetComponent<GoupSelfieScript>().originVector.y - 0.6f, locNetID.gameObject.GetComponent<GoupSelfieScript>().originVector.z);
                locNetID.gameObject.GetComponent<Transform>().gameObject.transform.position = locNetID.gameObject.GetComponent<GoupSelfieScript>().newOriginVectopr;

                locNetID.gameObject.GetComponent<ThirdPersonController>().enabled = true;
                locNetID.gameObject.GetComponent<GroupSelfieManager>().isRaising = false;
            }

            netID.gameObject.GetComponent<GroupSelfieGameObj>().joinButtonCanvas.gameObject.SetActive(false);

            if (isLocalPlayer)
            {
                netID.gameObject.GetComponent<GroupSelfieGameObj>().joinButtonCanvas.gameObject.SetActive(true);
            }
            
            netID.gameObject.GetComponent<GroupSelfieManager>().selfiePos[selfID].gameObject.SetActive(true);
            netID.gameObject.GetComponent<GroupSelfieGameObj>().currentText.SetText(netID.gameObject.GetComponent<GroupSelfieManager>().countNum.ToString());
        }
    }
}
