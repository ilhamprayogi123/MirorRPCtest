using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace razz
{
    [InitializeOnLoad]
    public static class SceneviewPopupGUIHandler
    {
        private static bool _initialized;
        private static Stopwatch _clickTimer;
        private static int _controlIDHint;
        private static Vector2 _mousePos;

        static SceneviewPopupGUIHandler()
        {
            SetEnabled(true);
        }

        public static void SetEnabled(bool enabled)
        {
            if (enabled)
            {
#if UNITY_2019_1_OR_NEWER
                SceneView.duringSceneGui += OnSceneGUI;
#else
                SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif

                if (_initialized == false)
                {
                    _clickTimer = new Stopwatch();
                    _controlIDHint = "InteractorUniqueID".GetHashCode();
                    _initialized = true;
                }
            }
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!InteractorTargetSpawner.isOpen) return;
            
            try
            {
                Event current = Event.current;
                int id = GUIUtility.GetControlID(_controlIDHint, FocusType.Passive);

                if (current.button == 1)
                {
                    HandleMouseButton(current, id);
                }
            }
            catch (Exception ex)
            {
                // When something goes wrong, we need to reset hotControl or
                // the SceneView mouse cursor will stay stuck as a drag hand.
                GUIUtility.hotControl = 0;

                if (ex.GetType() != typeof(ExitGUIException))
                    Debug.LogException(ex);
            }
        }

        private static void HandleMouseButton(Event current, int id)
        {
            switch (current.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    OnMouseDown();
                    break;

                case EventType.MouseUp:
                    OnMouseUp(current);
                    break;
            }
        }

        private static void OnMouseDown()
        {
            _clickTimer.Start();
            _mousePos = Event.current.mousePosition;
        }

        private static void OnMouseUp(Event current)
        {
            long elapsedMilliseconds = ResetTimer();
            
            // Only show the selection menu if the click was short,
            // not if the user is holding to drag the SceneView camera.
            if (elapsedMilliseconds < InteractorTargetSpawner.rightClickTimer)
            {
#if UNITY_2022_2_OR_NEWER //Workaround to counter the changes they made after this version
                EditorApplication.delayCall += () =>
                {
                    var gameObjects = InteractorTargetSpawner.GetPrefabList();

                    if (gameObjects.Count() > 0)
                    {
                        Rect activatorRect = new Rect(current.mousePosition, Vector2.zero);
                        ShowSelectableGameObjectsPopup(activatorRect, gameObjects);
                    }
                };
#else
                GUIUtility.hotControl = 0;
                current.Use();
                var gameObjects = InteractorTargetSpawner.GetPrefabList();

                if (gameObjects.Count() > 0)
                {
                    Rect activatorRect = new Rect(current.mousePosition, Vector2.zero);
                    ShowSelectableGameObjectsPopup(activatorRect, gameObjects);
                    current.Use();
                }
#endif
            }
        }

        private static long ResetTimer()
        {
            _clickTimer.Stop();
            long elapsedTime = _clickTimer.ElapsedMilliseconds;
            _clickTimer.Reset();
            return elapsedTime;
        }

        private static void ShowSelectableGameObjectsPopup(Rect rect, List<GameObject> prefabs)
        {
            var content = new SceneviewPopup(prefabs, _mousePos);
            PopupWindow.Show(rect, content);
        }
    }
}
