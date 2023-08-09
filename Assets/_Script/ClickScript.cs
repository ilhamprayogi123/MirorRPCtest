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
using UnityEditor.Experimental.GraphView;

namespace StarterAssets
{
    // This script is used for other player click mechanics for the Couple Animation feature.
    public class ClickScript : NetworkBehaviour
    {
        [SerializeField]
        private PlayerNetworkBehaviour playerNet;
        [SerializeField]
        private ValueScript valueScript;
        [SerializeField]
        private PosRotScript posRot;
        [SerializeField]
        private AnimScript animScript;

        private void Update()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 100f;

            if (isLocalPlayer && Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100, playerNet.mask))
                {
                    if (!hit.transform.gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
                    {
                        //objectID = GameObject.Find(hit.transform.gameObject.name);
                        playerNet.objectID = GameObject.Find(hit.transform.gameObject.name);
                        //valueScript.idNet = objectID.GetComponent<NetworkIdentity>().netId;
                        valueScript.idNet = playerNet.objectID.GetComponent<NetworkIdentity>().netId;
                        valueScript.localID = this.gameObject.GetComponent<NetworkIdentity>().netId;

                        //posRot.SpawnVar = objectID.gameObject.GetComponent<Transform>().position;
                        posRot.SpawnVar = playerNet.objectID.gameObject.GetComponent<Transform>().position;
                        //posRot.locRot = objectID.gameObject.GetComponent<Transform>().rotation;
                        posRot.locRot = playerNet.objectID.gameObject.GetComponent<Transform>().rotation;
                        posRot.newVar = new Vector3(posRot.SpawnVar.x, posRot.SpawnVar.y, posRot.SpawnVar.z + 0.75f);
                        posRot.newRot = new Quaternion(posRot.locRot.x, posRot.locRot.y + 180f, posRot.locRot.z, posRot.locRot.w);
                        playerNet.OnChangeID(playerNet.idNetwork, valueScript.idNet);
                        animScript.CmdSelf(valueScript.localID);
                        animScript.CmdClick(valueScript.idNet, posRot.newVar, valueScript.localID, posRot.newRot);
                        //LocaleCmd(localID);
                    }
                }
            }
            
        }
    }
}


