using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    // This script is used to store gameobjects used in Group Selfie.
    public class GroupSelfieGameObj : NetworkBehaviour
    {
        public GameObject ExitButton;
        
        public GameObject SelfieReqPanel;
        public GameObject ExitRequestPanel;
        //[SyncVar]
        public GameObject SelfieCanvas;
        //[SyncVar]
        public GameObject joinButtonCanvas;
        public GameObject buttonSelfieCanvas;
        public GameObject closeSelfieCanvas;
        public GameObject raiseStandButton;
        public GameObject lowerStandButton;

        public TMP_Text currentText;
        public TMP_Text maxText;
    }
}
