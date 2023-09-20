using UnityEngine;
using UnityEngine.Events;

namespace razz
{
    public class AddInteractionTriggerExample : MonoBehaviour
    {
        public PathGrid pathGrid;
        public BasicBot[] botsToStartInteraction;

        private void OnTriggerEnter(Collider other)
        {
            InteractorObject intObj = other.GetComponent<InteractorObject>();
            if (intObj)
            {
                pathGrid.AddInteractionManual(intObj);

                if (botsToStartInteraction != null)
                {
                    for (int i = 0; i < botsToStartInteraction.Length; i++)
                    {
                        if (botsToStartInteraction[i] != null)
                        {
                            botsToStartInteraction[i].StartInteractions();
                        }
                    }
                }
            }
        }
        private void OnTriggerExit(Collider other)
        {
            InteractorObject intObj = other.GetComponent<InteractorObject>();
            if (intObj)
            {
                pathGrid.RemoveInteractionManual(intObj);
            }
        }
    }
}
