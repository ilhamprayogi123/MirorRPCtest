using UnityEngine;
using UnityEngine.Events;

namespace razz
{
    //Deals with switches' rotation, sounds and values
    [HelpURL("https://negengames.com/interactor/components.html#interactiveswitchcs")]
    [DisallowMultipleComponent]
    public class InteractiveSwitch : MonoBehaviour
    {
        private int _currentSwitch = 0;

        [SerializeField, Tooltip("What audiosource will play on click")]
        public AudioSource clickAudioSource;
        [SerializeField, Tooltip("What will be transformed on click")]
        public GameObject switchModel;
        [SerializeField, Tooltip("How many switches will be")]
        public UnitySwitchEvent[] unitySwitchEvents;

        public void Click()
        {
            if (unitySwitchEvents.Length == 0) return;

            if (_currentSwitch < unitySwitchEvents.Length - 1)
            {
                _currentSwitch++;
            }
            else
            {
                _currentSwitch = 0;
            }
            CallOnClickEvent();
        }

        private void CallOnClickEvent()
        {
            if (unitySwitchEvents[_currentSwitch] == null) return;

            unitySwitchEvents[_currentSwitch].eventOnClick.Invoke();
            if (clickAudioSource != null)
            {
                clickAudioSource.Play();
            }
            if (switchModel != null)
            {
                switchModel.transform.localPosition = unitySwitchEvents[_currentSwitch].switchPostion;
                switchModel.transform.localRotation = Quaternion.Euler(unitySwitchEvents[_currentSwitch].switchRotation);
            }
        }

        [System.Serializable]
        public class UnitySwitchEvent
        {
            [Tooltip("Event that will be called on click")]
            public UnityEvent eventOnClick;
            [Tooltip("Switch position on click")]
            public Vector3 switchPostion;
            [Tooltip("Switch rotation on click")]
            public Vector3 switchRotation;
        }
    }
}
