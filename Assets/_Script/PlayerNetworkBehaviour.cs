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

namespace StarterAssets
{
    public class PlayerNetworkBehaviour : NetworkBehaviour
    {
        private GameObject animationManager;
        CoupleAnimationManager animManager;

        [SerializeField]
        private UiCanvas uiCanvasObj;

        private GetButtonFunc buttonFunc;
        //private Button joinObject;

        public List<GameObject> ReqButton;
        public List<CoupleAnimationData> animDatas;
        
        [SyncVar]
        private int animID;
        [SyncVar]
        private int intIndexAnim;
        [SyncVar]
        public int varIndex;
        [SyncVar]
        private int maxIndex = 4;
        
        private bool isMax;
        public LayerMask mask;

        [SerializeField] private float countAnimTime = 10.0f;

        public Animator animator;

        [SerializeField]
        private InputHandler inputData;
        
        [SerializeField]
        private GameObject inputPrefab;

        public Button assignButton;

        CinemachineVirtualCamera virtualCam;

        public GameObject sphere;
        
        public bool isSphereActive = false;

        public GameObject cubePrefab;
        public GameObject objectPos;
        private NetworkIdentity objNetId;

        private int _greetAnimID;
        //private bool _hasAnimator;

        [SerializeField] private Transform camTransform;
        
        [SyncVar] private GameObject objectID;

        private GameObject objectPlay;
        private Color objectColor;

        GameObject playerCam;
        
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
        
        [SyncVar(hook = "OnChangeID")]
        public uint idNetwork;
        
        [SyncVar]
        public uint objId;
        public Transform targe;

        public TMP_Text playerNameText;

        [SyncVar(hook = nameof(RpcChangeIndex))]
        public int selfiePosIndex;

        public GameObject[] selfiePos;

        [SyncVar]
        private Vector3 newVar;
        [SyncVar]
        private Vector3 SpawnVar;
        [SyncVar]
        private Quaternion newRot;
        [SyncVar]
        private Quaternion locRot;
        [SyncVar]
        private Quaternion targetRot;

        [SyncVar(hook = nameof(OnDisplayNameChangeUpdated))]
        public string playerName;

        [SerializeField] 
        private string inputName;
        [SerializeField]
        private string playName;

        //public bool changePos;
        public TMP_Text animRequest;

        // Start is called before the first frame update
        void Start()
        {
            //changePos = false;
            isMax = false;
            sphere.gameObject.SetActive(false);

            localID = this.gameObject.GetComponent<NetworkIdentity>().netId;
            locID = localID;

            targetRot = this.gameObject.GetComponent<Transform>().rotation;

            inputPrefab = GameObject.FindGameObjectWithTag("Input");
            inputData = inputPrefab.transform.GetComponentInChildren<InputHandler>();

            // Get animation manager data
            animationManager = GameObject.FindGameObjectWithTag("AnimateManager");
            animManager = animationManager.gameObject.GetComponent<CoupleAnimationManager>();
            animDatas = animationManager.GetComponent<CoupleAnimationManager>().animationData;

            if (isLocalPlayer)
            {
                this.gameObject.GetComponent<MeshCollider>().enabled = false;

                playName = inputName;
                Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

                assignButton.onClick.AddListener(assignAct);

                playName = inputData.InputText;
                
                CharacterController charControl = GetComponent<CharacterController>();
                PlayerInput playInput = GetComponent<PlayerInput>();
                ThirdPersonController TPControl = GetComponent<ThirdPersonController>();

                charControl.enabled = true;
                playInput.enabled = true;
                TPControl.enabled = true;

                // Camera follow locale player
                playerCam = GameObject.Find("PlayerFollowCamera");
                virtualCam = playerCam.GetComponent<CinemachineVirtualCamera>();
                virtualCam.Follow = targe;
            }
        }

        // Setup client name in server side
        [Command]
        public void CmdSetupPlayer(string _name, Color _col)
        {
            RpcClientName(_name, _col);
            playerName = _name;
            playerNameText.text = playerName;
            gameObject.name = playerName;
        }

        // Function to call Commmand Function to change name
        public void assignAct()
        {
            inputName = inputData.InputText;
            playName = inputName;
            
            Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            CmdSetupPlayer(playName, color);
        }
        
        // Command to give data to server about client interact with other client
        [Command]
        public void CmdClick(uint objectId, Vector3 locPos, uint localID, Quaternion locRot)
        {
            NetworkIdentity opponentId = NetworkServer.spawned[objectId];
            objId = objectId;
            uint idLocale = localID;
            Debug.Log(this.gameObject.name + " is clicking " + opponentId.gameObject.name);
            TargetClick(opponentId.connectionToClient, objId, locPos, opponentId, idLocale, locRot);
        }

