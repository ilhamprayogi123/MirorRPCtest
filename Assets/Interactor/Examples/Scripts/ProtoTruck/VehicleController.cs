using UnityEngine;

namespace razz
{
    public class VehicleController : MonoBehaviour
    {
        private Quaternion[] _wheelMeshLocalRotations;
        private float _steerAngle;
        private int _gearNum;
        private float _gearFactor;
        private float _currentTorque;
        private Rigidbody _rigidbody;

        [SerializeField] private WheelCollider[] _wheelColliders = new WheelCollider[4];
        [SerializeField] private GameObject[] _wheelMeshes = new GameObject[4];
        [SerializeField] private float _maxHandbrakeTorque;
        [SerializeField] private float _topspeed = 200;
        [SerializeField] private static int _gears = 5;
        [SerializeField] private float _revRangeBoundary = 1f;

        [HideInInspector] public float steerAmount;
        [HideInInspector] public float accelAmount;

        public Vector3 m_CentreOfMassOffset;
        public float m_MaximumSteerAngle;
        [Range(0, 1)] public float m_TractionControl;
        public float m_FullTorqueOverAllWheels;
        public float m_ReverseTorque;
        public float m_SlipLimit;
        public float m_BrakeTorque;
        [Tooltip("This is for player parenting and repositioning when using this object.")]
        public Transform sitPos;

        public float BrakeInput { get; private set; }
        public float CurrentSpeed { get { return _rigidbody.velocity.magnitude * 2.23693629f; } }
        public float MaxSpeed { get { return _topspeed; } }
        public float Revs { get; private set; }
        public float AccelInput { get; private set; }

        private void Start()
        {
            _wheelMeshLocalRotations = new Quaternion[_wheelColliders.Length];

            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                _wheelMeshLocalRotations[i] = _wheelMeshes[i].transform.localRotation;
            }

            _wheelColliders[0].attachedRigidbody.centerOfMass = m_CentreOfMassOffset;
            _maxHandbrakeTorque = float.MaxValue;
            _rigidbody = GetComponent<Rigidbody>();
            _currentTorque = m_FullTorqueOverAllWheels - (m_TractionControl * m_FullTorqueOverAllWheels);
        }

        private void GearChanging()
        {
            float f = Mathf.Abs(CurrentSpeed / MaxSpeed);
            float upgearlimit = (1 / (float)_gears) * (_gearNum + 1);
            float downgearlimit = (1 / (float)_gears) * _gearNum;

            if (_gearNum > 0 && f < downgearlimit)
            {
                _gearNum--;
            }

            if (f > upgearlimit && (_gearNum < (_gears - 1)))
            {
                _gearNum++;
            }
        }

        private static float CurveFactor(float factor)
        {
            return 1 - (1 - factor) * (1 - factor);
        }

        private static float ULerp(float from, float to, float value)
        {
            return (1.0f - value) * from + value * to;
        }

        private void CalculateGearFactor()
        {
            float f = (1 / (float)_gears);
            var targetGearFactor = Mathf.InverseLerp(f * _gearNum, f * (_gearNum + 1), Mathf.Abs(CurrentSpeed / MaxSpeed));

            _gearFactor = Mathf.Lerp(_gearFactor, targetGearFactor, Time.deltaTime * 5f);
        }

        private void CalculateRevs()
        {
            CalculateGearFactor();

            var gearNumFactor = _gearNum / (float)_gears;
            var revsRangeMin = ULerp(0f, _revRangeBoundary, CurveFactor(gearNumFactor));
            var revsRangeMax = ULerp(_revRangeBoundary, 1f, gearNumFactor);

            Revs = ULerp(revsRangeMin, revsRangeMax, _gearFactor);
        }

        public void Move(float steering, float accel, float footbrake, float handbrake)
        {
            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                Quaternion quat;
                Vector3 position;
                _wheelColliders[i].GetWorldPose(out position, out quat);
                _wheelMeshes[i].transform.position = position;
                _wheelMeshes[i].transform.rotation = quat;
            }

            //For pedal animation
            accelAmount = Mathf.Clamp(accel, -1, 1);

            steering = Mathf.Clamp(steering, -1, 1);
            AccelInput = accel = Mathf.Clamp(accel, 0, 1);
            BrakeInput = footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
            handbrake = Mathf.Clamp(handbrake, 0, 1);

            _steerAngle = steering * m_MaximumSteerAngle;
            _wheelColliders[0].steerAngle = _steerAngle;
            _wheelColliders[1].steerAngle = _steerAngle;

            //For steering wheel animation
            steerAmount = steering;

            ApplyDrive(accel, footbrake);
            CapSpeed();

            if (handbrake > 0f)
            {
                var hbTorque = handbrake * _maxHandbrakeTorque;
                _wheelColliders[2].brakeTorque = hbTorque;
                _wheelColliders[3].brakeTorque = hbTorque;
            }

            CalculateRevs();
            GearChanging();
            TractionControl();
        }

        private void CapSpeed()
        {
            float speed = _rigidbody.velocity.magnitude;

            speed *= 3.6f;

            if (speed > _topspeed)
                _rigidbody.velocity = (_topspeed / 3.6f) * _rigidbody.velocity.normalized;
        }

        private void ApplyDrive(float accel, float footbrake)
        {
            float thrustTorque = accel * (_currentTorque / 4f);

            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                _wheelColliders[i].motorTorque = thrustTorque;
            }

            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, _rigidbody.velocity) < 50f)
                {
                    _wheelColliders[i].brakeTorque = m_BrakeTorque * footbrake;
                }
                else if (footbrake > 0)
                {
                    _wheelColliders[i].brakeTorque = 0f;
                    _wheelColliders[i].motorTorque = -m_ReverseTorque * footbrake;
                }
            }
        }

        private void TractionControl()
        {
            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                _wheelColliders[i].GetGroundHit(out WheelHit wheelHit);
                AdjustTorque(wheelHit.forwardSlip);
            }
        }

        private void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= m_SlipLimit && _currentTorque >= 0)
            {
                _currentTorque -= 10 * m_TractionControl;
            }
            else
            {
                _currentTorque += 10 * m_TractionControl;
                if (_currentTorque > m_FullTorqueOverAllWheels)
                {
                    _currentTorque = m_FullTorqueOverAllWheels;
                }
            }
        }
    }
}
