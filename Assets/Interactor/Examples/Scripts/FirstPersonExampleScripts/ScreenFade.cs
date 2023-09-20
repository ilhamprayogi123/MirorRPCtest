using UnityEngine;

namespace razz
{
    public class ScreenFade : MonoBehaviour
    {
        public AnimationCurve FadeCurve;
        public bool fadeEnabled = true;

        private float _alpha;
        private Texture2D _texture;
        private float _time;
        private bool _start;

        [ContextMenu("StartFade")]
        public void StartFade()
        {
            _start = true;
        }

        public void OnGUI()
        {
            if (!_start || !fadeEnabled) return;
            if (_texture == null) _texture = new Texture2D(1, 1);

            _texture.SetPixel(0, 0, new Color(1, 0, 0, _alpha));
            _texture.Apply();

            _time += Time.deltaTime;
            _alpha = FadeCurve.Evaluate(_time);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _texture);
        }
    }
}
