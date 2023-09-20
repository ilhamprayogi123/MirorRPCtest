using UnityEngine;

namespace razz
{
    public class BasicBot : MonoBehaviour
    {
        public Interactor interactor;
        public InteractorAi interactorAi;
        public float speed = 2f;

        private float _yaw = 0f;

        private void Update()
        {
            if (interactorAi.GetForward())
            {
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
                _yaw = interactorAi.GetYaw();
            }
            if (_yaw != 0)
            {
                transform.Rotate(Vector3.up, _yaw * Time.deltaTime);
                _yaw = 0;
            }
        }

        public void StartInteractions()
        {
            if (interactor && interactorAi && interactorAi.enabled && interactor.enabled && interactor.gameObject.activeInHierarchy)
            {
                interactor.StartStopInteractions(false);
            }
        }
    }
}
