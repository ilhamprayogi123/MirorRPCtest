using UnityEngine;

namespace razz
{
    //Showing values with prefix on console frame. 
    //Called by Rotators(InteractiveRotator) in their inspector events.
    public class DigitalDisplayTextFloat : MonoBehaviour
    {
        private float _decimal;

        [SerializeField, Tooltip("Text after value")]
        public string prefix = "";
        [SerializeField, Tooltip("Value will be multiplied by this")]
        public float multiplyValue = 1;
        [SerializeField, Tooltip("How many decimals value will have"), Range(0, 5)]
        public int valueDecimals = 0;
        [SerializeField, Tooltip("TextMesh that will be used")]
        public TextMesh textMesh;

        public void SetTextFloat(float value)
        {
            if (textMesh != null)
            {
                _decimal = Mathf.Pow(10, (float)valueDecimals);
                value *= multiplyValue;

                value = Mathf.Round(value * _decimal) / _decimal;
                textMesh.text = value.ToString() + " " + prefix;
            }
        }
    }
}
