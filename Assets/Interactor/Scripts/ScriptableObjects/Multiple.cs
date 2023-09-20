using UnityEngine;

namespace razz
{
    [CreateAssetMenu(fileName = "MultipleSettings", menuName = "Interactor/MultipleSettings")]
    public class Multiple : InteractionTypeSettings
    {
        [Tooltip("Before using, sets player forward to body target forward.")]
        public bool setPlayerDirection = true;
        [Tooltip("Before using, sets player position to body target position.")]
        public bool setPlayerPosition = true;
        [Tooltip("Disables player collider (on Interactor gameobject only) while using.")]
        public bool turnColliderPlayerOff = false;
        [Tooltip("Disables object collider (on InteractorObject only) while using.")]
        public bool turnObjColliderOff = true;

        private MonoBehaviour _bodyTarget;
        private Transform _playerTransform;
        private Collider _playerCollider;
        private Collider _col;
        private Rigidbody _playerRigidbody;
        private Vector3 _cachedPosition;

        public override void Init(InteractorObject interactorObject)
        {
            base.Init(interactorObject);

            _col = _intObj.col;
            _bodyTarget = _intObj.GetTargetForEffectorType((int)Interactor.FullBodyBipedEffector.Body);

            if ((!_bodyTarget && setPlayerDirection) || (!_bodyTarget && setPlayerPosition)) Debug.Log(_intObj.name + " Interactor Object (Interaction Type: Multiple) has set for body direction but has no body target.");
            if (!_col) Debug.Log(_intObj.name + " has no collider!");
        }

        public void MultipleIn(Interactor interactor)
        {
            if (setPlayerDirection) SetPlayerDir(interactor);
            if (setPlayerPosition) SetPlayerPos(interactor);
            if (turnColliderPlayerOff) TurnPlayerColOff(interactor);
            if (turnObjColliderOff) TurnObjColOff();
        }
        public void MultipleOut(Interactor interactor)
        {
            if (setPlayerPosition) SetBodyPosBack(interactor);
            if (turnColliderPlayerOff) TurnPlayerColOn(interactor);
            if (turnObjColliderOff) TurnObjColOn();
        }

        private void SetPlayerDir(Interactor interactor)
        {
            if (_bodyTarget)
            {
                _playerTransform = interactor.playerTransform;
                Quaternion targetRotation = Quaternion.Euler(_playerTransform.eulerAngles.x, _bodyTarget.transform.eulerAngles.y, _playerTransform.eulerAngles.z);
                Quaternion deltaRotation = targetRotation * Quaternion.Inverse(_playerTransform.rotation);
                _playerTransform.rotation *= deltaRotation;
            }
        }
        private void SetPlayerPos(Interactor interactor)
        {
            _playerTransform = interactor.playerTransform;
            Vector3 temp = _bodyTarget.transform.position;
            temp.y = _playerTransform.position.y;
            _cachedPosition = _playerTransform.position - _intObj.transform.position;
            if (_bodyTarget) _playerTransform.position = temp;
        }
        private void SetBodyPosBack(Interactor interactor)
        {
            if (_bodyTarget)
            {
                _playerTransform = interactor.playerTransform;
                _playerTransform.position = _intObj.transform.position + _cachedPosition;
            }
        }

        private void TurnPlayerColOff(Interactor interactor)
        {
            _playerCollider = interactor.playerCollider;
            _playerRigidbody = interactor.playerRigidbody;

            if (_playerCollider && _playerRigidbody)
            {
                _playerRigidbody.isKinematic = true;
                _playerCollider.enabled = false;
            }
        }
        private void TurnPlayerColOn(Interactor interactor)
        {
            _playerCollider = interactor.playerCollider;
            _playerRigidbody = interactor.playerRigidbody;

            if (_playerCollider && _playerRigidbody)
            {
                _playerRigidbody.isKinematic = false;
                _playerCollider.enabled = true;
            }
        }
        private void TurnObjColOff()
        {
            if (_col) _col.enabled = false;
        }
        private void TurnObjColOn()
        {
            if (_col) _col.enabled = true;
        }
    }
}
