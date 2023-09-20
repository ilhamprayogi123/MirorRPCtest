using UnityEngine;
using System.Collections;

namespace razz
{
    public class LookAtTarget
    {
        private Transform _headTransform;
        private Interactor _interactor;
        private InteractorIK _interactorIK;
        //LookAt object used for transferring look target from one to another
        private GameObject _lookObject;
        private Transform _lookTransform;
        private InteractorObject _currentLookTarget;
        private Transform _currentLookTransform;
        private InteractorObject _nextLookTarget;
        private Look _currentLookState = Look.Never;
        private Look _nextLookState = Look.Never;
        private float _currentLookDuration = 0.6f;
        private float _durationMultiplier = 1f;
        private float _durationMultiplierOut = 1f;
        private bool _nonstopLook;
        private bool _interruptLooking;
        private bool _lookActive;
        private bool _lookEnding;
        private bool _interruptEnding;
        private Vector3 _lastHeadForward;

        public bool Init(Interactor interactor, InteractorIK interactorIK, Transform head)
        {
            _interactor = interactor;
            _interactorIK = interactorIK;
            if (!_interactor.lookAtTargetEnabled || !head || !_interactorIK)
            {
                InitFail();
                return false;
            }

            _headTransform = head;
            _interactorIK.SetHeadBone(head);
            CreateLookTarget();
            LookDeactive();
            return true;
        }

        private void InitFail()
        {
            if (_interactorIK) _interactorIK.lookEnabled = false;
            if (_interactor) _interactor.lookAtTargetEnabled = false;
            Debug.LogWarning("Head bone couldn't find!", _interactorIK);
            Debug.LogWarning("Look At Target disabled.", _interactor);
        }
        private void CreateLookTarget()
        {
            _lookObject = new GameObject();
            _lookObject.name = "LookTransform";
            _lookTransform = _lookObject.transform;
            _interactorIK.lookTarget = _lookTransform;
        }
        //Resets lookObject to 1 unit forward of head and makes its child so head will have default straight look.
        private void LookDeactive()
        {
            _lookTransform.rotation = _headTransform.rotation;
            _lookTransform.position = _headTransform.position + _interactor.transform.forward;
            _currentLookState = Look.Never;
            if (!_lookTransform.parent) _lookTransform.SetParent(_headTransform);
            _interactorIK.lookWeight = 0;
        }

        private void LookActive()
        {
            _interactorIK.lookWeight = _currentLookTarget.lookWeight;
        }

        public void NewLookOrder(InteractorObject interactorObject, Look newLookState)
        {
            if (interactorObject == null || interactorObject.lookAtThis == Look.Never)
            {
                CheckLookTargets();
                return;
            }

            if (_currentLookTarget == interactorObject)
            {
                TryCurrentLook(newLookState);
                return;
            }

            AssignNextLookTarget(interactorObject, newLookState);
        }

        private void AssignNextLookTarget(InteractorObject interactorObject, Look newLookState)
        {
            if (newLookState == Look.Never) return;
            if (!interactorObject.lookAtThis.HasFlag(newLookState)) return;
            if (_nextLookTarget != null && _nextLookTarget.priority > interactorObject.priority) return;

            _nextLookTarget = interactorObject;
            _nextLookState = newLookState;

            if (interactorObject.waitTimeToLook == 0)
                TryNextLook();
            else
                _interactor.waitForNewTarget = interactorObject.waitTimeToLook;
                
        }
        //If look target is gone (disabled or destroyed), remove from next, current or both
        public void RemoveLookTargets(InteractorObject removeObject)
        {
            if (_nextLookTarget == removeObject)
            {
                NextLookTargetToNull();
            }

            if (_currentLookTarget == removeObject)
            {
                if (_lookActive && !_lookEnding)
                {
                    _interruptEnding = false;
                    _interactor.StartCoroutine(EndLookProcess());
                }
                CurrentLookTargetToNull();
                CheckLookTargets();
            }
        }

        public void RemoveCurrentLookTarget()
        {
            RemoveLookTargets(_currentLookTarget);
        }
        //Check for next, if not exist end current
        private void CheckLookTargets()
        {
            if (TryNextLook()) return;
            if (CheckCurrentLook()) return;
            if (!_lookEnding) _interactor.StartCoroutine(EndLookProcess());
        }

        private bool CheckCurrentLook()
        {
            if (!_lookActive) return true;

            if (_currentLookState == Look.OnSelection)
                return _interactor.CheckSelection(_currentLookTarget);
            else
                return false;
        }

        private void TryCurrentLook(Look newLookState)
        {
            if (_currentLookState == newLookState) return;
            if (_currentLookState > newLookState || !_currentLookTarget.lookAtThis.HasFlag(newLookState) || newLookState == Look.Never)
            {
                if (!_lookEnding)
                    RemoveLookTargets(_currentLookTarget);
                return;
            }

            if (_lookActive)
            {
                if (newLookState == Look.After)
                    _nonstopLook = false;
                else if (newLookState == Look.OnPause && !_currentLookTarget.lookAtThis.HasFlag(Look.After))
                    _nonstopLook = false;
                return;
            }

            _currentLookState = newLookState;
            if (_lookEnding)
            {
                _nextLookTarget = _currentLookTarget;
                _nextLookState = newLookState;
                CheckLookTargets();
                return;
            }

            _interactor.StartCoroutine(LookTransfer());
        }

