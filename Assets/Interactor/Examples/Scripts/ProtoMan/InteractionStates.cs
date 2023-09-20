using UnityEngine;

namespace razz
{
    //This class responsible to hold only this player's interaction states. Various classes look for this states. It'll get instantiated by Interactor if not exist.
    public class InteractionStates : MonoBehaviour
    {
        public bool playerClimbing { get; set; }
        public bool playerClimable { get; set; }
        public bool playerClimbed { get; set; }
        public bool rePos { get; set; }
        public bool playerChanging { get; set; }
        public bool playerChangable { get; set; }
        public bool playerOnVehicle { get; set; }
        public bool playerUsing { get; set; }
        public bool playerUsable { get; set; }
        public bool playerCrouching { get; set; }
        public bool playerIdle { get; set; }
        public bool playerMoving { get; set; }
        public bool playerGrounded { get; set; }
        public bool playerPushing { get; set; }

        //Focused vehicle or used object (for multiple type interactions)
        public GameObject enteredVehicle { get; set; }

        [HideInInspector] public Vector3 targetPosition;
        [HideInInspector] public Quaternion targetRotation;
        [HideInInspector] public Vector3 targetTopPosition;
        [HideInInspector] public Quaternion targetTopRotation;
    }
}
