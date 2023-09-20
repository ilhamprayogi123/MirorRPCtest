using UnityEngine;

namespace razz
{
    /*Handles all vehicle part animations when used by Interactor. 
    This class is a little messy. But this is written for all-in-one example.
    I've used ids instead of strings for Animation SetBools. Its 2x faster.
    Also since ProtoTruck has only one AnimationController, if any animation remains active, others wont work. 
    Because animation state needs to go back to Default state for other states to work.*/
    [HelpURL("https://negengames.com/interactor/components.html#vehiclepartcontrolscs")]
    [DisallowMultipleComponent]
    public class VehiclePartControls : MonoBehaviour
    {
        private Animator _vehicleAnimator;
        private InteractorObject[] _vehicleInteractorObjects;

        private void Start()
        {
            _vehicleAnimator = GetComponent<Animator>();
            _vehicleInteractorObjects = GetComponentsInChildren<InteractorObject>();

            //These loops take all vehicle InteractorObjects, compare with Vehicle Animator parameters,
            //sets their hash values to their Ids if has same name.
            for (int i = 0; i < _vehicleInteractorObjects.Length; i++)
            {
                _vehicleInteractorObjects[i].isVehiclePartwithAnimation = true;

                for (int a = 0; a < _vehicleAnimator.parameterCount; a++)
                {
                    if (_vehicleAnimator.parameters[a].name == _vehicleInteractorObjects[i].name)
                    {
                        _vehicleInteractorObjects[i].vehiclePartId = Animator.StringToHash(_vehicleAnimator.parameters[a].name);
                    }
                }
            }
        }

        private void Update()
        {
            if (BasicInput.GetReset())
                ResetAnim(null);
        }

        //Toggles active animation state, temporary solution.
        private void ResetAnim(Interactor interactor)
        {
            if (interactor) interactor.interactionStates.playerUsing = false;

            AnimatorClipInfo[] _currentClipInfo = _vehicleAnimator.GetCurrentAnimatorClipInfo(0);
            if (_currentClipInfo.Length > 0 && _currentClipInfo[0].clip.name != "Mladder_Extension" && _currentClipInfo[0].clip.name != "Windshield2")
            {
                if (_vehicleAnimator.GetBool(_currentClipInfo[0].clip.name))
                {
                    _vehicleAnimator.SetBool(_currentClipInfo[0].clip.name, false);
                }
                else
                {
                    _vehicleAnimator.SetBool(_currentClipInfo[0].clip.name, true);
                }
            }
        }

        //For Elevator Animation Event
        public void SetElevatorFalse()
        {
            _vehicleAnimator.SetBool("ElevatorButton", false);
        }

        //For BackDoor Animation Event
        public void SetBackDoorFalse()
        {
            _vehicleAnimator.SetBool("BackDoor", false);
        }

        //For SideDoor Animation Event
        public void SetSideDoorFalse()
        {
            _vehicleAnimator.SetBool("MDdoor1AB", false);
        }

        //If animation state is Default, set id part. If not, reset first.
        public void Animate(int vehiclePartId, bool onOff, Interactor interactor)
        {
            if (vehiclePartId == 0) return;

            for (int i = 0; i < _vehicleAnimator.parameterCount; i++)
            {
                if (_vehicleAnimator.parameters[i].nameHash == vehiclePartId)
                {
                    if (onOff)
                    {
                        if (!_vehicleAnimator.GetCurrentAnimatorStateInfo(0).IsName("Default"))
                        {
                            ResetAnim(interactor);
                        }
                    }

                    _vehicleAnimator.SetBool(vehiclePartId, onOff);
                    return;
                }
            }
        }

        //Called by Interactor when used MultipleCockpit
        public void ToggleWindshield(bool onOff, Interactor interactor)
        {
            if (onOff)
            {
                if (!_vehicleAnimator.GetCurrentAnimatorStateInfo(0).IsName("Default"))
                {
                    ResetAnim(interactor);
                }
                _vehicleAnimator.SetBool("Windshield", true);
            }
            else
            {
                _vehicleAnimator.SetBool("Windshield", false);
            }
        }
    }
}
