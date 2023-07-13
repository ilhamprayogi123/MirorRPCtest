using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    public class CoupleAnimationManager : MonoBehaviour
    {
        [SerializeField]
        List<CoupleAnimationData> animationData;
        int count;
        private string stateName;

        //public Animator animator;

        public void SpawnButton()
        {
            for (int i = 0; i < animationData.Count; i++)
            {
                Instantiate(animationData[i].button);
            }
        }

        /*
        public void GreetingAnim()
        {
            animator.CrossFadeInFixedTime("Greeting", 0.1f);
        }

        public void ClapAnim()
        {
            animator.CrossFadeInFixedTime("Clapping", 0.1f);
        }

        public void DanceAnim()
        {
            animator.CrossFadeInFixedTime("Dance", 0.1f);
        }
        */
    }
}
