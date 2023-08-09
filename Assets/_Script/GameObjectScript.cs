using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    // This script is used to store game objects for UI parts when using the Couple Animation feature
    public class GameObjectScript : NetworkBehaviour
    {
        public Button inputButton;
        public Button ToggleButton;

        [SerializeField]
        public InputHandler inputData;

        [SerializeField]
        public GameObject inputPrefab;

        public GameObject RequestCanvas;

        public GameObject WaitCanvas;
        public GameObject AnimationCanvas;
        public GameObject NoAnimCanvas;

        public GameObject floatingInfo;
    }
}
