using UnityEngine;
using UnityEditor;

namespace razz
{
    public abstract class WindowGUI
    {
        private static Color _defaultBGcolor;
        private static Color _defaultTextcolor;
        private static Color _defaultGUIcolor;
        private static Color _guiColor = new Color(1, 1, 1, 0.65f);
        private static Color _mbColor = new Color(0.196f, 0.349f, 0.439f, 0.6f);
        private static Color _textColor = new Color(0.675f, 0.829f, 0.9f, 1f);

        // Overridable fields
        protected bool draggable = true;
        protected float labelWidth = 100;
        protected float sliderNumberWidth = 60;
        [HideInInspector] public Rect windowRect = new Rect(360, 20, 100, 0);
        public Rect Rect => windowRect;
        public string windowTooltip;

        private static GUISkin _skin;
        public static GUISkin Skin
        {
            get
            {
                if (!_skin)
                    _skin = Resources.Load<GUISkin>("InteractorGUISkin");
                return _skin;
            }
        }

        public void WindowBase(int id)
        {
            Window();

            if (draggable)
                GUI.DragWindow();
        }

        protected abstract void Window();

        protected void Foldout(string label, ref bool value, string tooltip = null)
        {
            bool clicked = string.IsNullOrEmpty(tooltip) ?
                Button((value ? "[-] " : "[+] ") + label) :
                Button((value ? "[-] " : "[+] ") + label, tooltip);

            if (clicked)
            {
                value = !value;
                windowRect.height = 0;
            }
        }

        #region Labels and Fields

        protected void Label(string label)
        {
            GUILayout.Label(label);
        }

        protected void Label(string label, string tooltip)
        {
            GUILayout.Label(new GUIContent(label, tooltip));
        }

        protected void LabelField(string label, ref string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(labelWidth));
            value = GUILayout.TextField(value);
            GUILayout.EndHorizontal();
        }

        protected string LabelField(string label, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, Skin.GetStyle("LabelSceneview"), GUILayout.Width(labelWidth));
            value = GUILayout.TextField(value, Skin.GetStyle("TextField"));
            GUILayout.EndHorizontal();

