using UnityEngine;

namespace razz
{
    public class DoorControl : MonoBehaviour
    {
        public Animator doorAnimator;
        public string animatorSpeedVariable;

        public void OpenDoor()
        {
            doorAnimator.SetBool("open", true);
        }

        public void PushDoor()
        {
            doorAnimator.SetBool("push", true);
        }

        //Called by animation event (Door animation in this case)
        public void SetSpeed(float newSpeedMult)
        {
            doorAnimator.SetFloat(animatorSpeedVariable, newSpeedMult);
        }
    }
}
