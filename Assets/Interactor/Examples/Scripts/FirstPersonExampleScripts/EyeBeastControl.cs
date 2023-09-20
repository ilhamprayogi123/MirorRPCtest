using UnityEngine;

namespace razz
{
    public class EyeBeastControl : MonoBehaviour
    {
        public float blendSpeed = 2f;
        public Transform playerCamTransform;
        public EaseType easeType;
        public InteractorObject attackInteractorObject;
        public InteractorObject leftHandInteractorObject;
        public InteractorObject rightHandInteractorObject;
        public Interactor eyebeastInteractor;
        public Transform jumpPosition;
        public Rigidbody playerRigidbody;

        private SkinnedMeshRenderer _sMeshRenderer;
        private bool _getAngry, _angryDone, _jumpStarted, _jumpDone, _attacked;
        private float _blendValue = 100;

        private void Start()
        {
            if (!(_sMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>()))
            {
                Debug.Log("There is no SkinnedMeshRenderer on EyeBeast!");
            }
        }
        private void Update()
        {
            if (_getAngry && !_angryDone)
            {
                _blendValue -= Time.deltaTime * blendSpeed;
                if (_blendValue < 0)
                {
                    _angryDone = true;
                    return;
                }
                _sMeshRenderer.SetBlendShapeWeight(0, _blendValue);
                attackInteractorObject.lookWeight += Time.deltaTime;
            }

            if (_angryDone && !_jumpStarted)
            {
                JumpOnPlayer();
                _jumpStarted = true;
            }

            if (_jumpStarted)
            {
                transform.position = Vector3.MoveTowards(transform.position, jumpPosition.position, Time.deltaTime * 10f);
                transform.rotation = Quaternion.LookRotation(jumpPosition.forward, playerCamTransform.up);

                if (!_jumpDone)
                {
                    if ((transform.localPosition - jumpPosition.position).sqrMagnitude < 0.04f)
                    {
                        _jumpDone = true;
                        if (playerRigidbody)
                        {
                            playerRigidbody.AddForce(jumpPosition.forward, ForceMode.Impulse);
                        }
                    }
                }
            }

            if (_jumpDone && !_attacked)
            {
                StartAttack();
                _attacked = true;
            }
        }

        public void GetAngry()
        {
            _getAngry = true;
        }

        private void JumpOnPlayer()
        {
            if (!playerCamTransform)
            {
                Debug.Log("There is no Player Camera Transform on: " + this.name);
            }

            transform.rotation = Quaternion.LookRotation(-jumpPosition.forward, playerCamTransform.up);
        }

        public void StartAttack()
        {
            if (!attackInteractorObject || !leftHandInteractorObject || !rightHandInteractorObject)
            {
                Debug.Log("Missing target InteractorObjects on: " + this.name);
                return;
            }
            if (!eyebeastInteractor)
            {
                Debug.Log("There is no Interactor on: " + this.name);
                return;
            }

            eyebeastInteractor.StartStopInteraction(attackInteractorObject);
            leftHandInteractorObject.enabled = true;
            rightHandInteractorObject.enabled = true;
        }
    }
}
