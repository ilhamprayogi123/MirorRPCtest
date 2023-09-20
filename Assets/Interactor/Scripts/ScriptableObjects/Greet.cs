using UnityEngine;

namespace razz
{
    [CreateAssetMenu(fileName = "GreetSettings", menuName = "Interactor/GreetSettings")]
    public class Greet : InteractionTypeSettings
    {
        private Vector3 _positionOffset;

        public void SetRotation(InteractorTarget interactorTarget, Vector3 effectorPosition)
        {
            Vector3 resetRot = new Vector3(effectorPosition.x, effectorPosition.y, effectorPosition.z);

            interactorTarget.transform.rotation = Quaternion.LookRotation(interactorTarget.transform.position - resetRot, Vector3.up);
        }

        public void Reset()
        {
            Release();
        }

        private void Release()
        {
            //levitated = false;
            //pulled = false;
            _positionOffset = Vector3.zero;
        }

        private void OnDisable()
        {
            Release();
        }
    }
}
