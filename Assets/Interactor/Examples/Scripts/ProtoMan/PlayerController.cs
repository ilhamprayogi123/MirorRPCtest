using UnityEngine;

namespace razz
{
    [HelpURL("https://negengames.com/interactor/components.html#playercontrollercs")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeedMultiplier = 1f;
        public float animSpeedMultiplier = 1f;
        public float climbSpeed = 1f;
        public float climbMax = 3f;
        public float charScaleY = 1f;
        public bool debugHeight;

        private Rigidbody _playerRigidbody;
        private Animator _playerAnimator;
        private Transform _playerTransform;
        private InteractionStates _interactionStates;
        private float _defaultGroundCheckDistance;
        private const float _half = 0.5f;
        private float _turnAmount;
        private float _forwardAmount;
        private Vector3 _groundNormal;
        private float _capsuleHeight;
        private Vector3 _capsuleCenter;
        private CapsuleCollider _playerCapsuleCollider;
        private float _climbPos;
        private Ray _testHeightRay;
        private float _testHeightFloat;
        
        [SerializeField] private LayerMask m_raycastLayerMaskforRagdoll;
        [SerializeField] private float m_MovingTurnSpeed = 360;
        [SerializeField] private float m_StationaryTurnSpeed = 180;
        [SerializeField] private float m_JumpPower = 12f;
        [Range(1f, 4f)] [SerializeField] private float m_GravityMultiplier = 2f;
        [SerializeField] private float m_RunCycleLegOffset = 0.2f;
        [SerializeField] private float m_GroundCheckDistance = 0.1f;

        private void Start()
        {
            _interactionStates = GetComponent<InteractionStates>();
            if (!_interactionStates) return;

            _playerRigidbody = GetComponent<Rigidbody>();
            _playerTransform = transform;
            _playerAnimator = GetComponent<Animator>();
            _playerCapsuleCollider = GetComponent<CapsuleCollider>();
            _capsuleHeight = _playerCapsuleCollider.height;
            _capsuleCenter = _playerCapsuleCollider.center;

            _playerRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            _defaultGroundCheckDistance = m_GroundCheckDistance;
        }

        public void Move(Vector3 move, bool crouch, bool jump, bool climb, bool use, bool changed, bool clicked, bool rifle)
        {
            if (!_interactionStates) return;

            if (move.magnitude > 1f) 
                move.Normalize();
            move = transform.InverseTransformDirection(move);
            CheckGroundStatus();
            move = Vector3.ProjectOnPlane(move, _groundNormal);

            _turnAmount = Mathf.Atan2(move.x, move.z);
            if (_interactionStates.playerClimbing) 
                _turnAmount = 0;

            _forwardAmount = move.z;

            if (_forwardAmount != 0)
                _interactionStates.playerMoving = true;
            else
                _interactionStates.playerMoving = false;

            if (move.magnitude == 0 && !crouch && !jump && !climb && !use && _interactionStates.playerGrounded && !clicked)
                _interactionStates.playerIdle = true;
            else
                _interactionStates.playerIdle = false;

            ApplyExtraTurnRotation();

            if (_interactionStates.playerGrounded)
                HandleGroundedMovement(crouch, jump);
            else if (!_interactionStates.playerClimbing)
                HandleAirborneMovement();

            ScaleCapsuleForCrouching(crouch);
            PreventStandingInLowHeadroom();

            if (rifle)
            {
                if (_playerAnimator.GetBool("Rifle"))
                    StopRifle();
                else
                    StartRifle();
            }

            if (changed)
            {
                _interactionStates.playerChanging = true;
                ControllerChange();
            }
            else
                _interactionStates.playerChanging = false;

            if (climb && !crouch && !_interactionStates.playerClimbing)
            {
                _interactionStates.playerClimbing = true;
                _climbPos = _playerRigidbody.position.y;
                _playerRigidbody.useGravity = false;
                _interactionStates.playerGrounded = false;
                _playerAnimator.applyRootMotion = false;
            }
            else if (climb && _interactionStates.playerClimbing)
            {
                _interactionStates.playerClimbing = false;
                _playerRigidbody.useGravity = true;
                _playerAnimator.applyRootMotion = true;
            }

            Climber();
            UpdateAnimator(move);
        }
        //For animation events to change animator bools
        public void StartRifle()
        {
            if (!_playerAnimator) return;

            _playerAnimator.SetBool("Rifle", true);
        }
        public void StopRifle()
        {
            if (!_playerAnimator) return;

            _playerAnimator.SetBool("Rifle", false);
        }

        private void Climber()
        {
            if (!_interactionStates) return;
            if (!_interactionStates.playerClimbing) return;

            if (_interactionStates.rePos)
            {
                Vector3 playerPos;
                playerPos.x = _playerRigidbody.position.x;
                playerPos.y = _playerRigidbody.position.y;
                playerPos.z = _playerRigidbody.position.z;
                _interactionStates.targetPosition.y = playerPos.y;

                Quaternion playerRot = _playerRigidbody.rotation;

                if (Mathf.Abs(playerPos.x - _interactionStates.targetPosition.x) > 0.01f || Mathf.Abs(playerPos.z - _interactionStates.targetPosition.z) > 0.01f)
                {
                    playerPos.x = Mathf.MoveTowards(_playerRigidbody.position.x, _interactionStates.targetPosition.x, Time.fixedDeltaTime * 0.1f);
                    playerPos.z = Mathf.MoveTowards(_playerRigidbody.position.z, _interactionStates.targetPosition.z, Time.fixedDeltaTime * 0.1f);

                    playerRot = Quaternion.RotateTowards(playerRot, _interactionStates.targetRotation, Time.fixedDeltaTime * 2f);

                    if (_forwardAmount > 0)
                    {
                        _playerRigidbody.position = playerPos;
                        _playerRigidbody.rotation = playerRot;
                    }
                }
                else
                {
                    _interactionStates.rePos = false;
                }
            }

            if (_playerRigidbody.position.y + 0.75f > _interactionStates.targetTopPosition.y)
            {
                _playerRigidbody.velocity = Vector3.zero;

                Vector3 playerPos;
                playerPos.x = _playerRigidbody.position.x;
                playerPos.y = _playerRigidbody.position.y;
                playerPos.z = _playerRigidbody.position.z;

                Quaternion playerRot = _playerRigidbody.rotation;

                if (Mathf.Abs(playerPos.x - _interactionStates.targetTopPosition.x) > 0.1f || Mathf.Abs(playerPos.z - _interactionStates.targetTopPosition.z) > 0.1f)
                {
                    float moveForwardSpeed = 1f;
                    playerPos.x = Mathf.MoveTowards(_playerRigidbody.position.x, _interactionStates.targetTopPosition.x, Time.fixedDeltaTime * moveForwardSpeed);
                    playerPos.z = Mathf.MoveTowards(_playerRigidbody.position.z, _interactionStates.targetTopPosition.z, Time.fixedDeltaTime * moveForwardSpeed);

                    playerRot = Quaternion.RotateTowards(playerRot, _interactionStates.targetTopRotation, Time.fixedDeltaTime * 2f);

                    if (_forwardAmount > 0)
                    {
                        _playerRigidbody.position = playerPos;
                        _playerRigidbody.rotation = playerRot;
                    }
                }
                else
                {

                    _interactionStates.playerClimbed = true;
                    _playerAnimator.SetBool("Climb", false);
                    _playerRigidbody.useGravity = true;
                    _interactionStates.playerClimbing = false;
                    _playerAnimator.applyRootMotion = true;
                }
            }
            else if (_playerRigidbody.position.y < _climbPos)
            {
                _playerRigidbody.useGravity = true;
                _interactionStates.playerClimbing = false;
                _playerAnimator.applyRootMotion = true;
            }
            else 
                _playerRigidbody.velocity = new Vector3(0, climbSpeed * _forwardAmount, 0);
        }

        public void Dash()
        {
            if (!_playerRigidbody) return;

            _playerRigidbody.AddForce(10000f, 15000f, 0);
        }

        void ControllerChange()
        {
            if (!_interactionStates) return;

            if (_interactionStates.playerOnVehicle)
                ExitVehicle();
            else
                EnterVehicle();
        }

        public void EnterVehicle()
        {
            VehicleController _cc;
            VehicleBasicInput _vehicleinput;
            BikeController _bc;
            BikeBasicInput _bikeinput;
            Rigidbody _rb;
            GameObject enteredVehicle = _interactionStates.enteredVehicle;

            if (_cc = enteredVehicle.GetComponent<VehicleController>())
            {
                _vehicleinput = enteredVehicle.GetComponent<VehicleBasicInput>();
                _rb = enteredVehicle.GetComponent<Rigidbody>();

                _interactionStates.playerOnVehicle = true;
                _vehicleinput.enabled = true;
                _rb.isKinematic = false;
                _playerCapsuleCollider.enabled = false;
                _playerTransform.parent = _cc.sitPos;
                _playerTransform.position = _cc.sitPos.position;
                _playerTransform.rotation = _cc.sitPos.rotation;
                _playerRigidbody.isKinematic = true;
            }
            else if (_bc = enteredVehicle.GetComponent<BikeController>())
            {
                _bikeinput = enteredVehicle.GetComponent<BikeBasicInput>();

                _interactionStates.playerOnVehicle = true;
                _bikeinput.enabled = true;
                _playerCapsuleCollider.enabled = false;
                _playerTransform.parent = _bc.sitPos;
                _playerTransform.position = _bc.sitPos.position;
                _playerTransform.rotation = _bc.sitPos.rotation;
                _playerRigidbody.isKinematic = true;
            }
        }

        public void ExitVehicle()
        {
            VehicleBasicInput _vehicleinput;
            BikeBasicInput _bikeinput;
            Rigidbody _rb;
            GameObject enteredVehicle = _interactionStates.enteredVehicle;

            if (enteredVehicle.GetComponent<VehicleController>())
            {
                _vehicleinput = enteredVehicle.GetComponent<VehicleBasicInput>();
                _rb = enteredVehicle.GetComponent<Rigidbody>();

                _interactionStates.playerOnVehicle = false;
                _vehicleinput.enabled = false;
                _rb.isKinematic = true;
                _playerTransform.parent = null;
                _playerCapsuleCollider.enabled = true;
                _playerRigidbody.isKinematic = false;
            }
            else if (enteredVehicle.GetComponent<BikeController>())
            {
                _bikeinput = enteredVehicle.GetComponent<BikeBasicInput>();

                _interactionStates.playerOnVehicle = false;
                _bikeinput.enabled = false;
                _playerTransform.parent = null;
                _playerTransform.position += -_playerTransform.forward * 0.02f;
                _playerCapsuleCollider.enabled = true;
                _playerRigidbody.isKinematic = false;
            }

            _interactionStates.playerChangable = false;
        }

        private void ScaleCapsuleForCrouching(bool crouch)
        {
            if (_interactionStates.playerGrounded && crouch && !_interactionStates.playerClimbing)
            {
                if (_interactionStates.playerCrouching) return;
                _playerCapsuleCollider.height = _playerCapsuleCollider.height / 2f;
                _playerCapsuleCollider.center = _playerCapsuleCollider.center / 2f;
                _interactionStates.playerCrouching = true;
            }
            else if (!_interactionStates.playerClimbing)
            {
                Ray crouchRay = new Ray(_playerRigidbody.position + Vector3.up * _playerCapsuleCollider.radius * _half, Vector3.up);
                float crouchRayLength = (_capsuleHeight * charScaleY) - _playerCapsuleCollider.radius * _half;

                if (Physics.SphereCast(crouchRay, _playerCapsuleCollider.radius * _half, crouchRayLength, m_raycastLayerMaskforRagdoll.value, QueryTriggerInteraction.Ignore))
                {
                    _interactionStates.playerCrouching = true;
                    return;
                }
                _playerCapsuleCollider.height = _capsuleHeight;
                _playerCapsuleCollider.center = _capsuleCenter;
                _interactionStates.playerCrouching = false;
            }
        }

        private void OnDrawGizmos()
        {
            if (debugHeight)
            {
                Gizmos.DrawRay(_testHeightRay.origin, Vector3.up * _testHeightFloat);
            }
        }

        private void PreventStandingInLowHeadroom()
        {
            if (!_interactionStates.playerCrouching && !_interactionStates.playerClimbing)
            {
                Ray crouchRay = new Ray(_playerRigidbody.position + Vector3.up * _playerCapsuleCollider.radius * _half, Vector3.up);
                float crouchRayLength = (_capsuleHeight * charScaleY) - _playerCapsuleCollider.radius * _half;

                _testHeightRay = crouchRay;
                _testHeightFloat = crouchRayLength;

                if (Physics.SphereCast(crouchRay, _playerCapsuleCollider.radius * _half, crouchRayLength, m_raycastLayerMaskforRagdoll.value, QueryTriggerInteraction.Ignore))
                {
                    _interactionStates.playerCrouching = true;
                }
            }
        }

        private void UpdateAnimator(Vector3 move)
        {
            _playerAnimator.SetFloat("Forward", _forwardAmount, 0.1f, Time.deltaTime);
            if (!_interactionStates.playerClimbing)
            {
                _playerAnimator.SetFloat("Turn", _turnAmount, 0.1f, Time.deltaTime);
            }
            _playerAnimator.SetBool("Crouch", _interactionStates.playerCrouching);
            _playerAnimator.SetBool("OnGround", _interactionStates.playerGrounded);
            _playerAnimator.SetBool("Climb", _interactionStates.playerClimbing);
            if (!_interactionStates.playerGrounded)
            {
                _playerAnimator.SetFloat("Jump", _playerRigidbody.velocity.y);
            }

            float runCycle = Mathf.Repeat(
                    _playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
            float jumpLeg = (runCycle < _half ? 1 : -1) * _forwardAmount;

            if (_interactionStates.playerGrounded) _playerAnimator.SetFloat("JumpLeg", jumpLeg);

            if (_interactionStates.playerGrounded && move.magnitude > 0)
                _playerAnimator.speed = animSpeedMultiplier;
            else
                _playerAnimator.speed = 1;
        }

        private void HandleAirborneMovement()
        {
            Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
            _playerRigidbody.AddForce(extraGravityForce);
            m_GroundCheckDistance = _playerRigidbody.velocity.y < 0 ? _defaultGroundCheckDistance : 0.01f;
        }

        private void HandleGroundedMovement(bool crouch, bool jump)
        {
            if (jump && !crouch && !_interactionStates.playerClimbing && _playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
            {
                _playerRigidbody.velocity = new Vector3(_playerRigidbody.velocity.x, m_JumpPower, _playerRigidbody.velocity.z);
                _interactionStates.playerGrounded = false;
                _playerAnimator.applyRootMotion = false;
                m_GroundCheckDistance = 0.1f;
            }
        }

        private void ApplyExtraTurnRotation()
        {
            float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, _forwardAmount);
            transform.Rotate(0, _turnAmount * turnSpeed * Time.deltaTime, 0);
        }

        public void OnAnimatorMove()
        {
            if (!_interactionStates) return;
            if (_playerRigidbody.isKinematic && _interactionStates.enteredVehicle) return;

            if (_interactionStates.playerGrounded && !_interactionStates.playerClimbing && Time.deltaTime > 0)
            {
                Vector3 moveForward = transform.forward * _playerAnimator.GetFloat("motionZ") * Time.deltaTime;
                Vector3 v = ((_playerAnimator.deltaPosition + moveForward) * moveSpeedMultiplier) / Time.deltaTime;
                v.y = _playerRigidbody.velocity.y;
                _playerRigidbody.velocity = v;
            }
        }

        private void CheckGroundStatus()
        {
            RaycastHit hitInfo;

            if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
            {
                _groundNormal = hitInfo.normal;
                _interactionStates.playerGrounded = true;
                _playerAnimator.applyRootMotion = true;
            }
            else
            {
                _interactionStates.playerGrounded = false;
                _groundNormal = Vector3.up;
                _playerAnimator.applyRootMotion = false;
            }
        }
    }
}
