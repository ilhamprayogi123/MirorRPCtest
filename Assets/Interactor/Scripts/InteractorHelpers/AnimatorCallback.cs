using UnityEngine;

namespace razz
{
    public class AnimatorCallback : MonoBehaviour
    {//If the player has Animator on a different gameobject other than InteractorIK,
	 //this will enable to call IK calculations when needed (on AnimatorIK pass).
     //Because OnAnimatorIK state only works in correct time when called by Animator from same object.
        public InteractorIK interactorIk;

		private void OnAnimatorIK(int layerIndex)
		{
            if (!interactorIk)
            {
                Debug.LogWarning("There is no InteractorIK on AnimatorCallback!", this);
                return;
            }
			interactorIk.OnAnimatorIK(layerIndex);
		}
	}
}
