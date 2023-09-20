using UnityEngine;

namespace razz
{
    [CreateAssetMenu(fileName = "PickableSettings", menuName = "Interactor/PickableSettings")]
    public class Pickable : InteractionTypeSettings
    {
        [Tooltip("Multiplier for moving pickup object, 4 will be similar to this interaction durations (in and out).")]
        [Range(0, 8f)] public float pickupSpeed = 1f;
        [Header("One Hand")]
        [Tooltip("If disabled, object will follow hand animation when picked up. Also this option is required for Drop Back feature (until v1.00).")]
        public bool holdInPosition;
        [Tooltip("One handed object target position relative to player. You can adjust this to change position of one handed objects while holding.")]
        public Vector3 holdPosition = Vector3.zero;
        [Tooltip("One handed object target rotation relative to player.")]
        public Vector3 holdRotation = Vector3.zero;
        [Tooltip("Requires Hold In Position (will change in v1.00). \n\nIf picked up location is in range of picked effector, it will drop on same location with same rotation. If you want to drop on other locations, check dropLocations at Other Settings on InteractorObject. You can add as many as you want.")]
        public bool dropBack;

        [Header("Two Hands")]
        [Tooltip("Two handed object target position relative to player. You can adjust this to change position of two handed objects while holding.")]
        public Vector3 holdPoint = new Vector3(0, 0.93f, 0.3f);
        [Tooltip("Target lerp between raycast hit and effector position")]
        [Range(0, 1)]
        public float twoHandCloser;
        [Tooltip("Targets will be repositioned to left and right when interacted. Raycast will determine their positions (from center to left and center to right).")]
        public bool raycastTargets = true;

        [HideInInspector] public bool oneHandPicked;
        [HideInInspector] public bool twoHandPicked;
        [HideInInspector] public bool pickable;
        [HideInInspector] public Vector3 pickPos;
        [HideInInspector] public bool dropDone;

        private Transform _playerTransform;
        private Transform _oldParentTransform;
        private Collider _col;
        private Transform _lastPickedChildTransform;
        private Vector3 _lastPickedChildLocalPos;
        private Quaternion _lastPickedChildLocalRot;
        private Vector3 _pickLocalPos;
        private Quaternion _pickLocalRot;
        private Quaternion _pickRot;
        private Quaternion _startRot;
        private Vector3 _startPos;
        private float _holdWeight;
        private bool _pickReady;
        private bool _droppingToLocation;
        private float _durationTarget, _durationBack;
        private float _elapsedTime;

        public override void Init(InteractorObject interactorObject)
        {
            base.Init(interactorObject);

            _col = _intObj.col;

            if (!_col) Debug.Log(_intObj.name + " has no collider!");
        }

