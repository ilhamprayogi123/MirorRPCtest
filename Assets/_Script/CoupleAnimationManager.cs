using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    // This script is used to list which scriptable objects will appear and be used in the game scene.
    public class CoupleAnimationManager : MonoBehaviour
    {
        [SerializeField]
        public List<CoupleAnimationData> animationData;
        int count;
        private string stateName;

        public void SpawnButton()
        {
            for (int i = 0; i < animationData.Count; i++)
            {
                Instantiate(animationData[i].button);
            }
        }
    }
}