            return value;
        }

        protected float FloatField(string label, float value)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(label, GUILayout.Width(labelWidth));
            float prevValue = value;
            if (float.TryParse(GUILayout.TextField(value.ToString()), out float newValue))
                value = newValue;
            else value = prevValue;

            GUILayout.EndHorizontal();

            return value;
        }

        protected float FloatFieldShort(string label, float value)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(label, GUILayout.Width(20f));
            float prevValue = value;
            if (float.TryParse(GUILayout.TextField(value.ToString(), GUI.skin.GetStyle("TextField")), out float newValue))
                value = newValue;
            else value = prevValue;

            GUILayout.EndHorizontal();

            return value;
        }
        #endregion

        #region Ref Sliders

        protected void Slider(string label, ref float value, float min, float max, string tooltip = null)
        {
            GUILayout.BeginHorizontal();

            TTip(label, tooltip);

            GUILayout.Label(value.ToString(), GUILayout.Width(sliderNumberWidth));
            value = GUILayout.HorizontalSlider(value, min, max);

            GUILayout.EndHorizontal();
        }

        protected void Slider(string label, ref int value, float min, float max, string tooltip = null)
        {
            GUILayout.BeginHorizontal();

            TTip(label, tooltip);

            GUILayout.Label(value.ToString(), GUILayout.Width(sliderNumberWidth));
            value = Mathf.RoundToInt(GUILayout.HorizontalSlider(value, min, max));

            GUILayout.EndHorizontal();
        }

        protected void Slider(string label, ref float value, float min, float max, float stepSize, string tooltip = null)
        {
            GUILayout.BeginHorizontal();

            TTip(label, tooltip);

            GUILayout.Label(value.ToString("F1"), GUILayout.Width(sliderNumberWidth));
            value = Mathf.Round(GUILayout.HorizontalSlider(value, min, max) / stepSize) * stepSize;

            GUILayout.EndHorizontal();
        }
        #endregion

        #region Non-ref Sliders

        protected float Slider(string label, float value, float min, float max, in string format = null, string tooltip = null)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, Skin.GetStyle("LabelSceneview"), GUILayout.Width(80));

            if (string.IsNullOrEmpty(format))
                GUILayout.Label(value.ToString(), Skin.GetStyle("LabelSceneview"), GUILayout.Width(40));
            else
                GUILayout.Label(value.ToString(format), Skin.GetStyle("LabelSceneview"), GUILayout.Width(40));

            value = GUILayout.HorizontalSlider(value, min, max, Skin.GetStyle("HorizontalsliderSceneview"), Skin.GetStyle("HorizontalsliderthumbSceneview"));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            return value;
        }

        protected int Slider(int value, float min, float max, string tooltip = null)
        {
            value = Mathf.RoundToInt(GUILayout.HorizontalSlider(value, min, max, Skin.GetStyle("HorizontalsliderSceneview"), Skin.GetStyle("HorizontalsliderthumbSceneview")));
            return value;
        }

        protected float Slider(string label, float value, float min, float max, float stepSize, string tooltip = null)
        {
            GUILayout.BeginHorizontal();
            value = Mathf.Round(GUILayout.HorizontalSlider(value, min, max, Skin.GetStyle("HorizontalsliderSceneview"), Skin.GetStyle("HorizontalsliderthumbSceneview")) / stepSize) * stepSize;
            GUILayout.EndHorizontal();
            return value;
        }

        protected float Slider(string label, float value, float min, float max, float stepSize)
        {
            GUILayout.BeginHorizontal();
            value = Mathf.Round(GUILayout.HorizontalSlider(value, min, max, Skin.GetStyle("HorizontalsliderSceneview"), Skin.GetStyle("HorizontalsliderthumbSceneview")) / stepSize) * stepSize;
            GUILayout.EndHorizontal();
            return value;
        }

        public static int Slider(int value, float min, float max, float width)
        {
            GUILayout.BeginHorizontal();
            value = Mathf.RoundToInt(GUILayout.HorizontalSlider(value, min, max, Skin.GetStyle("HorizontalsliderSceneview"), Skin.GetStyle("HorizontalsliderthumbSceneview"), GUILayout.Width(width)));
            GUILayout.EndHorizontal();
            return value;
        }
        #endregion

        #region Buttons

        protected bool Button(string label)
        {
            GetColor();
            GetGuiColor();
            bool value = GUILayout.Button(label, EditorStyles.miniButton);
            SetGuiColor();
            SetColor();
            return value;
        }

        protected bool Button(string label, string tooltip)
        {
            GetColor();
            GetGuiColor();
            bool value = GUILayout.Button(new GUIContent(label, tooltip), EditorStyles.miniButton);
            SetGuiColor();
            SetColor();
            return value;
        }

        protected bool ToggleButton(string name, bool value)
        {
            GetColor();
            if (GUILayout.Button(name + (value ? " On" : " Off"), EditorStyles.miniButton))
                value = !value;
            SetColor();
            return value;
        }

        protected bool ToggleButton(string name, ref bool value)
        {
            GetColor();
            GetGuiColor();
            if (GUILayout.Button(name + (value ? " On" : " Off"), EditorStyles.miniButton))
            {
                value = !value;
                SetColor();
                return true;
            }
            SetGuiColor();
            SetColor();

            return false;
        }

        public static bool ToggleButtonRef(string name, ref bool value)
        {
            GetColor();
            GetGuiColor();
            if (GUILayout.Button(name + (value ? " Effector Settings Mode " : " Spawner Settings Mode "), EditorStyles.miniButton))
            {
                value = !value;
                SetColor();
                InteractorTargetSpawner.Instance.Repaint();
                return true;
            }
            SetGuiColor();
            SetColor();
            return false;
        }

        public static bool ToggleButtonValue(string name, bool value)
        {
            GetColor();
            if (GUILayout.Button(name + (value ? " [+]" : " [-]"), EditorStyles.miniButton))
                value = !value;
            SetColor();
            return value;
        }

        public static bool ButtonValue(string label, string tooltip)
        {
            GetColor();
            GetGuiColor();
            bool value = GUILayout.Button(new GUIContent(label, tooltip), EditorStyles.miniButton);
            SetGuiColor();
            SetColor();
            return value;
        }
        #endregion

        void TTip(in string label, in string tooltip)
        {
            if (string.IsNullOrEmpty(tooltip))
                GUILayout.Label(label, GUILayout.Width(150));
            else
                GUILayout.Label(new GUIContent(label, tooltip), GUILayout.Width(labelWidth));
        }

        private static void GetColor()
        {
            _defaultBGcolor = GUI.backgroundColor;
            _defaultTextcolor = EditorStyles.miniButton.normal.textColor;
            GUI.backgroundColor = _mbColor;
            EditorStyles.miniButton.normal.textColor = _textColor;
        }

        private static void GetGuiColor()
        {
            _defaultGUIcolor = GUI.color;
            GUI.color *= _guiColor;
        }

        private static void SetColor()
        {
            GUI.backgroundColor = _defaultBGcolor;
            EditorStyles.miniButton.normal.textColor = _defaultTextcolor;
        }

        private static void SetGuiColor()
        {
            GUI.color = _defaultGUIcolor;
        }
    }
}