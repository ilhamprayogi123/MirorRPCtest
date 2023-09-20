using UnityEngine;
using System.Collections.Generic;

namespace razz
{
    [HelpURL("https://negengames.com/interactor/components.html#freelookcamcs")]
    [DisallowMultipleComponent]
    public class FreeLookCam : MonoBehaviour
    {
        private float _lookAngle;
        private float _tiltAngle;
        private Vector3 _pivotEulers;
        private Quaternion _pivotTargetRot;
        private Quaternion _transformTargetRot;
        private Transform _cam;
        private static Transform _pivot;
        private bool _screenLock;
        private static List<GameObject> lockers;

        [SerializeField] private Transform m_Target;
        [SerializeField] private float m_MoveSpeed = 1f;
        [Range(0f, 10f)] [SerializeField] private float m_TurnSpeed = 1.5f;
        [SerializeField] private float m_TurnSmoothing = 0.0f;
        [SerializeField] private float m_TiltMax = 75f;
        [SerializeField] private float m_TiltMin = 45f;
        [SerializeField] private bool m_VerticalAutoReturn = false;

        void Awake()
        {
            _cam = GetComponentInChildren<Camera>().transform;
            _pivot = _cam.parent;
            Cursor.lockState = _screenLock ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !_screenLock;
            _pivotEulers = _pivot.rotation.eulerAngles;
            _pivotTargetRot = _pivot.transform.localRotation;
            _transformTargetRot = transform.localRotation;
            lockers = new List<GameObject>();
        }

        void Start()
        {
            if (m_Target == null) return;

            transform.position = m_Target.position;
        }

        void Update()
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
                HandleRotationMovement();
        }

        private void OnApplicationFocus(bool focus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _screenLock = true;
        }

        void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void FollowTarget(float deltaTime)
        {
            if (m_Target == null) return;

            transform.position = Vector3.Lerp(transform.position, m_Target.position, deltaTime * m_MoveSpeed);
        }

        private void HandleRotationMovement()
        {
            if (Time.timeScale < float.Epsilon) return;

            float x = BasicInput.GetMouseX();
            float y = 0;

            _lookAngle += x * m_TurnSpeed;
            _transformTargetRot = Quaternion.Euler(0f, _lookAngle, 0f);

            if (lockers.Count == 0)
            {
                y = BasicInput.GetMouseY();

                if (m_VerticalAutoReturn)
                {
                    _tiltAngle = y > 0 ? Mathf.Lerp(0, -m_TiltMin, y) : Mathf.Lerp(0, m_TiltMax, -y);
                }
                else
                {
                    _tiltAngle -= y * m_TurnSpeed;
                    _tiltAngle = Mathf.Clamp(_tiltAngle, -m_TiltMin, m_TiltMax);
                }
            }
            else
            {
                _tiltAngle = Mathf.Clamp(_pivot.transform.eulerAngles.x, 0, 30);
            }

            _pivotTargetRot = Quaternion.Euler(_tiltAngle, _pivotEulers.y, _pivotEulers.z);

            if (m_TurnSmoothing > 0)
            {
                _pivot.localRotation = Quaternion.Slerp(_pivot.localRotation, _pivotTargetRot, m_TurnSmoothing * Time.deltaTime);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, _transformTargetRot, m_TurnSmoothing * Time.deltaTime);
            }
            else
            {
                _pivot.localRotation = _pivotTargetRot;
                transform.localRotation = _transformTargetRot;
            }
        }

        public static void LockCamY(GameObject lockerObject)
        {
            if (lockers.Count == 0)
            {
                lockers.Add(lockerObject);
            }
            else if (lockers.Contains(lockerObject))
            {
                lockers.Remove(lockerObject);
            }
            else
            {
                lockers.Add(lockerObject);
            }
        }

        private void FixedUpdate()
        {
            FollowTarget(Time.fixedDeltaTime);
        }

        /*private void LateUpdate()
        {
            FollowTarget(Time.deltaTime);
        }*/

        public void SetTarget(Transform newTransform)
        {
            m_Target = newTransform;
        }

        public Transform Target
        {
            get { return m_Target; }
        }
    }
}
