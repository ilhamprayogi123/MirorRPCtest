using UnityEngine;

namespace razz
{
    //Handler class for Turret attack. Targeting both turrets and lights with different timings,
    //firing, playing sound and particle effects. Uses Auto class for targeting.
    public class TurretAim : MonoBehaviour
    {
        private Animation _animation;
        private Transform _target;
        private bool _available = true;
        private AudioSource _audio;

        public Transform turretGun;
        public Transform turretLightBase;
        public Transform turretLight;
        public ParticleSystem particles;
        public bool locked = false;
        public Interactor.FullBodyBipedEffector effector;
        public float force = 1f;

        private void Start()
        {
            if (!(_animation = GetComponent<Animation>()))
            {
                Debug.Log("No animator component on Turret: " + this.name);
            }

            if (turretGun == null || turretLightBase == null || turretLight == null)
            {
                Debug.Log("Turret Aim gameobject or gameobjects are not assigned: " + this.name);
            }

            _audio = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (locked)
            {
                _animation.Play();
                if (_audio != null)
                {
                    _audio.Play();
                }
                if (particles != null)
                {
                    particles.Play();
                }
                PushTarget();
                locked = false;
                _available = true;
            }
        }

        public void Attack(Transform target)
        {
            if (!_available) return;

            _available = false;
            _target = target;
            Vector3 direction = target.position - turretGun.position;
            Quaternion look = Quaternion.LookRotation(direction, turretGun.up);
            
            LightAim(look);
            Fire(look);
        }

        private void PushTarget()
        {
            _target.GetComponent<Rigidbody>().AddForce((_target.transform.position - turretGun.position) * force, ForceMode.Impulse);
            Debug.DrawLine(_target.transform.position, turretGun.position, Color.red, 3f);
        }

        public void Fire(Quaternion look)
        {
            StartCoroutine(turretGun.RotateToGlobal(look, 1f, Ease.QuadIn, this));
        }

        public void LightAim(Quaternion look)
        {
            StartCoroutine(turretLightBase.RotateToGlobal(look, 0.5f, Ease.CubeIn));
        }
    }
}
