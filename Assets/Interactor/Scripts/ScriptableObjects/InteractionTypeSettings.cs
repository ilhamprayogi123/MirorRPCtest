using UnityEngine;

namespace razz
{
    public abstract class InteractionTypeSettings : ScriptableObject
    {
        protected InteractorObject _intObj;

        public virtual void UpdateSettings()
        {

        }

        public virtual void Init(InteractorObject interactorObject)
        {
            _intObj = interactorObject;
        }
    }
}
