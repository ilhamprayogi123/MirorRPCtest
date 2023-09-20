using UnityEngine;

namespace razz
{
    //Gas and brake pedal rotation for ProtoTruck
    public class Pedals : MonoBehaviour
    {
        private VehicleController _vehicleController;
        private VehicleBasicInput _vehicleInput;
        private float _defaultRot;
        private float _calculateX;
        private Vector3 _calculateXYZ;

        [SerializeField] private float _maxRot = 30f;

        public bool brake = false;

        private void Awake()
        {
            if (!(_vehicleController = GetComponentInParent<VehicleController>()))
            {
                Debug.Log("There is no VehicleController in parent of Pedals");
                return;
            }

            if (!(_vehicleInput = GetComponentInParent<VehicleBasicInput>()))
            {
                Debug.Log("There is no VehicleBasicInput in parent of SteeringWheel");
                return;
            }

            _defaultRot = this.transform.localRotation.eulerAngles.x;
        }

        private void Update()
        {
            if (!_vehicleInput.isActiveAndEnabled) return;

            if (_vehicleController.accelAmount < 0 && brake)
            {
                _calculateX = Mathf.Lerp(_defaultRot, _defaultRot + _maxRot, -_vehicleController.accelAmount * Time.timeScale);
            }
            else if (_vehicleController.accelAmount > 0 && !brake)
            {
                _calculateX = Mathf.Lerp(_defaultRot, _defaultRot + _maxRot, _vehicleController.accelAmount * Time.timeScale);
            }
            else
            {
                _calculateX = _defaultRot;
            }
            
            _calculateXYZ = this.transform.localRotation.eulerAngles;
            _calculateXYZ.x = _calculateX;
            this.transform.localEulerAngles = _calculateXYZ;
        }
    }
}
