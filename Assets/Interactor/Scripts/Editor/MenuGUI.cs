using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace razz
{
    [ExecuteInEditMode]
    public class MenuGUI : WindowGUI
    {
        private Interactor _interactor;
        private Interactor.EffectorLink _effectorlink;
        private bool _showFoldout;
        private List<GameObject> _spawnSettings;

        //Effector Data
        private Vector3 _posOffset;
        private float _angleXZ, _angleYZ, _angleOffset, _angleOffsetYZ, _maxRadius, _minRadius, _opacity;
        private int _selectedTab;

        //Styles
        private GUIStyle _horizontalsliderthumb;
        private GUIStyle _labelSceneview;
        private GUIStyle _labelSceneview2;
        private GUIStyle _smallToggleSceneview;
        private float _defaultThumbWidth;

        [HideInInspector] public bool enabled = false;
        public string WindowLabel = "Effector Settings";

        public void SetInteractor(Interactor newInteractor)
        {
            _interactor = newInteractor;
        }

        protected override void Window()
        {
            if (_interactor == null)
            {
                _interactor = Interactor.Instance;
                if (_interactor == null)
                {
                    Disable();
                    return;
                }
            }
            if (_interactor.effectorLinks.Count == 0)
            {
                Disable();
                return;
            }

            if (InteractorTargetSpawner.spawnerSceneviewMenu)
            {
                WindowLabel = "Spawn Settings";
#if UNITY_2019_3_OR_NEWER
                windowRect.height = 162;
#else
                windowRect.height = 155;
#endif

                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();


                InteractorTargetSpawner.addPivotOnPoint = (bool)EditorGUILayout.Toggle(InteractorTargetSpawner.addPivotOnPoint, _smallToggleSceneview);
                if (EditorGUI.EndChangeCheck())
                {

                }
                GUILayout.Label(" Add Pivot ", _labelSceneview2);
                GUILayout.EndHorizontal();

                GUILayout.Space(4f);

                GUILayout.BeginHorizontal();
                InteractorTargetSpawner.addComponentsOnParent = (bool)EditorGUILayout.Toggle(InteractorTargetSpawner.addComponentsOnParent, _smallToggleSceneview);
                GUILayout.Label(" Add Components ", _labelSceneview2);
                GUILayout.EndHorizontal();

                GUILayout.Space(8f);
                EditorGUILayout.LabelField("", Skin.GetStyle("HorizontalLineCenter"));

                if (InteractorTargetSpawner.addComponentsOnParent)
                {
                    _spawnSettings = InteractorTargetSpawner.GetSpawnSettings();
                    if (_spawnSettings == null)
                    {
                        InteractorTargetSpawner.addComponentsOnParent = false;
                        Debug.LogWarning("Add Component option disabled because one or more spawn presets are null. (Spawner Window -> SceneView Window for Spawner)");
                    }
                    else
                    {
                        GUILayout.Label(" " + _spawnSettings[InteractorTargetSpawner.selected].name, _labelSceneview);
                        Rect selectedSpawnRect = new Rect(0, 45, windowRect.width, 50);
                        if (selectedSpawnRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.ScrollWheel)
                        {
                            if (InteractorTargetSpawner.selected < _spawnSettings.Count - 1 && Event.current.delta.y < 0)
                                InteractorTargetSpawner.selected++;
                            else if (InteractorTargetSpawner.selected > 0 && Event.current.delta.y > 0)
                                InteractorTargetSpawner.selected--;
                            Event.current.Use();
                        }
                        InteractorTargetSpawner.selected = (int)Slider(InteractorTargetSpawner.selected, 0, _spawnSettings.Count - 1);
                        GUILayout.Space(8f);
                    }
                }
                else GUILayout.Space(31f);

                if (InteractorTargetSpawner.surfaceRotation == 1)
                    GUILayout.Label(" Surface Rotation", _labelSceneview);
                else if (InteractorTargetSpawner.surfaceRotation == 2)
                    GUILayout.Label(" Camera to Object Direction", _labelSceneview);
                else if (InteractorTargetSpawner.surfaceRotation == 3)
                    GUILayout.Label(" Camera to Object (Y only)", _labelSceneview);
                else GUILayout.Label(" Default Prefab Rotation", _labelSceneview);
                Rect surfaceRect = new Rect(0, 95, windowRect.width, 50);
                if (surfaceRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.ScrollWheel)
                {
                    if (InteractorTargetSpawner.surfaceRotation < 3 && Event.current.delta.y < 0)
                        InteractorTargetSpawner.surfaceRotation++;
                    else if (InteractorTargetSpawner.surfaceRotation > 0 && Event.current.delta.y > 0)
                        InteractorTargetSpawner.surfaceRotation--;
                    Event.current.Use();
                }
                InteractorTargetSpawner.surfaceRotation = (int)Slider(InteractorTargetSpawner.surfaceRotation, 0, 3);

                GUILayout.Space(8f);
                ToggleButtonRef("", ref InteractorTargetSpawner.spawnerSceneviewMenu);
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();

                if (GUI.changed) InteractorTargetSpawner.Instance.Repainter();
                return;
            }

            WindowLabel = "Effector Settings";
            _horizontalsliderthumb.fixedWidth = 30f;

            GUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName("Slider");
            _selectedTab = (int)Slider(_interactor.selectedTab, 0, _interactor.effectorLinks.Count - 1);
            Rect effectorTabRect = new Rect(0, 0, windowRect.width, 45);
            if (effectorTabRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.ScrollWheel)
            {
                if (_selectedTab < _interactor.effectorLinks.Count - 1 && Event.current.delta.y < 0)
                    _selectedTab++;
                else if (_selectedTab > 0 && Event.current.delta.y > 0)
                    _selectedTab--;
                Event.current.Use();
            }
            if (EditorGUI.EndChangeCheck() || _selectedTab != _interactor.selectedTab)
            {
                _interactor.selectedTab = _selectedTab;
                GUI.FocusControl("Slider");

                if (InteractorTargetSpawner.Instance)
                {
                    InteractorTargetSpawner.Instance.Repainter();
                }
                InteractorEditor.Repainter();
            }

            _effectorlink = _interactor.effectorLinks[_interactor.selectedTab];

            GUILayout.Space(12f);

            GUILayout.BeginHorizontal();
            GUILayout.Label(_interactor.effectorLinks[_interactor.selectedTab].effectorName + " Position:", _labelSceneview);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            _posOffset = EditorGUILayout.Vector3Field("", _effectorlink.posOffset);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_interactor, "Interactor Position Change");
                _effectorlink.posOffset = _posOffset;
                InteractorEditor.Repainter();
            }

            GUILayout.Space(10f);

            EditorGUI.BeginChangeCheck();
            _angleXZ = (int)Slider("H Angle: ", _effectorlink.angleXZ, 0f, 360f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_interactor, "Effector H.Angle Change");
                _effectorlink.angleXZ = _angleXZ;
                InteractorEditor.Repainter();
            }

            GUILayout.Space(4f);

            EditorGUI.BeginChangeCheck();
            _angleOffset = (int)Slider("H Offset: ", _effectorlink.angleOffset, -180f, 180f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_interactor, "Effector H.Offset Change");
                _effectorlink.angleOffset = _angleOffset;
                InteractorEditor.Repainter();
            }

            GUILayout.Space(4f);

            EditorGUI.BeginChangeCheck();
            _angleYZ = (int)Slider("V Angle: ", _effectorlink.angleYZ, 0f, 360f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_interactor, "Effector V.Angle Change");
                _effectorlink.angleYZ = _angleYZ;
                InteractorEditor.Repainter();
            }

            GUILayout.Space(4f);

            EditorGUI.BeginChangeCheck();
            _angleOffsetYZ = (int)Slider("V Offset: ", _effectorlink.angleOffsetYZ, -180f, 180f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_interactor, "Effector V.Offset Change");
                _effectorlink.angleOffsetYZ = _angleOffsetYZ;
                InteractorEditor.Repainter();
            }

            GUILayout.Space(4f);

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Dist:     " + _effectorlink.maxRadius.ToString("F2"), _labelSceneview);
            _maxRadius = Slider("", _effectorlink.maxRadius, 0f, _interactor.sphereCol.radius, .01f);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_interactor, "Effector Distance Change");
                _effectorlink.maxRadius = _maxRadius;
                if (_effectorlink.minRadius > _effectorlink.maxRadius)
                    _effectorlink.minRadius = _effectorlink.maxRadius;
                InteractorEditor.Repainter();
            }

            GUILayout.Space(4f);

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Min Dist:     " + _effectorlink.minRadius.ToString("F2"), _labelSceneview);
            _minRadius = Slider("", _effectorlink.minRadius, 0f, _effectorlink.maxRadius, .01f);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_interactor, "Effector Distance Change");
                _effectorlink.minRadius = _minRadius;
                InteractorEditor.Repainter();
            }

            GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            bool enabled = _effectorlink.enabled;
            enabled = ToggleButton("Enabled:", enabled);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_interactor, "Effector Enable Change");
                _effectorlink.enabled = enabled;
                InteractorEditor.Repainter();
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            bool debug = _interactor.debug;
            debug = ToggleButton("Debug", debug);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_interactor, "Interactor Debug Change");
                _interactor.debug = debug;
                InteractorEditor.Repainter();
                SceneView.RepaintAll();
            }
            Foldout("More", ref _showFoldout);
            GUILayout.EndHorizontal();

            if (_showFoldout)
            {
                GUILayout.Space(10f);
                EditorGUI.BeginChangeCheck();
                _opacity = Slider("Opacity: ", InteractorEditor.opacityValue, 0f, 1f, "F2");
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_interactor, "Interactor Opacity Change");
                    _interactor.opacity = _opacity;
                    InteractorEditor.Repainter();
                }
                GUILayout.Space(10f);
            }

            if (!InteractorTargetSpawner.spawnerSceneviewMenu)
            {
                GUILayout.Space(2f);
                ToggleButtonRef("", ref InteractorTargetSpawner.spawnerSceneviewMenu);
                GUILayout.Space(6f);
            }

            _horizontalsliderthumb.fixedWidth = _defaultThumbWidth;
            if (GUI.changed) EditorUtility.SetDirty(_interactor);
        }

        private void Setup()
        {
            windowRect.width = 250;
            labelWidth = 90;
            sliderNumberWidth = 30;
            draggable = true;
        }

        public void Show()
        {
            if (InteractorTargetSpawner.addComponentsOnParent)
            {
                _spawnSettings = InteractorTargetSpawner.GetSpawnSettings();
                if (_spawnSettings == null)
                {
                    InteractorTargetSpawner.addComponentsOnParent = false;
                    Debug.LogWarning("Add Component option disabled because one or more spawn presets are null. (Spawner Window -> SceneView Window for Spawner)");
                }
            }

            enabled = !enabled;
            if (enabled)
            {
                GetStyles();
                Undo.undoRedoPerformed += UndoRedoRefresh;
#if UNITY_2019_1_OR_NEWER
                SceneView.duringSceneGui += OnScene;
#else
                SceneView.onSceneGUIDelegate += OnScene;
#endif
                if (_interactor != null)
                {
                    Selection.activeTransform = _interactor.transform;
                }
                else
                {
                    _interactor = Interactor.Instance;
                    Selection.activeTransform = _interactor.transform;
                }
            }
            else
            {
                Undo.undoRedoPerformed -= UndoRedoRefresh;
#if UNITY_2019_1_OR_NEWER
                SceneView.duringSceneGui -= OnScene;
#else
                SceneView.onSceneGUIDelegate -= OnScene;
#endif
            }
        }

        public void Disable()
        {
            if (InteractorTargetSpawner.Instance.HasSaveData())
                InteractorTargetSpawner.Instance.Save();

            enabled = false;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnScene;
#else
            SceneView.onSceneGUIDelegate -= OnScene;
#endif
            Undo.undoRedoPerformed -= UndoRedoRefresh;
        }

        private void UndoRedoRefresh()
        {
            SceneView.RepaintAll();
        }

        private void GetStyles()
        {
            _horizontalsliderthumb = Skin.GetStyle("horizontalsliderthumb");
            _defaultThumbWidth = _horizontalsliderthumb.fixedWidth;
            _labelSceneview = Skin.GetStyle("LabelSceneview");
            _labelSceneview2 = Skin.GetStyle("LabelSceneview2");
            _smallToggleSceneview = Skin.GetStyle("SmallToggleSceneview");
        }

        public void OnScene(SceneView sceneView)
        {
            Setup();
            GUI.skin = Skin;
            windowRect = GUILayout.Window(0, windowRect, base.WindowBase, WindowLabel);

            if (windowRect.x < 0f)
            {
                windowRect.x = 0f;
            }
            if (windowRect.x + windowRect.width > sceneView.position.width)
            {
                windowRect.x = sceneView.position.width - windowRect.width;
            }
#if UNITY_2019_3_OR_NEWER
            if (windowRect.y < 21f)
            {
                windowRect.y = 21f;
            }
#else
            if (windowRect.y < 17f)
            {
                windowRect.y = 17f;
            }
#endif
#if UNITY_2021_2_OR_NEWER
            if (windowRect.y + windowRect.height > sceneView.position.height - 25f)
            {
                windowRect.y = sceneView.position.height - windowRect.height - 25f;
            }
#else
            if (windowRect.y + windowRect.height > sceneView.position.height)
            {
                windowRect.y = sceneView.position.height - windowRect.height;
            }
#endif
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
