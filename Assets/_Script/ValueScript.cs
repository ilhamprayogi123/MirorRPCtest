using Mirror;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ValueScript : NetworkBehaviour
{
    [SerializeField]
    private PlayerNetworkBehaviour playerNet;

    [SyncVar]
    public int animID;
    [SyncVar]
    public int intIndexAnim;
    [SyncVar]
    public int varIndex;

    [SyncVar]
    public int maxIndex;
    [SyncVar]
    public int indexNum;

    public TMP_Text countText;

    [SyncVar]
    public uint localID;
    [SyncVar]
    public uint locID;
    [SyncVar]
    public uint localeSelfieID;
    [SyncVar]
    public uint otherClientIDs;
    [SyncVar]
    public uint testClientID;
    [SyncVar]
    public uint idNet;
    [SyncVar]
    public uint objId;
    [SyncVar]
    public uint testID;

    public void IncreaseCount()
    {
        playerNet.indexNum++;
        countText.SetText(playerNet.indexNum.ToString());
        playerNet.maxIndex = playerNet.indexNum;
    }

    public void DecreaseCount()
    {
        playerNet.indexNum--;
        if (playerNet.indexNum <= 0)
        {
            playerNet.indexNum = 0;
        }
        countText.SetText(playerNet.indexNum.ToString());
        playerNet.maxIndex = playerNet.indexNum;
    }
}
