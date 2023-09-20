using UnityEngine;

namespace razz
{
    //Toggles on and off its GameObject array objects in hierarchy.
    //Called by Switch_Base(InteractiveSwitch) event on inspector.
    public class BasicOnOff : MonoBehaviour
    {
        private bool _onoff = false;

        public GameObject[] onoffArray;

        public void Toggle()
        {
            if (onoffArray.Length == 0) return;

            if (!_onoff)
            {
                for (int i = 0; i < onoffArray.Length; i++)
                {
                    if (onoffArray[i] != null)
                    {
                        onoffArray[i].SetActive(true);
                    }
                    else
                    {
                        Debug.Log(this.name + " has empty gameobject for BasicOnOff script array.");
                    }
                }
                _onoff = !_onoff;
            }
            else
            {
                for (int i = 0; i < onoffArray.Length; i++)
                {
                    if (onoffArray[i] != null)
                    {
                        onoffArray[i].SetActive(false);
                    }
                    else
                    {
                        Debug.Log(this.name + " has empty gameobject for BasicOnOff script array.");
                    }
                }
                _onoff = !_onoff;
            }
        }
    }
}

