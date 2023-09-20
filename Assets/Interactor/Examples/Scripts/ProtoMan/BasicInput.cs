using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace razz
{
    [HelpURL("https://negengames.com/interactor/components.html#basicinputcs")]
    [DisallowMultipleComponent]
    public class BasicInput : MonoBehaviour
    {
        public bool updateOnFixedUpdate = true;

        private PlayerController _playerController;
        private InteractionStates _interactionStates;
        private Interactor _interactor;
        private Transform _camTransform;
        private Vector3 _camForward;
        private Vector3 _move;
        private bool _jump;

        [HideInInspector] public bool climb;
        [HideInInspector] public bool climbable;
        [HideInInspector] public bool use;
        [HideInInspector] public bool rifle;
        [HideInInspector] public bool usable;
        [HideInInspector] public bool changed;
        [HideInInspector] public bool changable;
        [HideInInspector] public bool onVehicle;
        [HideInInspector] public bool click;

        private void Start()
        {
            if (Camera.main != null)
            {
                _camTransform = Camera.main.transform;
            }
            _playerController = GetComponent<PlayerController>();
            _interactor = GetComponent<Interactor>();
            _interactionStates = _interactor.interactionStates;
        }
        #region InputSystem & Legacy Controls Mapping
        //For animation pick animation event to set use
        public void SetUse()
        {
            use = true;
        }
#if ENABLE_INPUT_SYSTEM
        public static InteractorKeys keys;
        //private static float _horizontal;
        //private static float _vertical;
        private static bool _workOutOfFocus;

        [Header("Input Focus Setting")]
        public bool workOutOfFocus;

        private void Awake()
        {
            //_horizontal = 0;
            //_vertical = 0;
            keys = new InteractorKeys();
            keys.Enable();
            SetWindowFocus();
        }

        private void SetWindowFocus()
        {
            _workOutOfFocus = workOutOfFocus;
        }

        //Keyboard
        public static float GetHorizontal()
        {
            if (!_workOutOfFocus && !Application.isFocused) return 0;

            /*_horizontal = Mathf.MoveTowards(_horizontal, keys.InteractorExampleSceneControls.Horizontal.ReadValue<float>(), 0.1f);

            return _horizontal;*/
            return InteractorAiInput.GetAxis("Horizontal");
        }
        public static float GetVertical()
        {
            if (!_workOutOfFocus && !Application.isFocused) return 0;

            /*_vertical = Mathf.MoveTowards(_vertical, keys.InteractorExampleSceneControls.Vertical.ReadValue<float>(), 0.1f);

            return _vertical;*/
            return InteractorAiInput.GetAxis("Vertical");
        }
        public static bool GetJump()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

            return Keyboard.current.spaceKey.wasPressedThisFrame;
        }
        public static bool GetUse()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

#if UNITY_ANDROID || UNITY_IOS
            return Gamepad.current.buttonEast.wasPressedThisFrame;
#else
            return Keyboard.current.eKey.wasPressedThisFrame;
#endif
        }
        public static bool GetRifle()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

            return Keyboard.current.gKey.wasPressedThisFrame;
        }
        public static bool GetStopAll()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

            return Keyboard.current.tKey.wasPressedThisFrame;
        }
        public static bool GetPushUp()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

            return Keyboard.current.yKey.wasPressedThisFrame;
        }
        public static bool GetCrouch()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

            return Keyboard.current.cKey.isPressed;
        }
        public static bool GetShift()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

            return Keyboard.current.leftShiftKey.isPressed;
        }
        public static bool GetEscape()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

            return Keyboard.current.escapeKey.wasPressedThisFrame;
        }
        public static bool GetEnter()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

            return Keyboard.current.enterKey.wasPressedThisFrame;
        }
        public static bool GetReset()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

            return Keyboard.current.uKey.wasPressedThisFrame;
        }
        public static bool GetToggle()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

            return Keyboard.current.nKey.wasPressedThisFrame;
        }
        public static bool GetBackdoor()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

            return Keyboard.current.bKey.wasPressedThisFrame;
        }
        public static float GetBrake()
        {
            if (!_workOutOfFocus && !Application.isFocused) return 0;

            return Mathf.Clamp(InteractorAiInput.GetAxis("Vertical"), -1f, 0);
        }

        //Mouse
        public static Vector2 GetMousePosition()
        {
            if (!_workOutOfFocus && !Application.isFocused) return Vector2.zero;

#if UNITY_ANDROID || UNITY_IOS
            return Vector2.zero;
#else
            return Mouse.current.position.ReadValue();
#endif
        }
        public static bool GetLeftClick()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

