using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
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
    }
}

