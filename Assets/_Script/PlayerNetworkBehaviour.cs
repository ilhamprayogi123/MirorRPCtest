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

namespace StarterAssets
{
    public class PlayerNetworkBehaviour : NetworkBehaviour
    {
        private GameObject animationManager;
        public CoupleAnimationManager animManager;

        [SerializeField]
        private ValueScript valueScript;
        [SerializeField]
        private CharcControl charControl;
        [SerializeField]
        private PosRotScript posRot;
        [SerializeField]
        private AnimScript animScript;
        [SerializeField]
        private UiCanvas uiCanvasObj;
        [SerializeField]
        private GameObjectScript gameObjectScript;

        private GetButtonFunc buttonFunc;
        
        public List<GameObject> ReqButton;
        public List<CoupleAnimationData> animDatas;
        
        [SyncVar]
        public int maxIndex;
        [SyncVar]
        public int indexNum;

        private bool isMax;
        public LayerMask mask;

        [SerializeField] public float countAnimTime = 10.0f;

        public Animator animator;

        [SerializeField]
        private InputHandler inputData;
        [SerializeField]
        private GameObject inputPrefab;

        public Button assignButton;

        CinemachineVirtualCamera virtualCam;

        [SerializeField] private Transform camTransform;
        
        [SyncVar] private GameObject objectID;

        GameObject playerCam;
        
        [SyncVar(hook = "OnChangeID")]
        public uint idNetwork;
        
        public Transform targe;
        public TMP_Text playerNameText;

        [SyncVar(hook = nameof(RpcChangeIndex))]
        public int selfiePosIndex;

        public GameObject[] selfiePos;

        [SyncVar(hook = nameof(OnDisplayNameChangeUpdated))]
        public string playerName;

        [SerializeField] 
        private string inputName;
        [SerializeField]
        private string playName;

        public TMP_Text animRequest;

        // Start is called before the first frame update
        void Start()
        {
            isMax = false;
            valueScript.localID = this.gameObject.GetComponent<NetworkIdentity>().netId;
            valueScript.locID = valueScript.localID;

            posRot.targetRot = this.gameObject.GetComponent<Transform>().rotation;

            inputPrefab = GameObject.FindGameObjectWithTag("Input");
            inputData = inputPrefab.transform.GetComponentInChildren<InputHandler>();

            animationManager = GameObject.FindGameObjectWithTag("AnimateManager");
            animManager = animationManager.gameObject.GetComponent<CoupleAnimationManager>();
            animDatas = animationManager.GetComponent<CoupleAnimationManager>().animationData;

            if (isLocalPlayer)
            {
                this.gameObject.GetComponent<MeshCollider>().enabled = false;
                playName = inputName;

                assignButton.onClick.AddListener(assignAct);
                playName = inputData.InputText;
                
                CharacterController charControl = GetComponent<CharacterController>();
                PlayerInput playInput = GetComponent<PlayerInput>();
                ThirdPersonController TPControl = GetComponent<ThirdPersonController>();

                charControl.enabled = true;
                playInput.enabled = true;
                TPControl.enabled = true;

                playerCam = GameObject.Find("PlayerFollowCamera");
                virtualCam = playerCam.GetComponent<CinemachineVirtualCamera>();
                virtualCam.Follow = targe;
            }
        }

        // Setup client name in server side
        [Command]
        public void CmdSetupPlayer(string _name)
        {
            RpcClientName(_name);
            playerName = _name;
            playerNameText.text = playerName;
            gameObject.name = playerName;
        }

        // Function to call Commmand Function to change name
        public void assignAct()
        {
            inputName = inputData.InputText;
            playName = inputName;
            CmdSetupPlayer(playName);
        }
        
        // Change Player name
        private void OnDisplayNameChangeUpdated(string oldName, string newName)
        {
            gameObject.name = playerName + " Player";
            playerNameText.text = playerName;
        }

        // Change player name to display for all other client
        [ClientRpc]
        public void RpcClientName(string name)
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
                CmdSetupPlayer(playName);
            }
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

        // Debug log this player Index
        void RpcChangeIndex(int oldValue, int newValue)
        {
            Debug.Log("Your new Index is : " + newValue);
        }
        
        public void MaxIndex()
        {
            gameObjectScript.joinButtonCanvas.gameObject.SetActive(false);
            Debug.Log("Maximum Client");
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
            
            idNetwork = valueScript.idNet;
            playName = inputData.InputText;

            if (isLocalPlayer && Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100, mask))
                {
                    objectID = GameObject.Find(hit.transform.gameObject.name);
                    valueScript.idNet = objectID.GetComponent<NetworkIdentity>().netId;
                    valueScript.localID = this.gameObject.GetComponent<NetworkIdentity>().netId;

                    posRot.SpawnVar = objectID.gameObject.GetComponent<Transform>().position;
                    posRot.locRot = objectID.gameObject.GetComponent<Transform>().rotation;

                    posRot.newVar = new Vector3(posRot.SpawnVar.x, posRot.SpawnVar.y, posRot.SpawnVar.z + 0.75f);
                    posRot.newRot = new Quaternion(posRot.locRot.x, posRot.locRot.y + 180f, posRot.locRot.z, posRot.locRot.w);

                    OnChangeID(idNetwork, valueScript.idNet);
                    animScript.CmdSelf(valueScript.localID);
                    animScript.CmdClick(valueScript.idNet, posRot.newVar, valueScript.localID, posRot.newRot);
                    //LocaleCmd(localID);
                }
            }
        }
    }
}
