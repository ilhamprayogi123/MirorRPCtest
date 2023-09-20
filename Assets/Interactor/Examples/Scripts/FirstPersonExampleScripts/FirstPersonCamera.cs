using UnityEngine;

namespace razz
{
    public class FirstPersonCamera : MonoBehaviour
    {
        public Transform playerRoot;
        public float mouseSensivity = 2f;

        private Vector2 _mouseMove;
        private float _rotationX;
        private float _rotationY;
        private bool _screenLock;

        private void Awake()
        {
            Cursor.lockState = _screenLock ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !_screenLock;
        }

        private void Update()
        {
            if (BasicInput.GetEscape())
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                _screenLock = false;
            }

            if (BasicInput.GetRightClickRelease())
            {
                if (_screenLock)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    _screenLock = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    _screenLock = true;
                }
            }

            if (_screenLock)
                _mouseMove = new Vector2(BasicInput.GetMouseX() * mouseSensivity, BasicInput.GetMouseY() * mouseSensivity);
            else
                _mouseMove = Vector2.zero;

            _rotationY += _mouseMove.x;
            _rotationX -= _mouseMove.y;
            _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);
        }

        private void FixedUpdate()
        {
            playerRoot.rotation = Quaternion.Euler(0, _rotationY, 0);
            transform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
        }
    }
}
