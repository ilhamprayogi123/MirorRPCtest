using UnityEngine;

namespace razz
{
    //Handler class for hit interaction. It rotates itself when enters player sphere, repositions its target
    //for a hit position(pulling hand back), moves target back to original position on object and deals force when hit happens.
    [HelpURL("https://negengames.com/interactor/components.html#hithandlercs")]
    [DisallowMultipleComponent]
    public class HitHandler : MonoBehaviour
    {
        private Rigidbody _hitObjRB;
        private Transform _playerTransform;
        private Vector3 _playerPosAdjustY;
        private Vector3 _interactionTargetDefaultPos;
        private Vector3 _tempVector3calc;

        public InteractorObject hitObj;
        [Tooltip("Hit position height offset")]
        public float yOffset = 0.1f;
        [Tooltip("Distance modifier between effector position and target.")] [Range(0, 2)]
        public float distancePercentage = 0.9f;
        [Tooltip("The angle between effector-default target line and new position.")]
        public float angle = 35f;
        [Tooltip("Amount of force that will be applied to InteractorObject when hit happens")]
        public float hitForce = 10f;

        [HideInInspector] public bool hitForceOnce;
        [HideInInspector] public bool moveOnce;
        [HideInInspector] public bool hitDone;
        
        private void Start()
        {
            if (!Init())
            {
                this.enabled = false;
            }
        }

        private bool Init()
        {
            if (hitObj == null)
            {
                if (!(hitObj = GetComponentInParent<InteractorObject>()))
                {
                    Debug.Log("Hit handler of " + this.name + " could not find its interaction object.");
                    return false;
                }
            }

            if (!(_hitObjRB = hitObj.GetComponent<Rigidbody>()))
            {
                Debug.Log("Hit handler of " + this.name + " could not find Rigidbody component on its interaction object rigidbody.");
                return false;
            }

            return true;
        }

        public void HitHandlerRotate(Interactor interactor)
        {
            _playerTransform = interactor.transform;
            hitForceOnce = false;
            moveOnce = false;
            hitDone = false;

            _playerPosAdjustY = _playerTransform.position;
            _playerPosAdjustY.y = transform.position.y;

            transform.rotation = Quaternion.LookRotation(_playerPosAdjustY - transform.position, Vector3.up);
        }

        public void HitPosMove(Transform interactionTarget, Vector3 effectorPos)
        {
            _interactionTargetDefaultPos = interactionTarget.position;
            effectorPos.y = interactionTarget.position.y + yOffset;

            _tempVector3calc = Quaternion.AngleAxis(angle, Vector3.up) * (interactionTarget.position - effectorPos) * distancePercentage;
            
            Debug.DrawLine(interactionTarget.position, interactionTarget.position - _tempVector3calc, Color.blue, 5f);

            interactionTarget.position -= _tempVector3calc;
        }

        public void HitPosDefault(Transform interactionTarget, Vector3 effectorPos)
        {
            if (Vector3.Distance(interactionTarget.position, _interactionTargetDefaultPos) > 0.01f)
            {
                //Auto.MoveTo(interactionTarget, _interactionTargetDefaultPos, 0.5f, Ease.QuadIn);
                interactionTarget.position = Vector3.MoveTowards(interactionTarget.position, _interactionTargetDefaultPos, Time.deltaTime *5);
            }
            else
            {
                hitDone = true;
                HitForce(interactionTarget, effectorPos);
            }
        }

        public void HitForce(Transform interactionTarget, Vector3 effectorPos)
        {
            _hitObjRB.AddForceAtPosition((interactionTarget.position - effectorPos) * hitForce, _interactionTargetDefaultPos);
        }
    }
}
