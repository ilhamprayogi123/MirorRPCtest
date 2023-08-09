using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking.Types;

namespace StarterAssets
{
    // This script is used to enable and disable player movement and player input.
    public class CharcControl : NetworkBehaviour
    {
        // Disable player control
        public void ControlStop()
        {
            this.gameObject.GetComponent<CharacterController>().enabled = false;
            this.gameObject.GetComponent<PlayerInput>().enabled = false;
            this.gameObject.GetComponent<ThirdPersonController>().enabled = false;
        }

        // Enable player control
        public void ControlON()
        {
            this.gameObject.GetComponent<CharacterController>().enabled = true;
            this.gameObject.GetComponent<PlayerInput>().enabled = true;
            this.gameObject.GetComponent<ThirdPersonController>().enabled = true;
        }
    }
}