        public bool TryNextLook()
        {
            if (!_nextLookTarget || !_interactor.CheckInteraction(_nextLookTarget))
            {
                NextLookTargetToNull();
                return false;
            }

            if (_lookActive && _currentLookTarget != null)
            {
                bool switchNextFrame = false;
                if (_nextLookTarget.priority >= _currentLookTarget.priority)
                    switchNextFrame = true;
                if (_currentLookState == Look.Before && _nextLookState >= Look.OnPause)
                    switchNextFrame = true;
                if (_nextLookState == Look.OnSelection)
                    switchNextFrame = true;

                if (switchNextFrame)
                    _interruptLooking = true;
                return true;
            }

            //Switch now
            if (_lookEnding)
                _interruptEnding = true;
            NextLookTargetToCurrent();
            NextLookTargetToNull();

            _interactor.StartCoroutine(LookTransfer());
            return true;
        }

        private void CurrentLookTargetToNull()
        {
            _currentLookTarget = null;
            _currentLookState = Look.Never;
            _interactor.lookEndTimer = 0;
        }

        private void NextLookTargetToCurrent()
        {
            _currentLookTarget = _nextLookTarget;
            _currentLookState = _nextLookState;
            _interactor.lookEndTimer = 0;
        }

        private void NextLookTargetToNull()
        {
            _nextLookTarget = null;
            _nextLookState = Look.Never;
        }
        //We lerp between the targets so head can rotate to new target without interruption
        private IEnumerator LookTransfer()
        {
            _interactor.lookEndTimer = _currentLookTarget.lookTimeout;
            _nonstopLook = true;
            _currentLookDuration = _currentLookTarget.rotationDurationTarget;
            _durationMultiplier = 1f / _currentLookDuration;
            _durationMultiplierOut = 1f / _currentLookTarget.rotationDurationBack;

            if (_currentLookState == Look.OnSelection)
            {
                _nonstopLook = true;
            }
            else if (_currentLookState == Look.Before)
            {
                _nonstopLook = true;
            }
            else if (_currentLookState == Look.OnPause)
            {
                if (!_currentLookTarget.lookAtThis.HasFlag(Look.After))
                    _nonstopLook = false;
            }
            else if (_currentLookState == Look.After)
            {
                _nonstopLook = false;
            }

            GetLookTargetTransform();
            _lookActive = true;
            _lookTransform.SetParent(_currentLookTransform);
            yield return new WaitUntil(LookProcess);
            _lookTransform.SetParent(null);
            _interactor.lookEndTimer = 0;

            if (_interruptLooking)
                _interruptLooking = false;

            if (!_lookEnding) 
                _interactor.StartCoroutine(EndLookProcess());
            CheckLookTargets();
        }

        private void GetLookTargetTransform()
        {
            if (_currentLookTarget.alternateLookTarget)
            {
                _currentLookTransform = _currentLookTarget.alternateLookTarget;
                return;
            }
            else if (_currentLookTarget.lookAtChildren)
            {
                if (_currentLookTarget.selfSettings && _currentLookTarget.selfSettings.selfActive)
                {
                    _currentLookTransform = _interactor.ReturnSelfActiveTarget().transform;
                    if (_currentLookTransform) return;
                }

                for (int i = 0; i < _interactor.effectors.Length; i++)
                {
                    if (_interactor.effectors[i].connectedTo == _currentLookTarget)
                    {
                        _currentLookTransform = _interactor.effectors[i].connectedTarget.transform;
                        if (_currentLookTransform) return;
                    }
                }
            }

            _currentLookTransform = _currentLookTarget.transform;
        }

        private bool LookProcess()
        {
            if (_interruptLooking || !_currentLookTarget) return true;
            if (_lookEnding && !_interruptEnding) return true;

            if (_currentLookDuration > 0 || _nonstopLook)
            {
                _lookTransform.position = Vector3.MoveTowards(_lookTransform.position, _currentLookTransform.position, Time.deltaTime * _durationMultiplier);
                LookActive();
                _currentLookDuration -= Time.deltaTime;
                return false;
            }
            else
            {
                if (!_nonstopLook) _nonstopLook = true;
                return true;
            }
        }

        private IEnumerator EndLookProcess()
        {
            _lookActive = false;
            _interruptLooking = false;
            _lookEnding = true;
            _lastHeadForward = _interactorIK.lastHeadDirection;

            if (!_lookTransform)
            {
                CreateLookTarget();
                CurrentLookTargetToNull();
                _lookEnding = false;
                LookDeactive();
                yield break;
            }

            float lastDistance = Vector3.Distance(_lookTransform.position, _headTransform.position);
            if (lastDistance > _interactor.sphereCol.radius)
            {
                _lookTransform.position = _headTransform.position + (_lookTransform.position - _headTransform.position).normalized;
            }

            //We can't set parent now because animation will reset head rotation and head will look at target and then it will rotate straight before ik pass with animation. Then next ik pass will continue look straight since target is now in front of head.
            //_lookTransform.SetParent(_headTransform);
            _lookTransform.position = _headTransform.position + _lastHeadForward;
            _interactorIK.lookWeight = 1f;

            while (_interactorIK.lookWeight > 0)
            {
                if (_interruptEnding)
                {
                    _lookEnding = false;
                    _interruptEnding = false;
                    yield break;
                }

                _lookTransform.position = _headTransform.position + _lastHeadForward;

                _interactorIK.lookWeight = Mathf.MoveTowards(_interactorIK.lookWeight, 0, Time.deltaTime * _durationMultiplierOut);
                yield return null;
            }

            CurrentLookTargetToNull();
            _lookEnding = false;
            LookDeactive();
        }

        public void DrawDebugLines()
        {
#if UNITY_EDITOR
            if (_interactor.debug)
                Debug.DrawLine(_headTransform.position, _lookTransform.position, Color.white);
#endif
        }
    }
}
