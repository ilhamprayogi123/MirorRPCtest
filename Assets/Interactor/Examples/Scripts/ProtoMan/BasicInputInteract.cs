using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace razz
{
    //Simplified BasicInput.cs for just starting some of interactions
    public class BasicInputInteract : MonoBehaviour
    {
        private InteractionStates _interactionStates;
        private Interactor _interactor;
        private bool _use;
        private bool _click;

        private void Start()
        {
            _interactor = GetComponent<Interactor>();
            _interactionStates = _interactor.interactionStates;
        }
        #region InputSystem & Legacy Controls Mapping
#if ENABLE_INPUT_SYSTEM
        public static InteractorKeys keys;
        private static bool _workOutOfFocus;

        [Header("Input Focus Setting")]
        public bool workOutOfFocus;

        private void Awake()
        {
            keys = new InteractorKeys();
            keys.Enable();
            SetWindowFocus();
        }

        private void SetWindowFocus()
        {
            _workOutOfFocus = workOutOfFocus;
        }

        //Keyboard
        public static bool GetUse()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

            return Keyboard.current.eKey.wasPressedThisFrame;
        }
        public static bool GetStopAll()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

            return Keyboard.current.tKey.wasPressedThisFrame;
        }

        //Mouse
        public static bool GetLeftClick()
        {
            if (!_workOutOfFocus && !Application.isFocused) return false;

            return Mouse.current.leftButton.wasPressedThisFrame;
        }
        public static float GetMouseWheel()
        {
            if (!_workOutOfFocus && !Application.isFocused) return 0;

            return keys.InteractorExampleSceneControls.MouseWheel.ReadValue<float>();
        }

#else
        //Keyboard
        public static bool GetUse()
        {
            return Input.GetKeyDown(KeyCode.E);
        }
        public static bool GetStopAll()
        {
            return Input.GetKeyDown(KeyCode.T);
        }

        //Mouse
        public static bool GetLeftClick()
        {
            return Input.GetKeyDown(KeyCode.Mouse0);
        }
        public static float GetMouseWheel()
        {
            return Input.GetAxis("Mouse ScrollWheel");
        }
#endif
        #endregion

        private void Update()
        {
            if (!_interactionStates) return;

            if (!_use)
            {
                _use = GetUse();
            }

            if (!_click && _interactionStates.playerUsable)
            {
                _click = GetLeftClick();

                if (_click && !_interactionStates.playerUsing)
                {
                    _interactionStates.playerUsing = true;
                }
            }

            //Press T to stop all interactions
            if (GetStopAll())
            {
                _interactor.DisconnectAll();
                Debug.Log("Stopped all interactions.");
            }
        }

        private void FixedUpdate()
        {
            if (!_interactionStates) return;

            if (_use)
            {
                _interactor.StartStopInteractions(false);
            }

            if (_click)
            {
                _interactor.StartStopInteractions(true);
            }

            _use = false;
            _click = false;
        }
    }
}
