using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    // This script is used to store position and rotation data from the client that will be used in the project.
    public class PosRotScript : NetworkBehaviour
    {
        [SyncVar]
        public Vector3 newVar;
        [SyncVar]
        public Vector3 SpawnVar;
        [SyncVar]
        public Quaternion newRot;
        [SyncVar]
        public Quaternion locRot;
        [SyncVar]
        public Quaternion targetRot;
        [SyncVar]
        public Vector3 varHeight;
        [SyncVar]
        public Vector3 newVarHeight;
    }
}

