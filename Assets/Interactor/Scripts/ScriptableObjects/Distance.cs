using UnityEngine;

namespace razz
{
    [CreateAssetMenu(fileName = "DistanceSettings", menuName = "Interactor/DistanceSettings")]
    public class Distance : InteractionTypeSettings
    {
        public override void Init(InteractorObject interactorObject)
        {
            base.Init(interactorObject);
            //Second max priority to keep distance interaction on second top of interaction list order.
            _intObj.priority = int.MaxValue - 1;
        }
    }
}
