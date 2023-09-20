using UnityEngine;

namespace razz
{
    //This needs to be on same object with Animator. So animation events can call its methods.
    public class ResetAttack : MonoBehaviour
    {
        public FirstPersonController _firstPersonController;

        public void ResetKnifeAttack()
        {
            if (_firstPersonController)
            {
                _firstPersonController.ResetKnifeAttack();
            }
        }
    }
}