        public override void UpdateSettings()
        {
            if (_intObj.interactionType == InteractionTypes.PickableOne)
            {
                //If this object is moving and interacted by Interactor, it needs to rotate the pivot until interaction on pause. So target change rotation towards effector.
                if (_intObj.rotating)
                {
                    //Always get latest positions until hand reaches to object. Will turn off rotating so it won't update after on pause.
                    if (_pickReady)
                    {
                        _lastPickedChildLocalPos = _lastPickedChildTransform.localPosition;
                        _lastPickedChildLocalRot = _lastPickedChildTransform.localRotation;
                        pickPos = _intObj.transform.position;
                        _pickRot = _intObj.transform.rotation;
                        _pickLocalPos = _intObj.transform.localPosition;
                        _pickLocalRot = _intObj.transform.localRotation;
                        _intObj.rotating = false;
                    }
                    _intObj.Rotate(_intObj.rotateTo);
                }
                //Interaction will wait for dropDone to resume interaction. Then it will release the object and continue from on pause state.
                if (_droppingToLocation)
                {
                    if (_elapsedTime >= _durationBack && !dropDone)
                    {
                        dropDone = true;
                        _elapsedTime = 0;
                        if (_intObj.hasRigid)
                        {
                            _intObj.rigid.isKinematic = false;
                            if (_intObj.currentInteractor.playerRigidbody)
                                _intObj.rigid.velocity = _intObj.currentInteractor.playerRigidbody.velocity;
                        }
                        if (_col) _col.enabled = true;
                        _droppingToLocation = false;
                        return;
                    }

                    _elapsedTime += Time.deltaTime;

                    if (_intObj.easeType == EaseType.CustomCurve)
                    {
                        _holdWeight = Mathf.Clamp01(Ease.FromType(_intObj.easeType)(1f + (_elapsedTime / _durationBack), _intObj.speedCurve));
                    }
                    else
                    {
                        _holdWeight = Mathf.Clamp01(Ease.FromType(_intObj.easeType)(_elapsedTime / _durationBack));
                    }
                    _holdWeight = 1f - _holdWeight;
                    //These are local because object has its old parent now.
                    _intObj.transform.localRotation = Quaternion.Lerp(_pickLocalRot, _startRot, _holdWeight);
                    _intObj.transform.localPosition = Vector3.Lerp(_pickLocalPos, _startPos, _holdWeight);
                    return;
                }
                else if (_pickReady && holdInPosition)
                {//Pickready set from a single Pick() call, now it can pick object to desired hold location until drop event.
                    if (_elapsedTime >= _durationTarget)
                    {
                        _pickReady = false;
                        _elapsedTime = 0;
                        return;
                    }
                    _elapsedTime += Time.deltaTime;
                    _holdWeight = Mathf.Clamp01(Ease.FromType(_intObj.easeType)(_elapsedTime / _durationTarget, _intObj.speedCurve));
                    Vector3 playerRelativePos = _playerTransform.position + _playerTransform.right * holdPosition.x + _playerTransform.up * holdPosition.y + _playerTransform.forward * holdPosition.z;
                    Quaternion playerRelativeRot = _playerTransform.rotation * Quaternion.Euler(holdRotation);
                    _intObj.transform.rotation = Quaternion.Lerp(_pickRot, playerRelativeRot, _holdWeight);
                    _intObj.transform.position = Vector3.Lerp(pickPos, playerRelativePos, _holdWeight);
                    return;
                }
            }
            else
            {
                //Simpler version of one hand pick. It will move object with on pause and will stop when reached.
                if (twoHandPicked)
                {
                    if (_elapsedTime >= _durationTarget) return;

                    _elapsedTime += Time.deltaTime;
                    _holdWeight = Mathf.Clamp01(Ease.FromType(_intObj.easeType)(_elapsedTime / _durationTarget, _intObj.speedCurve));
                    Vector3 playerRelativePos = _playerTransform.position + _playerTransform.right * holdPoint.x + _playerTransform.up * holdPoint.y + _playerTransform.forward * holdPoint.z;
                    _intObj.transform.position = Vector3.Lerp(pickPos, playerRelativePos, _holdWeight);
                }
            }
        }

        //One or Two handed picks, also uses late update loop for moving objects
        public void Pick(Transform toParentTransform, Transform childTarget)
        {
            if (_pickReady) return;

            _playerTransform = _intObj.currentInteractor.playerTransform;
            //Durations comes from InteractorObject interaction speed settings
            _durationTarget = _intObj.targetDuration / pickupSpeed;
            _durationBack = _intObj.backDuration / pickupSpeed;
            dropDone = false;
            _oldParentTransform = _intObj.transform.parent;

            if (_intObj.hasRigid)
            {
                _intObj.rigid.velocity = Vector3.zero;
                _intObj.rigid.isKinematic = true;
            }
            if (_col) _col.enabled = false;

            if (_intObj.interactionType == InteractionTypes.PickableTwo)
            {
                //Cache object position for pick up on LateUpdate
                pickPos = _intObj.transform.position;
                twoHandPicked = true;
            }
            else
            {
                //Cache object positions for pick up on LateUpdate, will update more if object is rotating pivot.
                _lastPickedChildTransform = childTarget;
                _lastPickedChildLocalPos = childTarget.localPosition;
                _lastPickedChildLocalRot = childTarget.localRotation;
                pickPos = _intObj.transform.position;
                _pickLocalPos = _intObj.transform.localPosition;
                _pickLocalRot = _intObj.transform.localRotation;
                _pickRot = _intObj.transform.rotation;

                if (!holdInPosition)
                {
                    //parentTransform is hand bone position
                    _intObj.transform.position += toParentTransform.position - childTarget.position;
                    _lastPickedChildTransform.parent = _playerTransform;
                }
                oneHandPicked = true;
            }
            _intObj.transform.parent = toParentTransform;
            //New parent so it needs new local caches, these are for lerp start positions.
            _startRot = _intObj.transform.localRotation;
            _startPos = _intObj.transform.localPosition;

            _pickReady = true;
        }
        //dropTransformIndex gives possible to drop transfrom index. -1 for drop original position, -2 for skip dropping back.
        public void Drop(int dropTransformIndex)
        {
            //canDrop checked by Interactor, if the pick location is in reach for this effector so it can drop to location. Otherwise, drop instantly.
            if (oneHandPicked && dropBack && dropTransformIndex > -2)
            {//-2 drop instantly, -1 drop to pick loc, 0 & 0+ drop to DropLoc index
                DropBack(dropTransformIndex);
                return;
            }

            _intObj.transform.parent = _oldParentTransform;

            if (_intObj.hasRigid)
            {
                _intObj.rigid.isKinematic = false;
                if (_intObj.currentInteractor.playerRigidbody)
                    _intObj.rigid.velocity = _intObj.currentInteractor.playerRigidbody.velocity;
            }
            if (_col) _col.enabled = true;

            if (_intObj.interactionType == InteractionTypes.PickableOne)
            {
                if (_lastPickedChildTransform)
                {
                    if (_intObj.pivot)
                        _lastPickedChildTransform.parent = _intObj.pivot.transform;
                    else
                        _lastPickedChildTransform.parent = _intObj.transform;

                    _lastPickedChildTransform.localPosition = _lastPickedChildLocalPos;
                    _lastPickedChildTransform.localRotation = _lastPickedChildLocalRot;
                }
            }
            dropDone = true;
            Reset();
        }

