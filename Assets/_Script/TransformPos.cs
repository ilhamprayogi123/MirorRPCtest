using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TransformPos : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NetworkIdentity netId = GetComponentInParent<NetworkIdentity>();
    }

}
