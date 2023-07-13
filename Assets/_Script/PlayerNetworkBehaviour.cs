using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Cinemachine;
using TMPro;
using UnityEngine.UI;

namespace StarterAssets
{
    public class PlayerNetworkBehaviour : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnScoreCountChanged))]
        int scoreCount = 0;

        //[SerializeField]
        private GameObject animationManager;
        private CoupleAnimationManager animManager;

        private GetButtonFunc buttonFunc;

        //private bool yesReq;

        [SyncVar]
        private int animID;
        [SyncVar]
        private int animationID;

        public Button inputButton;
        public Button ToggleButton;

        public LayerMask mask;

        [SerializeField] private float countAnimTime = 10.0f;

        public Animator animator;

        [SerializeField]
        private InputHandler inputData;

        [SerializeField]
        private GameObject inputPrefab;

        [SerializeField]
        private GameObject requestPanel;

        public Button assignButton;

        public GameObject RequestCanvas;
        public GameObject ClapRequestCanvas;
        public GameObject DanceRequestCanvas;

        public GameObject WaitCanvas;
        public GameObject AnimationCanvas;
        public GameObject NoAnimCanvas;

        public GameObject SelfieReqPanel;
        public GameObject SelfieCanvas;
        public GameObject joinButtonCanvas;
        public GameObject buttonSelfieCanvas;

        CinemachineVirtualCamera virtualCam;

        public GameObject sphere;

        private NetworkIdentity networkIdentityID;

        public bool isSphereActive = false;

        public GameObject cubePrefab;
        public GameObject objectPos;
        private NetworkIdentity objNetId;

        private int _greetAnimID;
        private bool _hasAnimator;

        [SerializeField] private Transform camTransform;
        //private RaycastHit hit;

        [SyncVar] private GameObject objectID;

        private GameObject objectPlay;
        private Color objectColor;

        GameObject playerCam;

        [SerializeField] private uint netID;

        [SyncVar]
        private uint localID;
        [SyncVar]
        private uint locID;

        [SyncVar]
        private uint localeSelfieID;
        [SyncVar]
        private uint forOtherClientSelfieID;
        [SyncVar]
        private uint clientSelfID;
        [SyncVar]
        private uint otherClientIDs;
        [SyncVar]
        private uint otherClients;
        [SyncVar]
        private uint forOthers;

        public uint othersPosID;

        //[SyncVar]
        //public uint testLocID;
        [SyncVar]
        private uint testID;
        [SyncVar]
        private uint idNet;
        [SyncVar]
        private uint localeId;
        //[SyncVar(hook = "OnChangeID")]
        private uint networkId;
        [SyncVar(hook = "OnChangeID")]
        private uint idNetwork;
        [SyncVar]
        private uint startID;

        [SerializeField] private int healthCount = 20;

        public Transform targe;
        public GameObject floatingInfo;

        public bool isWin = false;
        public bool isDefeat = false;
        public bool isButtonActive = false;

        public TMP_Text playerNameText;

        [SyncVar]
        private Vector3 newVar;
        [SyncVar]
        private Vector3 SpawnVar;
        private Vector3 varSpawn;

        //[SyncVar]
        public GameObject selfiePos1;
        //[SyncVar]
        public GameObject selfiePos2;

        [SyncVar]
        private Vector3 newSelfiePos;
        [SyncVar]
        private Vector3 newSelfirPos2;

        [SyncVar]
        private Vector3 pointNewPos;

        [SyncVar]
        private Quaternion newRot;
        [SyncVar]
        private Quaternion locRot;
        [SyncVar]
        private Quaternion targetRot;

        [SyncVar(hook = nameof(OnDisplayNameChangeUpdated))]
        public string playerName;

        [SerializeField] 
        private string inputName = "Player";
        //[SerializeField] private string getName;

        [SerializeField]
        private string playName;

        // Start is called before the first frame update
        void Start()
        {
            //yesReq = false;
            _hasAnimator = TryGetComponent(out animator);
            //localIDchange = false;
            startID = this.gameObject.GetComponent<NetworkIdentity>().netId;

            sphere.gameObject.SetActive(false);

            localID = this.gameObject.GetComponent<NetworkIdentity>().netId;

            locID = localID;

            SelfieCanvas.gameObject.SetActive(false);

            ToggleButton = GameObject.FindGameObjectWithTag("ToggleButton").GetComponent<Button>();

            ToggleButton.onClick.AddListener(() => ButtonActivate());

            //newTargetRot = this.gameObject.GetComponent<Transform>().rotation;
            targetRot = this.gameObject.GetComponent<Transform>().rotation;

            inputPrefab = GameObject.FindGameObjectWithTag("Input");

            inputData = inputPrefab.transform.GetComponentInChildren<InputHandler>();

            AssignAnimationIDs();

            if (isLocalPlayer)
            {
                animationManager = GameObject.FindGameObjectWithTag("AnimateManager");
                animManager = animationManager.gameObject.GetComponent<CoupleAnimationManager>();

                buttonSelfieCanvas.gameObject.SetActive(true);
                this.gameObject.GetComponent<MeshCollider>().enabled = false;

                RequestCanvas.gameObject.SetActive(false);

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

                playerCam = GameObject.Find("PlayerFollowCamera");
                virtualCam = playerCam.GetComponent<CinemachineVirtualCamera>();
                virtualCam.Follow = targe;
            }
        }

        [Command]
        public void CmdSetupPlayer(string _name, Color _col)
        {
            //gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
            RpcClientName(_name, _col);
            playerName = _name;
            playerNameText.text = playerName;
            gameObject.name = playerName;
        }

        [Command]
        public void TestConnectCmd()
        {
            Debug.Log("Receive Hello from Client");
        }

        [Command]
        public void TestConnectAllCmd()
        {
            Debug.Log("Get Hello from the Client");
            ReplyServerToAll();
        }

        public void assignAct()
        {
            inputName = inputData.InputText;

            playName = inputName;

            Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

            CmdSetupPlayer(playName, color);
        }

        [Command]
        public void GetHelloCmd()
        {
            Debug.Log("Get Hola from client");
            TargetReplyHola();
        }

        [Command]
        public void GetScore()
        {
            Debug.Log("Player Get score");
            scoreCount += 10;
        }

        [Command]
        public void LoseHealthCommand()
        {
            Debug.Log("One player lose health");
            healthCount -= 10;
            HealthLoseClient();
        }

        [Command]
        public void PlayerAttack(GameObject target, int damage)
        {
            NetworkIdentity opponentIdentity = target.GetComponent<NetworkIdentity>();
            GetAttacked(opponentIdentity.connectionToClient, damage);
        }

        [Command]
        public void ChatCmd(uint netId)
        {
            NetworkIdentity opponentIdentity = NetworkServer.spawned[netId];
            Debug.Log("One player message other player");
            //healthCount -= damage;
            GotMessage(opponentIdentity.connectionToClient);
        }

        [Command]
        public void CmdSpawnItem()
        {
            Vector3 pos = objectPos.transform.position;
            Quaternion rot = objectPos.transform.rotation;
            GameObject newCubeObject = Instantiate(cubePrefab, pos, rot);

            NetworkServer.Spawn(newCubeObject);
        }

        [Server]
        public void StopClient(GameObject gameObj)
        {
            Debug.Log("One player removed");

            NetworkIdentity playerId = gameObj.GetComponent<NetworkIdentity>();
            TargetDisconnect(playerId.connectionToClient);
        }

        [TargetRpc]
        public void TargetDisconnect(NetworkConnectionToClient netId)
        {
            NetworkManager.singleton.StopClient();
        }

        [Command]
        public void CmdClick(uint objectId, Vector3 locPos, uint localID, Quaternion locRot)
        {
            NetworkIdentity opponentId = NetworkServer.spawned[objectId];

            uint objId = objectId;
            uint idLocale = localID;

            Debug.Log(this.gameObject.name + " is clicking " + opponentId.gameObject.name);

            //opponentId.gameObject.GetComponent<PlayerInput>().enabled = false;

            TargetClick(opponentId.connectionToClient, objId, locPos, opponentId, idLocale, locRot);
        }

        [Command]
        public void LocaleCmd(uint localeIDs)
        {
            NetworkIdentity opponentId = NetworkServer.spawned[localeIDs];

            //uint objId = objectId;
            uint idLocale = localeIDs;

            LocaleRPC(opponentId.connectionToClient, opponentId, idLocale);
        }

        [TargetRpc]
        public void TargetClick(NetworkConnectionToClient netId, uint idNetwork, Vector3 posSpawn, NetworkIdentity networkID, uint localeIDs, Quaternion locRot)
        {
            Debug.Log(this.gameObject.name + " Has clicked you !");

            networkID.gameObject.GetComponent<CharacterController>().enabled = false;
            networkID.gameObject.GetComponent<PlayerInput>().enabled = false;
            networkID.gameObject.GetComponent<ThirdPersonController>().enabled = false;

            //RequestCanvas.gameObject.SetActive(true);
            WaitCanvas.gameObject.SetActive(true);

            //varSpawn = posSpawn;
            locID = localeIDs;
        }

        [TargetRpc]
        public void LocaleRPC(NetworkConnectionToClient netCon, NetworkIdentity netID, uint localeID)
        {
            netID.gameObject.GetComponent<CharacterController>().enabled = false;
            netID.gameObject.GetComponent<PlayerInput>().enabled = false;
            netID.gameObject.GetComponent<ThirdPersonController>().enabled = false;

            locID = localeID;
        }

        [ClientRpc]
        void RpcPaint(GameObject gameObj, Color col)
        {
            gameObj.GetComponent<Renderer>().material.color = col;
        }

        [Command]
        public void CmdChangeColor(GameObject gameObj, Color color)
        {
            gameObj.GetComponent<Renderer>().material.color = color;
            RpcPaint(gameObj, color);
        }

        [Command]
        public void AgreeCmd()
        {
            Debug.Log(this.gameObject.name + " agree");
        }

        [TargetRpc]
        public void GotMessage(NetworkConnectionToClient target)
        {
            //healthCount -= 10;
            Debug.Log("You got Hello from other client");
        }

        [TargetRpc]
        public void HealthLoseClient()
        {
            Debug.Log("You lose health");
            healthCount -= 10;
        }

        [Server]
        public void ReceiveCongrats()
        {
            if (scoreCount > 20 && isWin == false)
            {
                Debug.Log("Player have win");
                isWin = true;
                CongratsFunc();
            }
        }

        [ClientRpc]
        public void RpcSphere()
        {
            isSphereActive = true;
            sphere.gameObject.SetActive(true);
        }

        [Server]
        public void ActivateSphere()
        {
            if (isSphereActive == false)
            {
                isSphereActive = true;
                Debug.Log("Sphere Active");
                sphere.gameObject.SetActive(true);
                RpcSphere();
            }
        }

        [Server]
        public void ReceiveDefeat()
        {
            if (healthCount <= 0 && isDefeat == false)
            {
                Debug.Log("One Player has defeated");
                isDefeat = true;
                TargetDefeatFunc();
            }
        }

        [ClientRpc]
        public void ReplyServerToAll()
        {
            Debug.Log("Get Hai from Server");
        }

        [TargetRpc]
        public void TargetReplyHola()
        {
            Debug.Log("Get Hola from Server");
        }

        [ClientRpc]
        public void CongratsFunc()
        {
            Debug.Log("Player side is Win");
        }

        [TargetRpc]
        public void TargetDefeatFunc()
        {
            Debug.Log("One Player is Defeated");
        }

        private void OnDisplayNameChangeUpdated(string oldName, string newName)
        {
            playerNameText.text = playerName;
            gameObject.name = playerName + " Player";
        }

        [ClientRpc]
        public void RpcClientName(string name, Color col)
        {
            playerName = name;
        }

        [ClientRpc]
        public void RpcLogNewName(string newName)
        {
            Debug.Log(newName);
        }

        [TargetRpc]
        public void GetAttacked(NetworkConnectionToClient target, int damage)
        {
            healthCount -= damage;
            Debug.Log("You got damage");
        }

        private IEnumerator animTIme(float animTime)
        {
            yield return new WaitForSeconds(animTime);

            animator.SetBool(_greetAnimID, false);
        }

        public void ShowName()
        {
            Debug.Log($"Input Field Value: {inputData.InputText}");
        }

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

        void OnScoreCountChanged(int oldCount, int newCount)
        {
            Debug.Log($"We had {oldCount} score, but now we have {newCount} ecore!");
        }

        private void AssignAnimationIDs()
        {
            _greetAnimID = Animator.StringToHash("Greeting");
        }

        [Command]
        void CmdGreetAnim(uint objectId, uint localID, int animID)
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
                //opponentId.gameObject.GetComponent<Animator>().SetBool(_greetAnimID, true);
                opponentId.gameObject.GetComponent<Animator>().CrossFadeInFixedTime(/*animationData[index].AnimState*/"Greeting" , 0.1f);

                yield return new WaitForSeconds(animPlay);

                //opponentId.gameObject.GetComponent<Animator>().SetBool(_greetAnimID, false);
                opponentId.gameObject.GetComponent<Animator>().CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);
            }
            RpcGreetAnimator(opponentId.connectionToClient, objId, opponentId, localeID, animID);
        }

        [Command]
        void CmdClapAnim(uint objectId, uint localID, int animID)
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
                //opponentId.gameObject.GetComponent<Animator>().SetBool(_greetAnimID, true);
                opponentId.gameObject.GetComponent<Animator>().CrossFadeInFixedTime("Clapping" , 0.1f);

                yield return new WaitForSeconds(animPlay);

                //opponentId.gameObject.GetComponent<Animator>().SetBool(_greetAnimID, false);
                opponentId.gameObject.GetComponent<Animator>().CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);
            }
            RpcClapAnimator(opponentId.connectionToClient, objId, opponentId, localeID, animID);
        }

        [Command]
        void CmdDanceAnim(uint objectId, uint localID, int animID)
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
                //opponentId.gameObject.GetComponent<Animator>().SetBool(_greetAnimID, true);
                opponentId.gameObject.GetComponent<Animator>().CrossFadeInFixedTime("Dance" , 0.1f);

                yield return new WaitForSeconds(animPlay);

                //opponentId.gameObject.GetComponent<Animator>().SetBool(_greetAnimID, false);
                opponentId.gameObject.GetComponent<Animator>().CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);
            }
            RpcDanceAnimator(opponentId.connectionToClient, objId, opponentId, localeID, animID);
        }

        [TargetRpc]
        void RpcLocaleGreetAnim(NetworkIdentity localeID, int animID)
        {
            Debug.Log("Locale Anim");
            //this.gameObject.transform.position = varSpawn;
            localeID.gameObject.GetComponent<Transform>().position = newVar;
            localeID.gameObject.GetComponent<Transform>().rotation = newRot;

            StartCoroutine(animTimePlay(countAnimTime));

            IEnumerator animTimePlay(float animTime)
            {
                //animator.SetBool(_greetAnimID, true);
                animator.CrossFadeInFixedTime("Greeting" , 0.1f);

                localeID.gameObject.GetComponent<CharacterController>().enabled = false;
                localeID.gameObject.GetComponent<PlayerInput>().enabled = false;
                localeID.gameObject.GetComponent<ThirdPersonController>().enabled = false;

                yield return new WaitForSeconds(animTime);
                Debug.Log("AnimStop");
                //animator.SetBool(_greetAnimID, false);
                animator.CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);

                localeID.gameObject.GetComponent<CharacterController>().enabled = true;
                localeID.gameObject.GetComponent<PlayerInput>().enabled = true;
                localeID.gameObject.GetComponent<ThirdPersonController>().enabled = true;
            }

            localeID.gameObject.GetComponent<PlayerNetworkBehaviour>().WaitCanvas.gameObject.SetActive(false);

            CmdGreetAnim(idNetwork, locID, animID);
        }

        [TargetRpc]
        void RpcLocaleClapAnime(NetworkIdentity localeID, int animID)
        {
            Debug.Log("Locale Anim");
            //this.gameObject.transform.position = varSpawn;
            localeID.gameObject.GetComponent<Transform>().position = newVar;
            localeID.gameObject.GetComponent<Transform>().rotation = newRot;

            StartCoroutine(animTimePlay(countAnimTime));

            IEnumerator animTimePlay(float animTime)
            {
                //animator.SetBool(_greetAnimID, true);
                animator.CrossFadeInFixedTime("Clapping" , 0.1f);

                localeID.gameObject.GetComponent<CharacterController>().enabled = false;
                localeID.gameObject.GetComponent<PlayerInput>().enabled = false;
                localeID.gameObject.GetComponent<ThirdPersonController>().enabled = false;

                yield return new WaitForSeconds(animTime);
                Debug.Log("AnimStop");
                //animator.SetBool(_greetAnimID, false);
                animator.CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);

                localeID.gameObject.GetComponent<CharacterController>().enabled = true;
                localeID.gameObject.GetComponent<PlayerInput>().enabled = true;
                localeID.gameObject.GetComponent<ThirdPersonController>().enabled = true;
            }

            localeID.gameObject.GetComponent<PlayerNetworkBehaviour>().WaitCanvas.gameObject.SetActive(false);
            CmdClapAnim(idNetwork, locID, animID);
        }

        [TargetRpc]
        void RpcLocaleDanceAnim(NetworkIdentity localeID, int animID)
        {
            Debug.Log("Locale Anim");
            //this.gameObject.transform.position = varSpawn;
            localeID.gameObject.GetComponent<Transform>().position = newVar;
            localeID.gameObject.GetComponent<Transform>().rotation = newRot;

            StartCoroutine(animTimePlay(countAnimTime));

            IEnumerator animTimePlay(float animTime)
            {
                //animator.SetBool(_greetAnimID, true);
                animator.CrossFadeInFixedTime("Dance" , 0.1f);

                localeID.gameObject.GetComponent<CharacterController>().enabled = false;
                localeID.gameObject.GetComponent<PlayerInput>().enabled = false;
                localeID.gameObject.GetComponent<ThirdPersonController>().enabled = false;

                yield return new WaitForSeconds(animTime);
                Debug.Log("AnimStop");
                //animator.SetBool(_greetAnimID, false);
                animator.CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);

                localeID.gameObject.GetComponent<CharacterController>().enabled = true;
                localeID.gameObject.GetComponent<PlayerInput>().enabled = true;
                localeID.gameObject.GetComponent<ThirdPersonController>().enabled = true;
            }

            localeID.gameObject.GetComponent<PlayerNetworkBehaviour>().WaitCanvas.gameObject.SetActive(false);
            CmdDanceAnim(idNetwork, locID, animID);
        }

        [TargetRpc]
        void RpcGreetAnimator(NetworkConnectionToClient netId, uint objectId, NetworkIdentity networkID, NetworkIdentity localeID, int animID)
        {
            localeID.gameObject.GetComponent<Transform>().position = newVar;
            localeID.gameObject.GetComponent<Transform>().rotation = newRot;

            networkID.gameObject.GetComponent<Transform>().rotation = targetRot;

            StartCoroutine(animTimePlay(countAnimTime));

            IEnumerator animTimePlay(float animTime)
            {
                //networkID.gameObject.GetComponent<Animator>().SetBool(_greetAnimID, true);
                networkID.gameObject.GetComponent<Animator>().CrossFadeInFixedTime("Greeting" , 0.1f);

                networkID.gameObject.GetComponent<CharacterController>().enabled = false;
                networkID.gameObject.GetComponent<PlayerInput>().enabled = false;
                networkID.gameObject.GetComponent<ThirdPersonController>().enabled = false;

                yield return new WaitForSeconds(animTime);

                Debug.Log("TestDemoAnimStop");

                //networkID.gameObject.GetComponent<Animator>().SetBool(_greetAnimID, false);
                networkID.gameObject.GetComponent<Animator>().CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);

                networkID.gameObject.GetComponent<CharacterController>().enabled = true;
                networkID.gameObject.GetComponent<PlayerInput>().enabled = true;
                networkID.gameObject.GetComponent<ThirdPersonController>().enabled = true;

            }
            //Debug.Log("Test Anim Client");
            //TestCommand();
        }

        [TargetRpc]
        void RpcClapAnimator(NetworkConnectionToClient netId, uint objectId, NetworkIdentity networkID, NetworkIdentity localeID, int animID)
        {
            localeID.gameObject.GetComponent<Transform>().position = newVar;
            localeID.gameObject.GetComponent<Transform>().rotation = newRot;

            networkID.gameObject.GetComponent<Transform>().rotation = targetRot;

            StartCoroutine(animTimePlay(countAnimTime));

            IEnumerator animTimePlay(float animTime)
            {
                //networkID.gameObject.GetComponent<Animator>().SetBool(_greetAnimID, true);
                networkID.gameObject.GetComponent<Animator>().CrossFadeInFixedTime("Clapping" , 0.1f);

                networkID.gameObject.GetComponent<CharacterController>().enabled = false;
                networkID.gameObject.GetComponent<PlayerInput>().enabled = false;
                networkID.gameObject.GetComponent<ThirdPersonController>().enabled = false;

                yield return new WaitForSeconds(animTime);

                Debug.Log("TestDemoAnimStop");

                //networkID.gameObject.GetComponent<Animator>().SetBool(_greetAnimID, false);
                networkID.gameObject.GetComponent<Animator>().CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);

                networkID.gameObject.GetComponent<CharacterController>().enabled = true;
                networkID.gameObject.GetComponent<PlayerInput>().enabled = true;
                networkID.gameObject.GetComponent<ThirdPersonController>().enabled = true;

            }
            //Debug.Log("Test Anim Client");
            //TestCommand();
        }

        [TargetRpc]
        void RpcDanceAnimator(NetworkConnectionToClient netId, uint objectId, NetworkIdentity networkID, NetworkIdentity localeID, int animID)
        {
            localeID.gameObject.GetComponent<Transform>().position = newVar;
            localeID.gameObject.GetComponent<Transform>().rotation = newRot;

            networkID.gameObject.GetComponent<Transform>().rotation = targetRot;

            StartCoroutine(animTimePlay(countAnimTime));

            IEnumerator animTimePlay(float animTime)
            {
                //networkID.gameObject.GetComponent<Animator>().SetBool(_greetAnimID, true);
                networkID.gameObject.GetComponent<Animator>().CrossFadeInFixedTime("Dance" , 0.1f);

                networkID.gameObject.GetComponent<CharacterController>().enabled = false;
                networkID.gameObject.GetComponent<PlayerInput>().enabled = false;
                networkID.gameObject.GetComponent<ThirdPersonController>().enabled = false;

                yield return new WaitForSeconds(animTime);

                Debug.Log("TestDemoAnimStop");

                //networkID.gameObject.GetComponent<Animator>().SetBool(_greetAnimID, false);
                networkID.gameObject.GetComponent<Animator>().CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);

                networkID.gameObject.GetComponent<CharacterController>().enabled = true;
                networkID.gameObject.GetComponent<PlayerInput>().enabled = true;
                networkID.gameObject.GetComponent<ThirdPersonController>().enabled = true;

            }
            //Debug.Log("Test Anim Client");
            //TestCommand();
        }

        [Command(requiresAuthority = false)]
        void TestCommand()
        {
            //yesReq = false;
            RpcTestCommand();
        }

        [TargetRpc]
        void RpcTestCommand()
        {
            Debug.Log("Test Command");
            //yesReq = false;
        }

        public void yesGreetAnswer()
        {
            Debug.Log("You are agree");

            CmdGreetAnimPlay(animationID);
            //CmdAnim(idNetwork);
        }

        public void yesClapAnswer()
        {
            Debug.Log("You are agree");
            CmdClapAnimPlay(animationID);
        }

        public void yesDanceAnswer()
        {
            Debug.Log("You are agree");
            CmdDanceAnimPlay(animationID);
        }

        public void noAnswer()
        {
            Debug.Log("No agree");

            NoPlay();
        }

        [Command(requiresAuthority = false)]
        void NoPlay()
        {
            Debug.Log("Test No");
            //uint objId = GetComponent<NetworkIdentity>().netId;
            NetworkIdentity localePlay = NetworkServer.spawned[locID];

            RpcNoPlay(localePlay.connectionToClient, localePlay);
        }

        [TargetRpc]
        void RpcNo(NetworkConnectionToClient netId, uint objectId, NetworkIdentity networkID, NetworkIdentity localeID)
        {
            localeID.gameObject.GetComponent<CharacterController>().enabled = true;
            localeID.gameObject.GetComponent<PlayerInput>().enabled = true;
            localeID.gameObject.GetComponent<ThirdPersonController>().enabled = true;

            //inputPref.gameObject.SetActive(false);
        }

        [Command(requiresAuthority = false)]
        void CmdGreetAnimPlay(int animID)
        {
            NetworkIdentity localePlay = NetworkServer.spawned[locID];

            StartCoroutine(animTimePlay(countAnimTime));

            IEnumerator animTimePlay(float animTime)
            {
                animator.CrossFadeInFixedTime("Greeting" , 0.1f);

                yield return new WaitForSeconds(animTime);

                //animator.SetBool(_greetAnimID, false);
                animator.CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);
                //opponentId.gameObject.GetComponent<Animator>().SetBool(_greetAnimID, false);
            }
            RpcLocaleGreetAnim(localePlay, animID);
        }

        [Command(requiresAuthority = false)]
        void CmdClapAnimPlay(int animID)
        {
            NetworkIdentity localePlay = NetworkServer.spawned[locID];

            StartCoroutine(animTimePlay(countAnimTime));

            IEnumerator animTimePlay(float animTime)
            {
                animator.CrossFadeInFixedTime("Clapping" , 0.1f);

                yield return new WaitForSeconds(animTime);

                //animator.SetBool(_greetAnimID, false);
                animator.CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);
                //opponentId.gameObject.GetComponent<Animator>().SetBool(_greetAnimID, false);
            }
            //RpcLocaleGreetAnim(localePlay, animID);
            RpcLocaleClapAnime(localePlay, animID);
        }

        [Command(requiresAuthority = false)]
        void CmdDanceAnimPlay(int animID)
        {
            NetworkIdentity localePlay = NetworkServer.spawned[locID];

            StartCoroutine(animTimePlay(countAnimTime));

            IEnumerator animTimePlay(float animTime)
            {
                animator.CrossFadeInFixedTime("Dance" , 0.1f);

                yield return new WaitForSeconds(animTime);

                //animator.SetBool(_greetAnimID, false);
                animator.CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);
                //opponentId.gameObject.GetComponent<Animator>().SetBool(_greetAnimID, false);
            }
            //RpcLocaleGreetAnim(localePlay, animID);
            RpcLocaleDanceAnim(localePlay, animID);
        }

        [TargetRpc]
        void RpcNoPlay(NetworkConnectionToClient netConID, NetworkIdentity localeID)
        {
            Debug.Log("Locale opt");

            localeID.gameObject.GetComponent<CharacterController>().enabled = true;
            localeID.gameObject.GetComponent<PlayerInput>().enabled = true;
            localeID.gameObject.GetComponent<ThirdPersonController>().enabled = true;
            localeID.gameObject.GetComponent<PlayerNetworkBehaviour>().WaitCanvas.gameObject.SetActive(false);

            //yesReq = true;
            //CmdAnim(idNetwork);
            CmdSetUpNo(idNetwork);
        }

        [Command]
        void CmdSetUpNo(uint objectIdentity)
        {
            Debug.Log("Test Anim");
            uint objId = GetComponent<NetworkIdentity>().netId;

            objId = objectIdentity;

            NetworkIdentity opponentId = NetworkServer.spawned[objectIdentity];

            RpcNoPlayClient(opponentId.connectionToClient, objId, opponentId);
        }

        void ButtonActivate()
        {
            isButtonActive = true;
        }

        void ButtonDeactivated()
        {
            //Destroy(GameObject.FindGameObjectWithTag("ButtonInst"));
            GameObject[] btnTarget = GameObject.FindGameObjectsWithTag("ButtonInst");
            foreach (GameObject btnDestroyed in btnTarget)
            GameObject.Destroy(btnDestroyed);
        }

        public void GreetButton()
        {
            Debug.Log("Test Greet");
            //animator.CrossFadeInFixedTime(animationData[animIndex].AnimState, 0.1f);
            animID = 1;
            ButtonDeactivated();
            CmdLocaleAnimPanel(localID);
            CmdGreetPanel(idNet, animID);
        }

        public void ClapButton()
        {
            Debug.Log("Test Clap");
            //animator.CrossFadeInFixedTime(animationData[animIndex].AnimState, 0.1f);
            animID = 2;
            ButtonDeactivated();
            CmdLocaleAnimPanel(localID);
            CmdGreetPanel(idNet, animID);
        }

        public void DanceBUtton()
        {
            Debug.Log("Test Dance");
            //animator.CrossFadeInFixedTime(animationData[animIndex].AnimState, 0.1f);
            animID = 3;
            ButtonDeactivated();
            CmdLocaleAnimPanel(localID);
            CmdGreetPanel(idNet, animID);
        }

        [Command]
        void CmdGreetPanel(uint idNet, int ID)
        {
            NetworkIdentity otherClientID = NetworkServer.spawned[idNet];

            RpcGreetPanel(otherClientID.connectionToClient, otherClientID, ID);
        }

        [TargetRpc]
        void RpcGreetPanel(NetworkConnectionToClient otherID, NetworkIdentity otherClientID, int ID)
        {
            if (ID == 1)
            {
                WaitCanvas.gameObject.SetActive(false);
                RequestCanvas.gameObject.SetActive(true);
            }

            if (ID == 2)
            {
                WaitCanvas.gameObject.SetActive(false);
                ClapRequestCanvas.gameObject.SetActive(true);
            }

            if (ID == 3)
            {
                WaitCanvas.gameObject.SetActive(false);
                DanceRequestCanvas.gameObject.SetActive(true);
            }
        }

        [Command]
        void CmdLocaleAnimPanel(uint localeID)
        {
            NetworkIdentity localePlayer = NetworkServer.spawned[localeID];

            RpcLocaleAnimPanel(localePlayer.connectionToClient, localePlayer);
        }

        [TargetRpc]
        void RpcLocaleAnimPanel(NetworkConnectionToClient netCon, NetworkIdentity netID)
        {
            netID.gameObject.GetComponent<PlayerNetworkBehaviour>().AnimationCanvas.gameObject.SetActive(false);
            netID.gameObject.GetComponent<PlayerNetworkBehaviour>().WaitCanvas.gameObject.SetActive(true);
        }

        [Command]
        public void CmdSelf(uint localID)
        {
            NetworkIdentity localeID = NetworkServer.spawned[localID];

            uint idLocale = localID;

            RpcSelf(localeID.connectionToClient, localeID, idLocale);
        }

        [TargetRpc]
        void RpcSelf(NetworkConnectionToClient networkID, NetworkIdentity netID, uint localeID)
        {
            netID.gameObject.GetComponent<PlayerNetworkBehaviour>().AnimationCanvas.gameObject.SetActive(true);
            
            netID.gameObject.GetComponent<PlayerNetworkBehaviour>().animManager.SpawnButton();
        }

        [TargetRpc]
        void RpcNoPlayClient(NetworkConnectionToClient netId, uint objectId, NetworkIdentity networkID)
        {
            //Debug.Log("Test Anim Client");
            networkID.gameObject.GetComponent<CharacterController>().enabled = true;
            networkID.gameObject.GetComponent<PlayerInput>().enabled = true;
            networkID.gameObject.GetComponent<ThirdPersonController>().enabled = true;
            networkID.gameObject.GetComponent<PlayerNetworkBehaviour>().WaitCanvas.gameObject.SetActive(false);

            Debug.Log("Test Anim Client ");
        }

        private void FixedUpdate()
        {
            mask = LayerMask.GetMask("Player");
        }

        void OnChangeID(uint oldID, uint newID)
        {
            idNetwork = newID;
        }

        public void SelfieButton()
        {
            if (isLocalPlayer)
            {
                Debug.Log("Is Local Player");

                localeSelfieID = this.gameObject.GetComponent<NetworkIdentity>().netId;
                otherClients = this.gameObject.GetComponent<NetworkIdentity>().netId;
                CmdSelfieLocalePanel(localeSelfieID);
                //localIDchange = true;
                CmdAllCLientAccess(otherClients);
                //CmdSelfiePos(localeSelfieID);
            }
        }

        [Command]
        void CmdAllCLientAccess(uint allClientAcc)
        {
            gameObject.GetComponent<PlayerNetworkBehaviour>().clientSelfID = allClientAcc;

            RpcAllClientAccess(clientSelfID, clientSelfID);
        }

        [ClientRpc]
        private void RpcAllClientAccess(uint oldID, uint clientID)
        {
            //clientSelfID = allNetID.gameObject.GetComponent<PlayerNetworkBehaviour>().otherClients;
            gameObject.GetComponent<PlayerNetworkBehaviour>().othersPosID = clientID;
        }
        
        [Command(requiresAuthority = false)]
        void CmdSelfiePos(uint localID)
        {
            forOtherClientSelfieID = localID;

            RpcSelfiePos(forOtherClientSelfieID);
        }

        [ClientRpc]
        void RpcSelfiePos(uint ID)
        {
            forOtherClientSelfieID = ID;
        }

        [Command]
        void CmdSelfieLocalePanel(uint localNetID)
        {
            NetworkIdentity localeNetID = NetworkServer.spawned[localNetID];

            //RpcSelfieLocal(localeNetID.connectionToClient, localeNetID);
            RpcSelfieLocal(localeNetID.connectionToClient, localeNetID);
        }
        
        [Command]
        void CmdSelfiePanelOther(NetworkIdentity localId)
        {
            RpcPanelSelfie(localId);
        }

        [TargetRpc]
        void RpcSelfieLocal(NetworkConnectionToClient netConID, NetworkIdentity localNetID)
        {
            localNetID.gameObject.GetComponent<PlayerNetworkBehaviour>().SelfieCanvas.gameObject.SetActive(true);
            //SelfieCanvas.gameObject.SetActive(true);
            joinButtonCanvas.gameObject.SetActive(false);
            CmdSelfiePanelOther(localNetID);
        }

        [TargetRpc]
        void RpcOtherPanel(NetworkConnectionToClient netConID, NetworkIdentity netID, NetworkIdentity localeID)
        {
            localeID.gameObject.GetComponent<PlayerNetworkBehaviour>().SelfieCanvas.gameObject.SetActive(true);
        }

        [ClientRpc]
        void RpcPanelSelfie(NetworkIdentity localeID)
        {
            localeID.gameObject.GetComponent<PlayerNetworkBehaviour>().SelfieCanvas.gameObject.SetActive(true);
            if (isLocalPlayer)
            {
                joinButtonCanvas.gameObject.SetActive(false);
            }
        }

        public void JoinSelfie()
        {
            SelfieReqPanel.gameObject.SetActive(true);
            testID = gameObject.GetComponent<NetworkIdentity>().netId;

            Debug.Log("Selfie Test");
            //CmdSelfieReqPanel(startID);
        }

        [Command(requiresAuthority = false)]
        void CmdSelfieReqPanel(uint otherID)
        {
            NetworkIdentity otherClientID = NetworkServer.spawned[otherID];

            RpcSelfieReqPanel(otherClientID.connectionToClient, otherClientID);
        }

        [TargetRpc]
        void RpcSelfieReqPanel(NetworkConnectionToClient netConID, NetworkIdentity netOtherID)
        {
            netOtherID.gameObject.GetComponent<PlayerNetworkBehaviour>().SelfieReqPanel.gameObject.SetActive(true);
        }

        public void YesJoin()
        {
            SelfieReqPanel.gameObject.SetActive(false);
            otherClientIDs = this.gameObject.GetComponent<NetworkIdentity>().netId;
            CmdYesjoin(otherClientIDs, othersPosID);
        }

        [Command(requiresAuthority = false)]
        void CmdYesjoin(uint corePosID, uint centerPosID)
        {
            Debug.Log("Tes Connect");

            NetworkIdentity localID = NetworkServer.spawned[corePosID];
            
            NetworkIdentity posID = NetworkServer.spawned[centerPosID];
            Debug.Log("Test Local");

            localID.gameObject.GetComponent<Transform>().position = posID.gameObject.GetComponent<PlayerNetworkBehaviour>().pointNewPos;
            //Vector3 newPos = new Vector3(localID.gameObject.GetComponent<PlayerNetworkBehaviour>().)
            RpcYesJoin(localID.connectionToClient, localID, posID);
        }

        [TargetRpc]
        void RpcYesJoin(NetworkConnectionToClient netConID, NetworkIdentity netID, NetworkIdentity localPosID)
        {
            //Vector3 currentPos = gameObject.GetComponent<Transform>().position;
            netID.gameObject.GetComponent<Transform>().position = localPosID.gameObject.GetComponent<PlayerNetworkBehaviour>().pointNewPos;
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 100f;

            newSelfiePos = gameObject.GetComponent<PlayerNetworkBehaviour>().selfiePos1.gameObject.transform.position;
            //newSelfirPos2 = GetComponent<PlayerNetworkBehaviour>().selfiePos2.transform.position;
            pointNewPos = new Vector3(newSelfiePos.x, newSelfiePos.y, newSelfiePos.z);

            _hasAnimator = TryGetComponent(out animator);
            idNetwork = idNet;
            animationID = animID;
            
            if (isLocalPlayer)
            {
                startID = this.gameObject.GetComponent<NetworkIdentity>().netId;
            }

            if (isButtonActive == true && inputButton == null)
            {
                inputButton = GameObject.FindGameObjectWithTag("InputButton").GetComponent<Button>();

                inputButton.onClick.AddListener(() => InputName());
                isButtonActive = false;
            }

            requestPanel = GameObject.FindGameObjectWithTag("Request");

            playName = inputData.InputText;

            if (isLocalPlayer && Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                //target = GameObject.FindGameObjectWithTag("Player");
                if (Physics.Raycast(ray, out hit, 100, mask))
                {
                    //requestPanel.gameObject.SetActive(true);
                    //Debug.Log("Testing");
                    objectID = GameObject.Find(hit.transform.gameObject.name);
                    
                    idNet = objectID.GetComponent<NetworkIdentity>().netId;
                    localID = this.gameObject.GetComponent<NetworkIdentity>().netId;
                    //transLoc = objectID.GetComponent<Transform>();

                    SpawnVar = objectID.gameObject.GetComponent<Transform>().position;
                    locRot = objectID.gameObject.GetComponent<Transform>().rotation;

                    //SpawnVar = new Vector3(newVar.x, newVar.y, newVar.z);
                    newVar = new Vector3(SpawnVar.x, SpawnVar.y, SpawnVar.z + 0.75f);
                    newRot = new Quaternion(locRot.x, locRot.y + 180f, locRot.z, locRot.w);

                    OnChangeID(idNetwork, idNet);
                    CmdSelf(localID);
                    CmdClick(idNet, newVar, localID, newRot);
                    LocaleCmd(localID);
                }
            }

            if (isServer && Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                //target = GameObject.FindGameObjectWithTag("Player");
                if (Physics.Raycast(ray, out hit, 100, mask))
                {
                    //Debug.Log("Testing");
                    objectID = GameObject.Find(hit.transform.name);                                     
                    //CmdClick(objectID);
                    StopClient(objectID);
                }
            }
            /*
            if (isLocalPlayer && Input.GetKeyDown(KeyCode.T))
            {
                netID = networkId;
            }

            if (isLocalPlayer && Input.GetKeyDown(KeyCode.Q))
            {
                TestConnectCmd();
            }

            if (isLocalPlayer && Input.GetKeyDown(KeyCode.E))
            {
                TestConnectAllCmd();
            }

            if (isLocalPlayer && Input.GetKeyDown(KeyCode.Z))
            {
                GetHelloCmd();
            }

            if (isLocalPlayer && Input.GetKeyDown(KeyCode.X))
            {
                GetScore();
            }

            if (isLocalPlayer && Input.GetKeyDown(KeyCode.V))
            {
                animator.SetBool("Greeting", true);
            }

            if (isLocalPlayer && Input.GetKeyDown(KeyCode.F))
            {
                LoseHealthCommand();
            }

            if (isLocalPlayer && Input.GetKeyDown(KeyCode.H))
            {
                ChatCmd(networkId);
            }
            */
            if (isLocalPlayer && Input.GetKeyDown(KeyCode.K))
            {
                AnimationCanvas.gameObject.SetActive(true);
                this.gameObject.GetComponent<PlayerNetworkBehaviour>().animManager.SpawnButton();
            }

            if (isServer)
            {
                ReceiveCongrats();
                ReceiveDefeat();
            }

            if (isServer && Input.GetKeyDown(KeyCode.M))
            {
                ActivateSphere();
            }

            if (isLocalPlayer && Input.GetKeyDown(KeyCode.Y))
            {
                CmdSpawnItem();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                if (isLocalPlayer)
                {
                    inputName = inputData.InputText;

                    playName = inputName;

                    Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

                    CmdSetupPlayer(playName, color);
                }
            }
            /*
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (isLocalPlayer)
                {
                    playName = inputName;
                    Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

                    CmdSetupPlayer(playName, color);
                }
            }
            */
            floatingInfo.transform.LookAt(Camera.main.transform);
            SelfieCanvas.transform.LookAt(Camera.main.transform);
        }
    }
}