#if UNITY_ANDROID || UNITY_IOS
            return false;
#else
            return Mouse.current.leftButton.wasPressedThisFrame;
#endif
        }
        public static bool GetRightClickRelease()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

#if UNITY_ANDROID || UNITY_IOS
            return false;
#else
            return Mouse.current.rightButton.wasReleasedThisFrame;
#endif
        }
        public static float GetMouseX()
        {
            if (!_workOutOfFocus && !Application.isFocused) return 0;

#if UNITY_ANDROID || UNITY_IOS
            return 0;
#else
            return Mouse.current.delta.x.ReadValue() * Time.deltaTime * 8f;
#endif
        }
        public static float GetMouseY()
        {
            if (!_workOutOfFocus && !Application.isFocused) return 0;

#if UNITY_ANDROID || UNITY_IOS
            return 0;
#else
            return Mouse.current.delta.y.ReadValue() * Time.deltaTime * 8f;
#endif
        }
        public static float GetMouseWheel()
        {
            if (!_workOutOfFocus && !Application.isFocused) return 0;

#if UNITY_ANDROID || UNITY_IOS
            return 0;
#else
            return keys.InteractorExampleSceneControls.MouseWheel.ReadValue<float>();
#endif
        }

#else
        //Keyboard
        public static float GetHorizontal()
        {
            //return Input.GetAxis("Horizontal");
            return InteractorAiInput.GetAxis("Horizontal");
        }
        public static float GetVertical()
        {
            //return Input.GetAxis("Vertical");
            return InteractorAiInput.GetAxis("Vertical");
        }
        public static bool GetJump()
        {
            return Input.GetKeyDown(KeyCode.Space);
        }
        public static bool GetUse()
        {
#if UNITY_ANDROID || UNITY_IOS
            return Input.GetKeyDown(KeyCode.Joystick1Button0);
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }
        public static bool GetRifle()
        {
            return Input.GetKeyDown(KeyCode.G);
        }
        public static bool GetStopAll()
        {
            return Input.GetKeyDown(KeyCode.T);
        }
        public static bool GetPushUp()
        {
            return Input.GetKeyDown(KeyCode.Y);
        }
        public static bool GetCrouch()
        {
            return Input.GetKey(KeyCode.C);
        }
        public static bool GetShift()
        {
            //return Input.GetKey(KeyCode.LeftShift);
            return InteractorAiInput.GetKey(KeyCode.LeftShift);
        }
        public static bool GetEscape()
        {
            return Input.GetKeyDown(KeyCode.Escape);
        }
        public static bool GetEnter()
        {
            return Input.GetKeyDown(KeyCode.Return);
        }
        public static bool GetReset()
        {
            return Input.GetKeyDown(KeyCode.U);
        }
        public static bool GetToggle()
        {
            return Input.GetKeyDown(KeyCode.N);
        }
        public static bool GetBackdoor()
        {
            return Input.GetKeyDown(KeyCode.B);
        }
        public static float GetBrake()
        {
            return Input.GetAxis("Jump");
        }

        //Mouse
        public static Vector2 GetMousePosition()
        {
#if UNITY_ANDROID || UNITY_IOS
            return Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }
        public static bool GetLeftClick()
        {
#if UNITY_ANDROID || UNITY_IOS
            return false;
#else
            return Input.GetKeyDown(KeyCode.Mouse0);
#endif
        }
        public static bool GetRightClickRelease()
        {
#if UNITY_ANDROID || UNITY_IOS
            return false;
#else
            return Input.GetKeyUp(KeyCode.Mouse1);
#endif
        }
        public static float GetMouseX()
        {
#if UNITY_ANDROID || UNITY_IOS
            return 0;
#else
            return Input.GetAxis("Mouse X");
#endif
        }
        public static float GetMouseY()
        {
#if UNITY_ANDROID || UNITY_IOS
            return 0;
#else
            return Input.GetAxis("Mouse Y");
#endif
        }
        public static float GetMouseWheel()
        {
#if UNITY_ANDROID || UNITY_IOS
            return 0;
#else
            return Input.GetAxis("Mouse ScrollWheel");
#endif
        }
