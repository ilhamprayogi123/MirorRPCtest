using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking.Types;

namespace StarterAssets
{
    public class CharcControl : NetworkBehaviour
    {
        public void ControlStop()
        {
            this.gameObject.GetComponent<CharacterController>().enabled = false;
            this.gameObject.GetComponent<PlayerInput>().enabled = false;
            this.gameObject.GetComponent<ThirdPersonController>().enabled = false;
        }

        public void ControlON()
        {
            this.gameObject.GetComponent<CharacterController>().enabled = true;
            this.gameObject.GetComponent<PlayerInput>().enabled = true;
            this.gameObject.GetComponent<ThirdPersonController>().enabled = true;
        }
    }
}
