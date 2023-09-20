using UnityEngine;

namespace razz
{
    //Rotation for steering wheel based on VehicleController steerAmount
    public class SteeringWheel : MonoBehaviour
    {
        private VehicleController _vehicleController;
        private VehicleBasicInput _vehicleInput;
        private float _defaultRot;
        private float _calculateZ;
        private Vector3 _calculateXYZ;

        [SerializeField] private float _maxRot = 90f;

        private void Awake()
        {
            if (!(_vehicleController = GetComponentInParent<VehicleController>()))
            {
                Debug.Log("There is no VehicleController in parent of SteeringWheel");
                return;
            }

            if (!(_vehicleInput = GetComponentInParent<VehicleBasicInput>()))
            {
                Debug.Log("There is no VehicleBasicInput in parent of SteeringWheel");
                return;
            }

            _defaultRot = this.transform.localRotation.eulerAngles.z;
        }

        private void Update()
        {
            if (!_vehicleInput.isActiveAndEnabled) return;

            if (_vehicleController.steerAmount < 0)
            {
                _calculateZ = Mathf.Lerp(_defaultRot, _defaultRot + _maxRot, -_vehicleController.steerAmount * Time.timeScale);
            }
            else
            {
                _calculateZ = Mathf.Lerp(_defaultRot, _defaultRot - _maxRot, _vehicleController.steerAmount * Time.timeScale);
            }
            
            _calculateXYZ = this.transform.localRotation.eulerAngles;
            _calculateXYZ.z = _calculateZ;
            this.transform.localEulerAngles = _calculateXYZ;
        }
    }
}
