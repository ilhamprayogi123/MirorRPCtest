using UnityEngine;
using UnityEditor;

namespace razz
{
    public class ToolbarGUI : WindowGUI
    {
        private MenuGUI _windowRef;
        private bool _hide = false;

        public string WindowLabel { get { return ""; } }
        public bool collectOnStart = true;
        public int buttonWidth = 80;
        public int toolbarOffset = 400;

        public void Setup(MenuGUI window, Interactor newInteractor)
        {
            _hide = false;
            draggable = false;
#if UNITY_2021_2_OR_NEWER
            windowRect.y = 0;
#elif UNITY_2019_3_OR_NEWER
            windowRect.y = 21f;
#else
            windowRect.y = 17f;
#endif
            windowRect.x = 80f;
            windowRect.width = 107f;
            _windowRef = window;
            _windowRef.SetInteractor(newInteractor);
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnScene;
#else
            SceneView.onSceneGUIDelegate += OnScene;
#endif
        }

        public void Disable(MenuGUI window)
        {
            window.Disable();
            _hide = true;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnScene;
#else
            SceneView.onSceneGUIDelegate -= OnScene;
#endif
        }

        protected override void Window()
        {
            if (GUILayout.Button("Interactor", Skin.GetStyle("SmallButtonSceneview")))
            {
                _windowRef.Show();
            }
        }

        public void OnScene(SceneView sceneView)
        {
            if (_hide) return;

            GUI.skin = Skin;

            windowRect = GUILayout.Window(1, windowRect, base.WindowBase, WindowLabel, GUI.skin.GetStyle("Box"));

            if (windowRect.Contains(Event.current.mousePosition))
            {
                sceneView.Repaint();
            }

            if (!string.IsNullOrEmpty(base.windowTooltip))
            {
                Vector2 mouse = Input.mousePosition;
                mouse.y = Screen.height - mouse.y;
                GUI.Label(new Rect(50, Screen.height - 80, 1000, 1000), base.windowTooltip);
            }
        }
    }
}