        private void DropBack(int dropTransformIndex)
        {
            if (dropTransformIndex > -1 && dropTransformIndex < _intObj.dropLocations.Length)
            {
                _pickLocalRot = _intObj.dropLocations[dropTransformIndex].localRotation;
                _pickLocalPos = _intObj.dropLocations[dropTransformIndex].localPosition;
                _intObj.transform.parent = _intObj.dropLocations[dropTransformIndex].parent;
            }
            else
            {//Drop to pick location instead of DropLocations
                _intObj.transform.parent = _oldParentTransform;
            }
            _droppingToLocation = true;
            _elapsedTime = 0;
            _startPos = _intObj.transform.localPosition;
            _startRot = _intObj.transform.localRotation;
        }
        //Reset all so it can be pickable again.
        public void Reset()
        {
            _holdWeight = 0f;
            _elapsedTime = 0;
            _pickReady = false;
            _droppingToLocation = false;
            oneHandPicked = false;
            twoHandPicked = false;
            pickable = false;
            if (_intObj)
                _intObj.ResetUseableEffectors();
        }

        //Repositions the targets of two handed pickup object. Sends two raycasts from object center with given closer offset to left and right.
        public void PickableTwoRetarget(Transform target, Vector3 posOffset)
        {
            _playerTransform = _intObj.currentInteractor.playerTransform;
            RaycastHit hit;
            LayerMask layerMask = _intObj.currentInteractor.GetPlayerLayerMask();
            //Closer is 0-1 value between object center and player position. It can be adjusted to fit better for object shape.
            //But if its too close to player, its raycast can miss hit so target wont change, which causes weird looks.
            Vector3 closerPos = Vector3.Lerp(_intObj.transform.position, _playerTransform.position, twoHandCloser);

            //This means effector is at left side of player so its goint to be left hand position.
            if (posOffset.x < 0)
            {
                Physics.Raycast(closerPos - _playerTransform.right, _playerTransform.right, out hit, 1f, layerMask);

                Debug.DrawRay(closerPos - _playerTransform.right, _playerTransform.right, Color.red, 10f);

                if (hit.collider == _col)
                {
                    target.transform.position = hit.point;
                    target.transform.rotation = Quaternion.LookRotation(-hit.normal, _playerTransform.forward);
                }
            }
            //Right side
            else
            {
                Physics.Raycast(closerPos + _playerTransform.right, -_playerTransform.right, out hit, 1f, layerMask);

                Debug.DrawRay(closerPos + _playerTransform.right, -_playerTransform.right, Color.blue, 10f);

                if (hit.collider == _col)
                {
                    target.transform.position = hit.point;
                    target.transform.rotation = Quaternion.LookRotation(-hit.normal, _playerTransform.forward);
                }
            }
        }
    }
}
