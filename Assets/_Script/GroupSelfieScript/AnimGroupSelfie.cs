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
using Unity.VisualScripting;
using razz;

namespace StarterAssets
{
    public class AnimGroupSelfie : NetworkBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            CharacterController charControls = GetComponent<CharacterController>();
            PlayerInput playInput = GetComponent<PlayerInput>();
            ThirdPersonController TPControl = GetComponent<ThirdPersonController>();
            PlayerController playerConn = GetComponent<PlayerController>();

            charControls.enabled = true;
            playInput.enabled = true;
            TPControl.enabled = true;
            playerConn.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
