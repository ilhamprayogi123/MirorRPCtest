using UnityEngine;

namespace razz
{
    [CreateAssetMenu(fileName = "TouchSettings", menuName = "Interactor/TouchSettings")]
    public class Touch : InteractionTypeSettings
    {
        [Header("Vertical Settings")]
        [Tooltip("Height amount for TouchVertical raycast")]
        public float touchHeight = 0.85f;
        [Tooltip("Forward amount for TouchVertical raycast")]
        public float touchForward = 0.2f;
        [Tooltip("Target rotation for TouchVertical")]
        public Vector3 touchRotations = new Vector3(0, 0, -50f);
        [Tooltip("Ray lenght for TouchVertical raycast")]
        public float touchRayLength = 0.45f;
        [Tooltip("Target lerp between raycast hit and effector position for TouchVertical")]
        [Range(0, 1)]
        public float touchLerp = 0.06f;
        [Tooltip("Time needs to pass before TouchVertical starts, to prevent stutter.")]
        public float touchVCooldown = 0.2f;

        [Header("Horizontal Settings")]
        [Tooltip("Forward amount for TouchHorizontal raycast (Multiplied by 3 to check earlier if upper collider ends but puts target on this forward amount.)")]
        public float touchHorizontalForward = 0.3f;
        [Tooltip("Right amount for TouchHorizontal raycast")]
        public float touchHorizontalRight = 0.1f;
        [Tooltip("Ray lenght for TouchHorizontal raycast")]
        public float touchHorizontalRayLenght = 2f;
        [Tooltip("Time needs to pass before TouchHorizontal starts, to prevent stutter.")]
        public float touchHCooldown = 0.2f;

        [HideInInspector] public float defaultTouchVCooldown;
        [HideInInspector] public float defaultTouchHCooldown;

        private LayerMask _layerMask;
        private Vector3 _touchRotationsTemp;

        public override void Init(InteractorObject interactorObject)
        {
            base.Init(interactorObject);
            defaultTouchHCooldown = touchHCooldown;
            defaultTouchVCooldown = touchVCooldown;
        }

        //Resets cooldown timers when touch starts
        public void ResetTouchCooldowns()
        {
            touchHCooldown = defaultTouchHCooldown;
            touchVCooldown = defaultTouchVCooldown;
        }

        public bool RaycastVertical(Transform playerTransform, out RaycastHit hit, Collider col, bool left)
        {
            int leftSide = -1;
            if (!left) leftSide = 1;
            GetLayerMask();

            Physics.Raycast(playerTransform.position + new Vector3(0, touchHeight, 0) + playerTransform.forward * touchForward, playerTransform.right * leftSide, out hit, touchRayLength, _layerMask);

            if (hit.collider == col)
                return true;
            else
                return false;
        }
        public void ReposForVertical(Vector3 effectorWorldSpace, RaycastHit hit, Transform target, bool left)
        {
            int leftSide = -1;
            if (!left) leftSide = 1;

            target.position = Vector3.Lerp(hit.point, effectorWorldSpace, touchLerp);

            Vector3 rotateAxis = Vector3.Cross(hit.normal, Vector3.forward);
            float rotateAngle = Vector3.Angle(-hit.normal, Vector3.forward);
            if (rotateAngle == 180) rotateAxis = Vector3.up;
            target.rotation = Quaternion.AngleAxis(rotateAngle, rotateAxis);

            Vector3 tempRot = target.rotation.eulerAngles;

            _touchRotationsTemp = touchRotations;
            if (_touchRotationsTemp.x < 1f && _touchRotationsTemp.x > -1f)
                _touchRotationsTemp.x = 1f;
            if (_touchRotationsTemp.y < 1f && _touchRotationsTemp.y > -1f)
                _touchRotationsTemp.y = 1f;
            if (_touchRotationsTemp.z < 1f && _touchRotationsTemp.z > -1f)
                _touchRotationsTemp.z = 1f;

            target.rotation = Quaternion.Euler(new Vector3(_touchRotationsTemp.x * tempRot.x, _touchRotationsTemp.y * tempRot.y, _touchRotationsTemp.z * leftSide));
        }

        public bool RaycastHorizontalUp(Transform playerTransform, out RaycastHit hit, Collider col)
        {
            GetLayerMask();
            Physics.Raycast(playerTransform.position + playerTransform.forward * touchHorizontalForward * 3f + playerTransform.right * touchHorizontalRight, Vector3.up, out hit, touchHorizontalRayLenght, _layerMask);

            if (hit.collider == col)
                return true;
            else
                return false;
        }
        public void ReposForHorizontalUp(Transform playerTransform, RaycastHit hit, Transform target)
        {
            Vector3 handNewPos = hit.point - playerTransform.forward * touchHorizontalForward * 2f;
            handNewPos.y = target.position.y;
            target.position = handNewPos;
            Vector3 tempRot = playerTransform.rotation.eulerAngles;
            Vector3 tempRot2 = target.transform.rotation.eulerAngles;
            target.rotation = Quaternion.Euler(new Vector3(tempRot2.x, tempRot.y, tempRot2.z));
        }

        private void GetLayerMask()
        {
            if (_layerMask == 0)
            {
                _layerMask = _intObj.currentInteractor.GetPlayerLayerMask();
            }
        }

        private void OnDisable()
        {
            ResetTouchCooldowns();
        }
    }
}
