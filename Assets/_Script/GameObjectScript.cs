using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    public class GameObjectScript : NetworkBehaviour
    {
        public Button inputButton;
        public Button ToggleButton;

        [SerializeField]
        public InputHandler inputData;

        [SerializeField]
        public GameObject inputPrefab;

        public GameObject ExitButton;
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

        public GameObject floatingInfo;
    }
}
