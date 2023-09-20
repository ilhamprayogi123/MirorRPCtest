using UnityEngine;

namespace razz
{
    public class InfoToggle : MonoBehaviour
    {
        public Renderer[] infos;

        private bool _active = true;

        private void Update()
        {
            if (BasicInput.GetToggle())
                ToggleInfos();
        }

        public void ToggleInfos()
        {
            _active = !_active;

            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i] != null)
                {
                    infos[i].enabled = _active;
                }
            }
        }
    }
}
