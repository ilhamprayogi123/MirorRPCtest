using UnityEngine;
using UnityEngine.Events;

namespace razz
{
    //Deals with rotators' rotation, sounds and values
    [HelpURL("https://negengames.com/interactor/components.html#interactiverotatorcs")]
    [DisallowMultipleComponent]
    public class InteractiveRotator : MonoBehaviour
    {
        private float _mouseY, _prevRotation, _decimal, _angleY;
        private bool _activateOnce = true;
        public enum Direction { X, Y, Z };

        [HideInInspector]
        public bool active = false;

        [SerializeField, Tooltip("Rotation Direction")]
        public Direction direction = Direction.Y;
        [SerializeField, Tooltip("Current rotation")]
        public float currentRotation;
        [SerializeField, Tooltip("Minimum rotation")]
        public float minRotation;
        [SerializeField, Tooltip("Maximum rotation")]
        public float maxRotation;
        [SerializeField, Tooltip("Reverse input?")]
        public bool reverseRotation;
        [SerializeField, Tooltip("How many decimals will be in the rotation"), Range(0, 5)]
        public int rotationDecimals = 1;
        [SerializeField, Tooltip("How fast will be rotated as value"), Range(0.01f, 360.00f)]
        public float rotationMultiply = 0.1f;
        [SerializeField, Tooltip("How fast model will be rotated"), Range(0.01f, 360.00f)]
        public float rotationModelMultiply = 36;
        [SerializeField, Tooltip("GameObject that will be rotated")]
        public GameObject rotationModel;
        [SerializeField, Tooltip("What AudioSource will be played when rotating")]
        public AudioSource rotationAudioSource;
        [Space(10)]
        [SerializeField, Tooltip("Call event once only on max rotation")]
        public bool activateOnce;
        public UnityEventFloat onRotation = new UnityEventFloat();

        private void Update()
        {
            if (!active) return;

            if (!reverseRotation)
            {
                _mouseY = BasicInput.GetMouseY();
            }
            else
            {
                _mouseY = -BasicInput.GetMouseY();
            }
            Rotate();
        }

        public void Rotate()
        {
            _prevRotation = currentRotation;
            _decimal = Mathf.Pow(10, (float)rotationDecimals);
            _angleY = Mathf.Round(Mathf.Clamp(currentRotation + _mouseY * rotationMultiply, minRotation, maxRotation) * _decimal) / _decimal;

            currentRotation = _angleY;

            Vector3 temp = rotationModel.transform.localRotation.eulerAngles;

            if (direction == Direction.Y)
            {
                temp.y = currentRotation * rotationModelMultiply;
            }
            else if (direction == Direction.X)
            {
                temp.x = currentRotation * rotationModelMultiply;
            }
            else if(direction == Direction.Z)
            {
                temp.z = currentRotation * rotationModelMultiply;
            }
            rotationModel.transform.localRotation = Quaternion.Euler(temp);

            if (activateOnce)
            {
                if (_angleY == maxRotation & _activateOnce)
                {
                    if (onRotation != null)
                    {
                        onRotation.Invoke(_angleY);
                    }

                    if (rotationAudioSource != null)
                    {
                        rotationAudioSource.Play();
                    }

                    _activateOnce = false;
                }
                else if (_angleY == minRotation & !_activateOnce)
                {
                    _activateOnce = true;
                }
            }
            else
            {
                if (onRotation != null)
                {
                    onRotation.Invoke(_angleY);
                }

                if (rotationAudioSource != null && _prevRotation != currentRotation)
                {
                    rotationAudioSource.Play();
                }
            }
        }

        [System.Serializable]
        public class UnityEventFloat : UnityEvent<float>
        {

        }
    }
}
