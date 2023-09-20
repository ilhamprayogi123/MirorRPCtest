using UnityEngine;

namespace razz
{
    [CreateAssetMenu(fileName = "DefaultSettings", menuName = "Interactor/DefaultSettings")]
    public class Default : InteractionTypeSettings
    {
        [Tooltip("When the object enters this range, it will be activated without any other effector rules check.")]
        public float defaultAnimatedDistance = 1.8f;
    }
}
