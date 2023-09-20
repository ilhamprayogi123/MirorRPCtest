using UnityEngine;

namespace razz
{
    public class AddRemoveExample : MonoBehaviour
    {
        [TextArea] public string note = "This example script shows how to add or remove moving colliders and InteractorObjects to PathGrid at runtime. While in the Play mode, you can assign the following properties in the Inspector.";
        public PathGrid pathGrid;
        public Collider addCollider;
        public Collider removeCollider;
        public InteractorObject addInteractorObject;
        public InteractorObject removeInteractorObject;

        private void OnValidate()
        {
            if (pathGrid == null) return;

            if (addCollider != null)
            {
                if (pathGrid.movingColliders.Contains(addCollider))
                {
                    Debug.Log("Collider is already exist.", this);
                }
                pathGrid.AddCollider(addCollider); //Won't add duplicates.
                addCollider = null;
            }

            if (removeCollider != null)
            {
                pathGrid.RemoveCollider(removeCollider);
                removeCollider = null;
            }

            if (addInteractorObject != null)
            {
                if (pathGrid.intObjs.Contains(addInteractorObject))
                {
                    Debug.Log("InteractorObject is already exist.", this);
                }
                pathGrid.AddInteractionManual(addInteractorObject); //Won't add duplicates.
                addInteractorObject = null;
            }

            if (removeInteractorObject != null)
            {
                pathGrid.RemoveInteractionManual(removeInteractorObject);
                removeInteractorObject = null;
            }
        }
    }
}
