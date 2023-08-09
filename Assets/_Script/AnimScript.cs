using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace StarterAssets
{
    // This script is the main script used to use the Couple Animation feature, which is to start the animation and move the position and rotation of the client that is currently related.
    public class AnimScript : NetworkBehaviour
    {
        [SerializeField]
        private PlayerNetworkBehaviour playerNet;
        [SerializeField]
        private ValueScript valueScript;
        [SerializeField]
        private UiCanvas uiCanvasObj;
        [SerializeField]
        private PosRotScript posRot;
        [SerializeField]
        private GameObjectScript gameObjectScript;

        // Call Command Function to play Couple Animation
        public void YesAnswer()
        {
            CmDAnimPlay(valueScript.varIndex);
        }

        // Command Function to call the animation using the data that was successfully obtained from the animation manager for locale player in server side 
        [Command(requiresAuthority = false)]
        void CmDAnimPlay(int animIndex)
        {
            NetworkIdentity localePlay = NetworkServer.spawned[valueScript.locID];
            StartCoroutine(animTimePlay(playerNet.countAnimTime));

            IEnumerator animTimePlay(float animTime)
            {
                playerNet.animator.CrossFadeInFixedTime(playerNet.animDatas[animIndex].AnimState, 0.1f);
                yield return new WaitForSeconds(animTime);
                playerNet.animator.CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);
            }
            RpclocalAnimPlay(localePlay, animIndex);
        }

        // Function to call the animation using the data that was successfully obtained from the animation manager in local client side, also change locale player position and rotation
        [TargetRpc]
        void RpclocalAnimPlay(NetworkIdentity localeID, int animIndex)
        {
            localeID.gameObject.GetComponent<Transform>().position = posRot.newVar;
            localeID.gameObject.GetComponent<Transform>().rotation = posRot.newRot;

            StartCoroutine(animTimePlay(playerNet.countAnimTime));

            IEnumerator animTimePlay(float animTime)
            {
                playerNet.animator.CrossFadeInFixedTime(playerNet.animDatas[animIndex].AnimState, 0.1f);
                localeID.gameObject.GetComponent<CharcControl>().ControlStop();
                yield return new WaitForSeconds(animTime);
                playerNet.animator.CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);
                localeID.gameObject.GetComponent<CharcControl>().ControlON();
            }
            //localeID.gameObject.GetComponent<UiCanvas>().WaitCanvas.gameObject.SetActive(false);
            localeID.gameObject.GetComponent<GameObjectScript>().WaitCanvas.gameObject.SetActive(false);
            CmdAnimation(playerNet.idNetwork, valueScript.locID, animIndex);
        }

        // Command Function to call the animation using the data that was successfully obtained from the animation manager and also change position locale client position in server side
        [Command]
        void CmdAnimation(uint objectId, uint localID, int animIndex)
        {
            NetworkIdentity opponentId = NetworkServer.spawned[objectId];
            NetworkIdentity localeID = NetworkServer.spawned[localID];

            localeID.gameObject.GetComponent<Transform>().position = posRot.newVar;
            localeID.gameObject.GetComponent<Transform>().rotation = posRot.newRot;
            opponentId.gameObject.GetComponent<Transform>().rotation = posRot.targetRot;

            StartCoroutine(animTimePlay(playerNet.countAnimTime));

            IEnumerator animTimePlay(float animPlay)
            {
                opponentId.gameObject.GetComponent<Animator>().CrossFadeInFixedTime(playerNet.animDatas[animIndex].AnimState, 0.1f);
                yield return new WaitForSeconds(animPlay);
                opponentId.gameObject.GetComponent<Animator>().CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);
            }
            RpcAnimator(opponentId.connectionToClient, objectId, opponentId, localeID, animIndex);
        }

        // Function to call animation for other client, also change to rotation of other client game object
        [TargetRpc]
        void RpcAnimator(NetworkConnectionToClient netId, uint objectId, NetworkIdentity networkID, NetworkIdentity localeID, int animindex)
        {
            localeID.gameObject.GetComponent<Transform>().position = posRot.newVar;
            localeID.gameObject.GetComponent<Transform>().rotation = posRot.newRot;
            networkID.gameObject.GetComponent<Transform>().rotation = posRot.targetRot;

            StartCoroutine(animTimePlay(playerNet.countAnimTime));

            IEnumerator animTimePlay(float animTime)
            {
                networkID.gameObject.GetComponent<Animator>().CrossFadeInFixedTime(playerNet.animDatas[animindex].AnimState, 0.1f);
                networkID.gameObject.GetComponent<CharcControl>().ControlStop();
                yield return new WaitForSeconds(animTime);
                networkID.gameObject.GetComponent<Animator>().CrossFadeInFixedTime("Idle Walk Run Blend", 0.1f);
                networkID.gameObject.GetComponent<CharcControl>().ControlON();
            }
        }

        // Call Command Function if client don't want ro play Couple Animation
        public void noAnswer()
        {
            NoPlay();
        }

        // Command Function to call Rpc function for cancel the request
        [Command(requiresAuthority = false)]
        void NoPlay()
        {
            NetworkIdentity localePlay = NetworkServer.spawned[valueScript.locID];
            RpcNoPlay(localePlay.connectionToClient, localePlay);
        }

        // Rpc Function to enable local client control and close wait canvas in locale client
        [TargetRpc]
        void RpcNoPlay(NetworkConnectionToClient netConID, NetworkIdentity localeID)
        {
            localeID.gameObject.GetComponent<CharcControl>().ControlON();
            localeID.gameObject.GetComponent<GameObjectScript>().WaitCanvas.gameObject.SetActive(false);
            CmdSetUpNo(playerNet.idNetwork);
        }

        // Command function to call RPC Function to enable other player control
        [Command]
        void CmdSetUpNo(uint objectIdentity)
        {
            uint objId = GetComponent<NetworkIdentity>().netId;
            objId = objectIdentity;
            NetworkIdentity opponentId = NetworkServer.spawned[objectIdentity];
            RpcNoPlayClient(opponentId.connectionToClient, objId, opponentId);
        }

        // Enable client control for other client
        [TargetRpc]
        void RpcNoPlayClient(NetworkConnectionToClient netId, uint objectId, NetworkIdentity networkID)
        {
            networkID.gameObject.GetComponent<CharcControl>().ControlON();
        }

        // Command to give data to server about client interact with other client
        [Command]
        public void CmdClick(uint objectId, Vector3 locPos, uint localID, Quaternion locRot)
        {
            NetworkIdentity opponentId = NetworkServer.spawned[objectId];
            valueScript.objId = objectId;
            uint idLocale = localID;
            Debug.Log(this.gameObject.name + " is clicking " + opponentId.gameObject.name);
            TargetClick(opponentId.connectionToClient, valueScript.objId, locPos, opponentId, idLocale, locRot);
        }

        // First interact for client and make the player who got clicked can't move
        [TargetRpc]
        public void TargetClick(NetworkConnectionToClient netId, uint idNetwork, Vector3 posSpawn, NetworkIdentity networkID, uint localeIDs, Quaternion locRot)
        {
            Debug.Log(this.gameObject.name + " Has clicked you !");
            networkID.gameObject.GetComponent<CharcControl>().ControlStop();

            gameObjectScript.WaitCanvas.gameObject.SetActive(true);
            valueScript.locID = localeIDs;
        }

        // Command function to call RPC function for disable control and open the animation canvas in local player
        [Command]
        public void CmdSelf(uint localID)
        {
            NetworkIdentity localeID = NetworkServer.spawned[localID];
            uint idLocale = localID;
            RpcSelf(localeID.connectionToClient, localeID, idLocale);
        }

        // Open animation canvas and disable the local player control
        [TargetRpc]
        void RpcSelf(NetworkConnectionToClient networkID, NetworkIdentity netID, uint localeID)
        {
            //netID.gameObject.GetComponent<UiCanvas>().AnimationCanvas.gameObject.SetActive(true);
            netID.gameObject.GetComponent<GameObjectScript>().AnimationCanvas.gameObject.SetActive(true);
            netID.gameObject.GetComponent<CharcControl>().ControlStop();
            netID.gameObject.GetComponent<PlayerNetworkBehaviour>().animManager.SpawnButton();
            valueScript.locID = localeID;
        }
    }
}