        // First interact for client and make the player who got clicked can't move
        [TargetRpc]
        public void TargetClick(NetworkConnectionToClient netId, uint idNetwork, Vector3 posSpawn, NetworkIdentity networkID, uint localeIDs, Quaternion locRot)
        {
            Debug.Log(this.gameObject.name + " Has clicked you !");

            networkID.gameObject.GetComponent<CharacterController>().enabled = false;
            networkID.gameObject.GetComponent<PlayerInput>().enabled = false;
            networkID.gameObject.GetComponent<ThirdPersonController>().enabled = false;
            uiCanvasObj.WaitCanvas.gameObject.SetActive(true);

            locID = localeIDs;
        }

        // Change Player name
        private void OnDisplayNameChangeUpdated(string oldName, string newName)
        {
            gameObject.name = playerName + " Player";
            playerNameText.text = playerName;
        }

        // Change player name to display for all other client
        [ClientRpc]
        public void RpcClientName(string name, Color col)
        {
            playerName = name;
        }

        // Call Command Function to change name
        public void InputName()
        {
            if (isLocalPlayer)
            {
                inputName = inputData.InputText;
                playName = inputName;
                Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                CmdSetupPlayer(playName, color);
            }
        }
        
        // Command Function to call the animation using the data that was successfully obtained from the animation manager and also change position locale client position in server side
        [Command]
        void CmdAnimation(uint objectId, uint localID, int animIndex)
        {
            Debug.Log("Test Anim");
            uint objId = GetComponent<NetworkIdentity>().netId;
            uint locIDs = GetComponent<NetworkIdentity>().netId;

            objId = objectId;
            locIDs = localID;

            NetworkIdentity opponentId = NetworkServer.spawned[objectId];
            NetworkIdentity localeID = NetworkServer.spawned[localID];

            localeID.gameObject.GetComponent<Transform>().position = newVar;
            localeID.gameObject.GetComponent<Transform>().rotation = newRot;

            opponentId.gameObject.GetComponent<Transform>().rotation = targetRot;

            StartCoroutine(animTimePlay(countAnimTime));

            IEnumerator animTimePlay(float animPlay)
            {
                opponentId.gameObject.GetComponent<Animator>().CrossFadeInFixedTime(animDatas[animIndex].AnimState, 0.1f);
                yield return new WaitForSeconds(animPlay);
                opponentId.gameObject.GetComponent<Animator>().CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);
            }
            RpcAnimator(opponentId.connectionToClient, objId, opponentId, localeID, animIndex);
        }