#endif
        #endregion

        private void Update()
        {
            if (!_interactionStates) return;

            if (!rifle)
            {
                rifle = GetRifle();
            }

            if (!_jump)
            {
                _jump = GetJump();
            }

            if (!use)
            {
                use = GetUse();

                if (use && (_interactionStates.playerChangable || _interactionStates.playerClimable))
                {
                    if (!climb && _interactionStates.playerClimable)
                    {
                        use = false;
                        _interactionStates.playerClimable = false;

                        if (!_interactionStates.playerGrounded && !_interactionStates.playerClimbing) return;

                        climb = true;
                    }
                    else if(_interactor.selectedByUI < _interactor.intObjComponents.Count && _interactor.intObjComponents[_interactor.selectedByUI].interactorObject.interactionType == InteractionTypes.MultipleCockpit)
                    {
                        changed = true;
                        use = false;
                    }
                }
            }

            if (!click && _interactionStates.playerUsable)
            {
                click = GetLeftClick();

                if (click && !_interactionStates.playerUsing)
                {
                    _interactionStates.playerUsing = true;
                }
            }

            //Press T to stop all interactions
            if (GetStopAll())
            {
                _interactor.DisconnectAll();
                Debug.Log("Stopped all interactions.", _interactor);
            }

            //Press Y to move player upwards if its stuck
            if (GetPushUp())
            {
                GetComponent<PlayerController>().Dash();
            }

            if (!updateOnFixedUpdate) UpdateMovement();
        }

        private void FixedUpdate()
        {
            if (updateOnFixedUpdate) UpdateMovement();
        }

        private void UpdateMovement()
        {
            if (!_interactionStates) return;

            float h = GetHorizontal();
            float v = GetVertical();
            bool crouch = GetCrouch();

            if (_interactionStates.playerClimbing || _interactionStates.playerPushing)
            {
                h = 0;
                v = Mathf.Clamp01(v);
            }

            if (_camTransform != null)
            {
                _camForward = Vector3.Scale(_camTransform.forward, new Vector3(1, 0, 1)).normalized;
                _move = v * _camForward + h * _camTransform.right;
            }
            else
            {
                _move = v * Vector3.forward + h * Vector3.right;
            }

            if (GetShift() || _interactionStates.playerClimbing)
            {
                _move *= 2f;
            }
            else if (_interactionStates.playerPushing)
            {
                _move *= 0.4f;
            }
            else
            {
                _move *= 0.5f;
            }

            if (climb || changed || _interactionStates.playerClimbed)
            {
                _interactor.StartStopInteractions(false);
                _interactionStates.playerClimbed = false;
            }

            if (use)
            {
                _interactor.StartStopInteractions(false);
            }

            if (click)
            {
                _interactor.StartStopInteractions(true);
            }

            if (_interactionStates.playerOnVehicle && _playerController)
            {
                _playerController.Move(Vector3.zero, false, false, false, false, changed, false, false);
            }
            else if (_playerController)
            {
                _playerController.Move(_move, crouch, _jump, climb, use, changed, click, rifle);
            }

            _jump = false;
            climb = false;
            use = false;
            changed = false;
            click = false;
            rifle = false;
        }
    }
}
