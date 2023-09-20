using UnityEngine;

namespace razz
{
    public class FirstPersonController : MonoBehaviour
    {
        public float speed = 2f;
        public float crouchHeight = 0.1f;

        private float _moveX, _moveZ;
        private bool _crouch, _crouching;
        private float _defaultCapsuleHeight;
        private bool _knife, _sniper;
        private int _knifeAttack;
        private bool _attack;
        private Animator _animator;
        private CapsuleCollider _playerCapsule;
        private Interactor _interactor;

        private void Start()
        {
            _interactor = GetComponentInChildren<Interactor>();
            _animator = GetComponentInChildren<Animator>();
            _playerCapsule = GetComponentInChildren<CapsuleCollider>();
            if (_playerCapsule)
            {
                _defaultCapsuleHeight = _playerCapsule.height;
            }
        }
        private void Update()
        {
            _moveX = BasicInput.GetHorizontal();
            _moveZ = BasicInput.GetVertical();
            _attack = BasicInput.GetLeftClick();
            _crouch = BasicInput.GetCrouch();

            Attack();
        }
        private void FixedUpdate()
        {
            transform.Translate(_moveX * Time.fixedDeltaTime * speed, 0, _moveZ * Time.fixedDeltaTime * speed);

            if (_playerCapsule) Crouch();
        }

        private void Crouch()
        {
            if (_crouch && !_crouching)
            {
                _playerCapsule.height = crouchHeight;
                _crouching = true;
            }

            if (_crouching && !_crouch)
            {
                _playerCapsule.height = _defaultCapsuleHeight;
                _crouching = false;
            }
        }

        public void Attack()
        {
            if (_attack && _animator && _knife && _knifeAttack == 0)
            {
                _knifeAttack = Random.Range(1, 4);
                _animator.SetInteger("knifeAttack", _knifeAttack);
            }
        }
        //Called by knife animation events
        public void ResetKnifeAttack()
        {
            _knifeAttack = 0;
            _animator.SetInteger("knifeAttack", _knifeAttack);
        }

        public void KnifePick()
        {
            _knife = !_knife;
        }

        public void SniperPick()
        {
            _sniper = !_sniper;
            if (_animator)
            {
                _animator.SetBool("sniper", _sniper);
            }
        }

        public void DropWeapons()
        {
            if (_sniper || _knife)
            {
                _interactor.DisconnectAll();
            }
        }
    }
}
