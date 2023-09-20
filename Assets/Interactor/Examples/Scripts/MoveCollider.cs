using UnityEngine;

namespace razz
{
    public class MoveCollider : MonoBehaviour
    {
        public float movementDistance = 5f;
        public Vector3 direction = new Vector3(1f, 0, 0);

        private Vector3 initialPosition;
        private float movementTimer;

        private void Start()
        {
            initialPosition = transform.position;
        }

        private void Update()
        {
            movementTimer += Time.deltaTime * 0.5f;
            Vector3 newPosition = initialPosition + direction * Mathf.PingPong(movementTimer, movementDistance);
            transform.position = newPosition;
        }
    }
}
