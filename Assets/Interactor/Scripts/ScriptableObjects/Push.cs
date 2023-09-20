using UnityEngine;

namespace razz
{
    [CreateAssetMenu(fileName = "PushSettings", menuName = "Interactor/PushSettings")]
    public class Push : InteractionTypeSettings
    {
        [HideInInspector] public bool pushed;

        private Transform _parentTransform;
        private Collider _col;

        public override void Init(InteractorObject interactorObject)
        {
            base.Init(interactorObject);

            _col = _intObj.col;
            _parentTransform = _intObj.transform.parent;

            if (!_col) Debug.Log(_intObj.name + " has no collider!");
        }

        //Only used by Push interaction
        public void PushStart(Transform parentTransform)
        {
            if (!pushed)
            {
                _intObj.transform.parent = parentTransform.transform;
                //Child object with rigidbody works different on Unity versions(Even if it is kinematic)
                //So we dont set it false on newer versions, we dont need it on newer versions.
#if UNITY_2019_3_OR_NEWER
                if (_intObj.hasRigid) DestroyImmediate(_intObj.rigid);
#else
                if (_intObj.hasRigid) _intObj.rigid.isKinematic = false;
#endif
                pushed = true;
            }
        }
        public void PushEnd()
        {
            if (pushed)
            {
                _intObj.transform.parent = _parentTransform;
#if UNITY_2019_3_OR_NEWER
#else
                if (_intObj.hasRigid) _intObj.rigid.isKinematic = true;
#endif
                _intObj.ready = false;
                pushed = false;
            }
        }
    }
}