        // Function to call the animation using the data that was successfully obtained from the animation manager in local client side, also change locale player position and rotation
        [TargetRpc]
        void RpclocalAnimPlay(NetworkIdentity localeID, int animIndex)
        {
            localeID.gameObject.GetComponent<Transform>().position = newVar;
            localeID.gameObject.GetComponent<Transform>().rotation = newRot;

            StartCoroutine(animTimePlay(countAnimTime));

            IEnumerator animTimePlay(float animTime)
            {
                animator.CrossFadeInFixedTime(animDatas[animIndex].AnimState, 0.1f);
                localeID.gameObject.GetComponent<CharacterController>().enabled = false;
                localeID.gameObject.GetComponent<PlayerInput>().enabled = false;
                localeID.gameObject.GetComponent<ThirdPersonController>().enabled = false;

                yield return new WaitForSeconds(animTime);

                animator.CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);
                localeID.gameObject.GetComponent<CharacterController>().enabled = true;
                localeID.gameObject.GetComponent<PlayerInput>().enabled = true;
                localeID.gameObject.GetComponent<ThirdPersonController>().enabled = true;
            }
            localeID.gameObject.GetComponent<UiCanvas>().WaitCanvas.gameObject.SetActive(false);
            CmdAnimation(idNetwork, locID, animIndex);
        }

        // Function to call animation for other client, also change to rotation of other client game object
        [TargetRpc]
        void RpcAnimator(NetworkConnectionToClient netId, uint objectId, NetworkIdentity networkID, NetworkIdentity localeID, int animindex)
        {
            localeID.gameObject.GetComponent<Transform>().position = newVar;
            localeID.gameObject.GetComponent<Transform>().rotation = newRot;

            networkID.gameObject.GetComponent<Transform>().rotation = targetRot;

            StartCoroutine(animTimePlay(countAnimTime));

            IEnumerator animTimePlay(float animTime)
            {
                networkID.gameObject.GetComponent<Animator>().CrossFadeInFixedTime(animDatas[animindex].AnimState, 0.1f);
                networkID.gameObject.GetComponent<CharacterController>().enabled = false;
                networkID.gameObject.GetComponent<PlayerInput>().enabled = false;
                networkID.gameObject.GetComponent<ThirdPersonController>().enabled = false;

                yield return new WaitForSeconds(animTime);

                networkID.gameObject.GetComponent<Animator>().CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);
                networkID.gameObject.GetComponent<CharacterController>().enabled = true;
                networkID.gameObject.GetComponent<PlayerInput>().enabled = true;
                networkID.gameObject.GetComponent<ThirdPersonController>().enabled = true;
            }
        }
        
        // Call Command Function to play Couple Animation
        public void YesAnswer()
        {
            CmDAnimPlay(varIndex);
        }

        // Call Command Function if client don't want ro play Couple Animation
        public void noAnswer()
        {
            NoPlay();
        }

        // Command Function to call Rpc function for cancel the request
        [Command(requiresAuthority = false)]
        void NoPlay()
        {
            NetworkIdentity localePlay = NetworkServer.spawned[locID];
            RpcNoPlay(localePlay.connectionToClient, localePlay);
        }

        // Command Function to call the animation using the data that was successfully obtained from the animation manager for locale player in server side 
        [Command(requiresAuthority = false)]
        void CmDAnimPlay(int animIndex)
        {
            NetworkIdentity localePlay = NetworkServer.spawned[locID];
            StartCoroutine(animTimePlay(countAnimTime));

            IEnumerator animTimePlay(float animTime)
            {
                animator.CrossFadeInFixedTime(animDatas[animIndex].AnimState, 0.1f);
                yield return new WaitForSeconds(animTime);
                animator.CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);
            }
            RpclocalAnimPlay(localePlay, animIndex);
        }

        // Rpc Function to enable local client control and close wait canvas in locale client
        [TargetRpc]
        void RpcNoPlay(NetworkConnectionToClient netConID, NetworkIdentity localeID)
        {
            localeID.gameObject.GetComponent<CharacterController>().enabled = true;
            localeID.gameObject.GetComponent<PlayerInput>().enabled = true;
            localeID.gameObject.GetComponent<ThirdPersonController>().enabled = true;
            localeID.gameObject.GetComponent<UiCanvas>().WaitCanvas.gameObject.SetActive(false);

            CmdSetUpNo(idNetwork);
        }

        // Command function to call RPC Function to enable other player control
        [Command]
        void CmdSetUpNo(uint objectIdentity)
        {
            uint objId = GetComponent<NetworkIdentity>().netId;
            objId = objectIdentity;
            NetworkIdentity opponentId = NetworkServer.spawned[objectIdentity];
            RpcNoPlayClient(opponentId.connectionToClient, objId, opponentId);
        }

        // Command function to call RPC function for disable control and open the animation canvas in local player
        [Command]
        public void CmdSelf(uint localID)
        {
            NetworkIdentity localeID = NetworkServer.spawned[localID];
            uint idLocale = localID;
            RpcSelf(localeID.connectionToClient, localeID, idLocale);
        }

        // Open animation canvas and disable the local player control
        [TargetRpc]
        void RpcSelf(NetworkConnectionToClient networkID, NetworkIdentity netID, uint localeID)
        {
            netID.gameObject.GetComponent<UiCanvas>().AnimationCanvas.gameObject.SetActive(true);
            netID.gameObject.GetComponent<CharacterController>().enabled = false;
            netID.gameObject.GetComponent<PlayerInput>().enabled = false;
            netID.gameObject.GetComponent<ThirdPersonController>().enabled = false;
            netID.gameObject.GetComponent<PlayerNetworkBehaviour>().animManager.SpawnButton();

            locID = localeID;
        }

        // Enable client control for other client
        [TargetRpc]
        void RpcNoPlayClient(NetworkConnectionToClient netId, uint objectId, NetworkIdentity networkID)
        {
            networkID.gameObject.GetComponent<CharacterController>().enabled = true;
            networkID.gameObject.GetComponent<PlayerInput>().enabled = true;
            networkID.gameObject.GetComponent<ThirdPersonController>().enabled = true;
            //networkID.gameObject.GetComponent<UiCanvas>().WaitCanvas.gameObject.SetActive(false);
        }

        // Changing the mask makes the client only able to interact with objects with the same mask
        private void FixedUpdate()
        {
            mask = LayerMask.GetMask("Player");
        }

        // Get network ID
        void OnChangeID(uint oldID, uint newID)
        {
            idNetwork = newID;
        }

        // Call Command Function to open group selfie canvas
        public void SelfieButtonFunc()
        {
            if (isLocalPlayer)
            {
                localeSelfieID = this.gameObject.GetComponent<NetworkIdentity>().netId;
                CmdSelfieLocalePanel(localeSelfieID);
            }
        }
        
        // Function to call Ommand Function to Close Group Selfie
        public void CloseSelfieGroup()
        {
            if (isLocalPlayer)
            {
                localeSelfieID = this.gameObject.GetComponent<NetworkIdentity>().netId;
                CmdSlefieCloselocalePanel(localeSelfieID);
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
        

        // Command function to call RPC function for closing the group selfie
        [Command]
        void CmdSlefieCloselocalePanel(uint localNetID)
        {
            NetworkIdentity localeNetID = NetworkServer.spawned[localNetID];
            localeNetID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 2;
            RpcSelfieLocalClose(localeNetID.connectionToClient, localeNetID);
        }
        
        // Call RPC function to display Selfie Panel from Server
        [Command]
        void CmdSelfiePanelOther(NetworkIdentity localId)
        {
            RpcPanelSelfie(localId);
        }
        
        // Call RPC function to closse Selfie Panel from Server
        [Command]
        void CmdClosePanelOther(NetworkIdentity localId)
        {
            RpcPanelSelfieClose(localId);
        }
        
        // Open selfie group panel in local player and disable client move
        [TargetRpc]
        void RpcSelfieLocal(NetworkConnectionToClient netConID, NetworkIdentity localNetID)
        {
            localNetID.gameObject.GetComponent<UiCanvas>().SelfieCanvas.gameObject.SetActive(true);
            localNetID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 0;
            uiCanvasObj.joinButtonCanvas.gameObject.SetActive(false);
            CmdSelfiePanelOther(localNetID);
        }
        
        // Close selfie group and enable client move
        [TargetRpc]
        void RpcSelfieLocalClose(NetworkConnectionToClient netConID, NetworkIdentity localNetID)
        {
            localNetID.gameObject.GetComponent<UiCanvas>().SelfieCanvas.gameObject.SetActive(false);
            localNetID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 2;
            uiCanvasObj.joinButtonCanvas.gameObject.SetActive(true);
            CmdClosePanelOther(localNetID);
        }
        
        // Display locale client selfie panel to all others client
        [ClientRpc]
        void RpcPanelSelfie(NetworkIdentity localeID)
        {
            localeID.gameObject.GetComponent<UiCanvas>().SelfieCanvas.gameObject.SetActive(true);

            if (isLocalPlayer)
            {
                uiCanvasObj.joinButtonCanvas.gameObject.SetActive(false);
                //ExitButton.gameObject.SetActive(true);
            }
        }
        
        // Close selfie canvas for local client
        [ClientRpc]
        void RpcPanelSelfieClose(NetworkIdentity localeID)
        {
            YesExit();
            localeID.gameObject.GetComponent<UiCanvas>().SelfieCanvas.gameObject.SetActive(false);
        }
        

        // Change indez for clients for join the group selfie
        void ChangeIndexNum()
        {
            GameObject[] playerTarget = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject playerNum in playerTarget)
            {
                playerNum.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePosIndex++;
            }
        }

        // Call function after client agree to join Group Selfie
        public void YesJoin()
        {
            uiCanvasObj.SelfieReqPanel.gameObject.SetActive(false);

            otherClientIDs = NetworkClient.localPlayer.gameObject.GetComponent<NetworkIdentity>().netId;
            GameObject thisObject = NetworkClient.localPlayer.gameObject;
            CmdYesjoin(otherClientIDs, testClientID);
        }

        // Call function after client want to ext the Group Selfie
        public void YesExit()
        {
            uiCanvasObj.ExitRequestPanel.gameObject.SetActive(false);
            otherClientIDs = NetworkClient.localPlayer.gameObject.GetComponent<NetworkIdentity>().netId;
            GameObject thisObject = NetworkClient.localPlayer.gameObject;
            CmdExitGroup(otherClientIDs, testClientID);
        }

        // Command Function to change position and rotation in Server Side
        [Command]
        void CmdYesjoin(uint corePosID, uint centerPosID)
        {
            NetworkIdentity localID = NetworkServer.spawned[corePosID];
            NetworkIdentity posID = NetworkServer.spawned[centerPosID];
            //localID.GetComponent<PlayerNetworkBehaviour>().changePos = true;

            localID.gameObject.GetComponent<Transform>().position = posID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePos[selfiePosIndex].transform.position;
            localID.gameObject.GetComponent<Transform>().rotation = posID.gameObject.GetComponent<Transform>().rotation;

            localID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 0;
            RpcYesJoin(localID.connectionToClient, localID, posID);
        }

        // Command function to cal RPC funtion for exit the group and enable client move in server side
        [Command(requiresAuthority = false)]
        void CmdExitGroup(uint corePosID, uint centerPosID)
        {
            NetworkIdentity localID = NetworkServer.spawned[corePosID];
            NetworkIdentity posID = NetworkServer.spawned[centerPosID];

            localID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 2;
            RpcYesExitGroup(localID.connectionToClient, localID, posID);
        }

        // RPC Function to change / transform position and rotation in Client Side
        [TargetRpc]
        void RpcYesJoin(NetworkConnectionToClient netConID, NetworkIdentity netID, NetworkIdentity localPosID)
        {
            //netID.GetComponent<PlayerNetworkBehaviour>().changePos = true;
            uiCanvasObj.buttonSelfieCanvas.gameObject.SetActive(false);
            
            netID.gameObject.GetComponent<Transform>().position = localPosID.gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePos[selfiePosIndex].transform.position;
            netID.gameObject.GetComponent<Transform>().rotation = localPosID.gameObject.GetComponent<Transform>().rotation;

            if (isLocalPlayer)
            {
                localPosID.gameObject.GetComponent<UiCanvas>().joinButtonCanvas.gameObject.SetActive(false);
                localPosID.gameObject.GetComponent<UiCanvas>().ExitButton.gameObject.SetActive(true);
            }

            netID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 0;
            CmdChangeIndex(netID, localPosID);
            //ChangeIndexNum();
        }

        // Enable clients control after choosing to exit the group selfie, also aneble the selfie button and join button
        [TargetRpc]
        void RpcYesExitGroup(NetworkConnectionToClient netConID, NetworkIdentity netID, NetworkIdentity localPosID)
        {
            uiCanvasObj.buttonSelfieCanvas.gameObject.SetActive(true);
            
            if (isLocalPlayer)
            {
                localPosID.gameObject.GetComponent<UiCanvas>().joinButtonCanvas.gameObject.SetActive(true);
                localPosID.gameObject.GetComponent<UiCanvas>().ExitButton.gameObject.SetActive(false);
            }
            netID.gameObject.GetComponent<ThirdPersonController>().MoveSpeed = 2;
        }

        // Command Function to call function to change Index foe Group Selfie
        [Command]
        void CmdChangeIndex(NetworkIdentity netID, NetworkIdentity localID)
        {
            ChangeIndexNum();  
        }
        
        // Debug log this player Index
        void RpcChangeIndex(int oldValue, int newValue)
        {
            Debug.Log("Your new Index is : " + newValue);
        }

        public void MaxIndex()
        {
            uiCanvasObj.joinButtonCanvas.gameObject.SetActive(false);
            isMax = false;
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 100f;
            
            if (selfiePosIndex > maxIndex)
            { 
                selfiePosIndex = maxIndex;
                isMax = true;
                if (isMax == true)
                {
                    MaxIndex();
                }
            }
            
            //_hasAnimator = TryGetComponent(out animator);
            idNetwork = idNet;
            //animationID = animID;
            playName = inputData.InputText;

            if (isLocalPlayer && Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100, mask))
                {
                    objectID = GameObject.Find(hit.transform.gameObject.name);
                    idNet = objectID.GetComponent<NetworkIdentity>().netId;
                    localID = this.gameObject.GetComponent<NetworkIdentity>().netId;
                    
                    SpawnVar = objectID.gameObject.GetComponent<Transform>().position;
                    locRot = objectID.gameObject.GetComponent<Transform>().rotation;

                    newVar = new Vector3(SpawnVar.x, SpawnVar.y, SpawnVar.z + 0.75f);
                    newRot = new Quaternion(locRot.x, locRot.y + 180f, locRot.z, locRot.w);

                    OnChangeID(idNetwork, idNet);
                    CmdSelf(localID);
                    CmdClick(idNet, newVar, localID, newRot);
                    //LocaleCmd(localID);
                }
            }
            
            if (isLocalPlayer && Input.GetKeyDown(KeyCode.K))
            {
                uiCanvasObj.AnimationCanvas.gameObject.SetActive(true);
                this.gameObject.GetComponent<PlayerNetworkBehaviour>().animManager.SpawnButton();
            }
        }
    }
}
