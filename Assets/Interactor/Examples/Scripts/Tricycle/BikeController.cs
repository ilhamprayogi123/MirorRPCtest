using UnityEngine;

namespace razz
{
    public class BikeController : MonoBehaviour
    {
        private Quaternion[] _wheelMeshLocalRotations;
        private float _steerAngle;
        private int _gearNum;
        private float _gearFactor;
        private float _currentTorque;
        private Rigidbody _rigidbody;
        private float _steerRotHandle;

        [SerializeField] private WheelCollider[] _wheelColliders = new WheelCollider[3];
        [SerializeField] private GameObject[] _wheelMeshes = new GameObject[3];
        [Tooltip("This pedals will rotate with front wheel and remains with its same Vector3.Up for feet placement.")]
        [SerializeField] private GameObject[] _pedals = new GameObject[2];
        [SerializeField] private float _topspeed = 200;
        [SerializeField] private static int _gears = 1;
        [SerializeField] private float _revRangeBoundary = 1f;

        [HideInInspector] public float accelAmount;

        [Tooltip("This is bike handle parent to rotate when steering.")]
        public GameObject handles;
        public Vector3 centreOfMassOffset;
        public float maximumSteerAngle;
        [Range(0, 1)] public float tractionControl;
        public float fullTorqueOverAllWheels;
        public float reverseTorque;
        public float slipLimit;
        public float brakeTorque;
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
                _wheelColliders[i].ConfigureVehicleSubsteps(20, 20, 1);
            }

            _wheelColliders[0].attachedRigidbody.centerOfMass = centreOfMassOffset;
            _rigidbody = GetComponent<Rigidbody>();
            _currentTorque = fullTorqueOverAllWheels - (tractionControl * fullTorqueOverAllWheels);
        }

        private void FixedUpdate()
        {
            handles.transform.localEulerAngles = new Vector3(handles.transform.localEulerAngles.x, _steerRotHandle, handles.transform.localEulerAngles.z);

            for (int i = 0; i < _pedals.Length; i++)
            {
                _pedals[i].transform.eulerAngles = new Vector3(handles.transform.eulerAngles.x, handles.transform.eulerAngles.y, handles.transform.eulerAngles.z);
            }
        }

        private void GearChanging()
        {
            float f = Mathf.Abs(CurrentSpeed / MaxSpeed);
            float upgearlimit = (1 / (float)_gears) * (_gearNum + 1);
            float downgearlimit = (1 / (float)_gears) * _gearNum;

            if (_gearNum > 0 && f < downgearlimit)
                _gearNum--;

            if (f > upgearlimit && (_gearNum < (_gears - 1)))
                _gearNum++;
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

        public void Move(float steering, float accel, float footbrake)
        {
            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                Quaternion quat;
                Vector3 position;
                _wheelColliders[i].GetWorldPose(out position, out quat);
                _wheelMeshes[i].transform.position = position;
                _wheelMeshes[i].transform.rotation = quat;
            }

            _steerRotHandle = steering * maximumSteerAngle;
            steering = Mathf.Clamp(steering, -1, 1);
            AccelInput = accel = Mathf.Clamp(accel, 0, 1);
            BrakeInput = footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
            _steerAngle = steering * maximumSteerAngle;
            _wheelColliders[0].steerAngle = _steerAngle;

            ApplyDrive(accel, footbrake);
            CapSpeed();
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
            float thrustTorque = accel * (_currentTorque / 3f);
            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                _wheelColliders[i].motorTorque = thrustTorque;
            }

            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, _rigidbody.velocity) < 50f)
                {
                    _wheelColliders[i].brakeTorque = brakeTorque * footbrake;
                }
                else if (footbrake > 0)
                {
                    _wheelColliders[i].brakeTorque = 0f;
                    _wheelColliders[i].motorTorque = -reverseTorque * footbrake;
                }
            }
        }

        private void TractionControl()
        {
            WheelHit wheelHit;
            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                _wheelColliders[i].GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);
            }
        }

        private void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= slipLimit && _currentTorque >= 0)
            {
                _currentTorque -= 10 * tractionControl;
            }
            else
            {
                _currentTorque += 10 * tractionControl;
                if (_currentTorque > fullTorqueOverAllWheels)
                {
                    _currentTorque = fullTorqueOverAllWheels;
                }
            }
        }
    }
}
