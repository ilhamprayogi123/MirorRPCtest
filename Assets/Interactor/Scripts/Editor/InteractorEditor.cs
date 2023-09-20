using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace razz
{
    [CustomEditor(typeof(Interactor))]
    public class InteractorEditor : Editor
    {
        #region Variables
        private Interactor _script { get { return target as Interactor; } }
        private Interactor.EffectorLink _effectorlink;

        //GUI
        private string[] _effectorTabs;
        private bool[] _toggleBottomElements;
        private bool _defaultBottom = true;
        public static float opacityValue = 1f;
        public int selectedEffectorTab;

        //Copy Paste jobs
        private bool _copiedTab, _debug, _logoChange, _lookEnabled;
        private bool _tempEnabled;
        private float _hRangeOff, _hRange, _vRangeOff, _vRange, _effectorMaxDist, _effectorMinDist;
        private Vector3 _effectorPos;

        //Styles
        private GUISkin _skin;
        private GUIStyle _background;
        private GUIStyle _smallButton;
        private GUIStyle _tabButtonStyle;
        private GUIStyle _addButton;
        private GUIStyle _deleteButton;
        private GUIStyle _copyButton;
        private GUIStyle _pasteButton;
        private GUIStyle _autoButton;
        private GUIStyle _createTargetButton;
        private GUIStyle _enableButton;
        private GUIStyle _textField;
        private GUIStyle _dropDown;
        private GUIStyle _hLine;
        private static GUIStyle _sceneText;
        private Texture2D _logoBig;
        private Texture2D _logoSmall;
        private Color _textColor = new Color(0.723f, 0.821f, 0.849f);
        private Color _textFieldColor = new Color(0.172f, 0.192f, 0.2f);
        private Color _GuiColor = new Color(0.71f, 0.82f, 0.88f, 0.96f);
        private Color _defaultGuiColor;
        private Color _defaultTextColor = new Color(0, 0, 0, 0); //Populated for OnEnable use to get default colors.
        private Color _defaultMiniButtonTextColor;
        private Color _defaultToolbarButtonTextColor;
        private Color _defaultTextFieldColor;
        private Color _changeHandleColor = new Color(0.65f, 0.65f, 0.65f);
        private float _verticalSpace = 4f;
        private float _logoSpace;
        private float _logoAreaSpace;
        private Rect _logo;
        private Rect windowRect;
        private float windowWidth, windowY; //float windowX;
        private bool handlesOn;
        private float _tabButtonStyleWidth;

        //Installing Integrations
        private bool _installDefault;
        private bool _installFik;
        private bool _defaultFiles;
        private bool _frontPage;

        //Temporary variables for calculations
        private static Vector3 _tempVecA, _tempVecB;
        private Color _tempColor;
        private Vector3 _posOffsetThis, _offsetCenterThis;

        //Gizmo calculations
        private static Vector3 _colCenter;
        private static Vector3 _offsetCenter;
        private static Vector3 _offsetCenterRot;
        private static Vector3 _posOffsetWithRot;
        private static Vector3 _colCenterWithPos;
        private static Vector3 _colCenterWithScaleWithZMoved;
        private static Vector3 _colCenterWithScaleWithYMoved;
        private static List<Vector3> _arcPoints;
        private static Vector3 _tempPoint;
        private static float _colDiameter, _colDiameterZ, _colDiameterY, _pointDist, _pointAngle, _pointAngle2, _halfAngle, _angleDist, _angleEdge, _angleColDiameter, _angleRest, _edge, _angleSlice, _arcAngle, _topAngle;
        private static int _end, _midPointA, _midPointB;
        private static Vector3[] _midPoints, _midLine;
        private static string _halfLabel, _fullLabel;

        //Effector Data
        private bool _enabled;
        private string _effectorName;
        private Interactor.FullBodyBipedEffector _effectorType;
        private Vector3 _posOffset;
        private float _angleXZ, _angleYZ, _angleOffset, _angleOffsetYZ, _maxRadius, _minRadius, _raycastDistance;
        private string _layerName = "Player";
        #endregion

        private void OnEnable()
        {
            if (_script != Interactor.Instance)
            {
                Interactor.Instance = _script;
                selectedEffectorTab = _script.selectedTab;
                if (InteractorTargetSpawner.Instance)
                {
                    if (Selection.activeObject != null)
                    {
                        if (AssetDatabase.Contains(Selection.activeObject))
                            InteractorTargetSpawner.Instance.Close();
                        else InteractorTargetSpawner.Instance.NewInit();
                    }
                }
            }

            Undo.undoRedoPerformed += UndoRedoRefresh;
            GetStyles();
            RefreshTabNames();
            RefreshSpawnerWindowSoft();
            _arcPoints = new List<Vector3>();
            _midPoints = new Vector3[3];
            _midLine = new Vector3[2];
            _toggleBottomElements = new bool[5];
            _logoSpace = _logoBig.height + 10f;

#if UNITY_2019_1_OR_NEWER
            if (_script.debug) SceneView.duringSceneGui += ShowHandles;
#else
            if (_script.debug) SceneView.onSceneGUIDelegate += ShowHandles;
#endif
            handlesOn = true;

            SceneView.RepaintAll();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoRefresh;
        }

        private void OnSceneGUI()
        {
            if (!_script.enabled) return;

            if (_script.effectorLinks.Count == 0) return;

            if (selectedEffectorTab >= _script.effectorLinks.Count)
                selectedEffectorTab = _script.effectorLinks.Count - 1;
        }

        #region Refresh and Style Methods
        private void GetStyles()
        {
            _skin = Resources.Load<GUISkin>("InteractorGUISkin");
            _logoBig = Resources.Load<Texture2D>("Images/TopLogoResized");
            _logoSmall = Resources.Load<Texture2D>("Images/TopLogoSmall");
            _background = _skin.GetStyle("BackgroundStyle");
            _smallButton = _skin.GetStyle("SmallButton");
            _tabButtonStyle = _skin.GetStyle("Button");
            _tabButtonStyleWidth = _tabButtonStyle.fixedWidth;
            _addButton = _skin.GetStyle("MiniButtonAddStyle");
            _deleteButton = _skin.GetStyle("MiniButtonDeleteStyle");
            _copyButton = _skin.GetStyle("MiniButtonCopyStyle");
            _pasteButton = _skin.GetStyle("MiniButtonPasteStyle");
            _autoButton = _skin.GetStyle("MiniButtonAutoStyle");
            _createTargetButton = _skin.GetStyle("MiniButtonCreateTargetStyle");
            _enableButton = _skin.GetStyle("MiniButtonEnableStyle");
            _textField = _skin.GetStyle("TextField");
            _dropDown = _skin.GetStyle("DropDownField");
            _hLine = _skin.GetStyle("HorizontalLine");
            _sceneText = _skin.GetStyle("SceneText");
        }

        private void GetDefaultColors()
        {
            if (_defaultTextColor.a == 0)
            {//These are throwing null exception in OnEnable so its cached this way.
                _defaultTextColor = EditorStyles.label.normal.textColor;
                _defaultTextFieldColor = EditorStyles.textField.normal.textColor;
                _defaultMiniButtonTextColor = EditorStyles.miniButton.normal.textColor;
                _defaultToolbarButtonTextColor = EditorStyles.toolbarButton.normal.textColor;
            }
        }

        private void SetGUIColors()
        {
            EditorStyles.label.normal.textColor = _textColor;
            EditorStyles.miniButton.normal.textColor = _textFieldColor;
            EditorStyles.toolbarButton.normal.textColor = _textFieldColor;
            if (EditorGUIUtility.isProSkin)
                EditorStyles.textField.normal.textColor = _textColor;
            else
                EditorStyles.textField.normal.textColor = _textFieldColor;
        }

        private void ResetGUIColors()
        {
            EditorStyles.label.normal.textColor = _defaultTextColor;
            EditorStyles.textField.normal.textColor = _defaultTextFieldColor;
            EditorStyles.miniButton.normal.textColor = _defaultMiniButtonTextColor;
            EditorStyles.toolbarButton.normal.textColor = _defaultToolbarButtonTextColor;
        }

        public static void Repainter()
        {
            Editor[] ed = (Editor[])Resources.FindObjectsOfTypeAll<Editor>();
            for (int i = 0; i < ed.Length; i++)
            {
                if (ed[i].GetType() == typeof(InteractorEditor))
                {
                    if ((ed[i].target as Interactor).gameObject == Selection.activeGameObject)
                    {
                        ed[i].Repaint();
                        return;
                    }
                }
            }
        }

        public void RefreshTabNames()
        {
            _effectorTabs = new string[_script.effectorLinks.Count];
            for (int i = 0; i < _script.effectorLinks.Count; i++)
            {
                if (_script.effectorLinks[i].effectorName == "")
                {
                    _script.effectorLinks[i].effectorName = i.ToString();
                    _effectorTabs[i] = _script.effectorLinks[i].effectorName;
                }
                else _effectorTabs[i] = _script.effectorLinks[i].effectorName;
            }
        }

        public void RefreshTabName(string name)
        {
            _script.effectorLinks[selectedEffectorTab].effectorName = name;
            _effectorTabs[selectedEffectorTab] = name;
        }

        private void RefreshSpawnerWindowSoft()
        {
            if (InteractorTargetSpawner.Instance)
            {
                InteractorTargetSpawner.Instance.Repainter();
            }
        }

        private void RefreshSpawnerWindowHard()
        {
            if (InteractorTargetSpawner.Instance)
            {
                InteractorTargetSpawner.Instance.RefreshTabs();
            }
            else
            {
                InteractorTargetSpawner Instance = EditorWindow.GetWindow<InteractorTargetSpawner>();
                InteractorTargetSpawner.Instance.RefreshTabs();
                Instance.Close();
            }
        }

        private void TopRightLinks()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
#if UNITY_2019_3_OR_NEWER
            GUILayout.Space(0f);
#else
            GUILayout.Space(2f);
#endif
            if (GUILayout.Button(new GUIContent(Links.onlineDocName, Links.onlineDocDesc), _smallButton))
            {
                Links.OnlineDocumentataion();
            }
            if (GUILayout.Button(new GUIContent(Links.forumName, Links.forumDesc), _smallButton))
            {
                Links.Forum();
            }
            if (GUILayout.Button(new GUIContent(Links.messageName, Links.messageDesc), _smallButton))
            {
                Links.Message();
            }
            if (GUILayout.Button(new GUIContent(Links.storeName, Links.storeDesc), _smallButton))
            {
                Links.Store(false);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void UndoRedoRefresh()
        {
            RefreshTabNames();
            SceneView.RepaintAll();
            RefreshSpawnerWindowSoft();
        }

        private void DrawBackground()
        {
#if UNITY_2019_3_OR_NEWER
            windowRect = new Rect(0, 0, EditorGUIUtility.currentViewWidth, windowY + 33f);
#else
            windowRect = new Rect(0, 0, EditorGUIUtility.currentViewWidth, windowY + 30f);
#endif
            GUI.Box(windowRect, "", _background);
        }

        private void ToggleBottomArea(int pushedButton)
        {
            _defaultBottom = false;

            for (int i = 0; i < _toggleBottomElements.Length; i++)
            {
                if (i == pushedButton)
                {
                    if (_toggleBottomElements[i])
                    {
                        _toggleBottomElements[i] = false;
                        _defaultBottom = true;
                        continue;
                    }
                    else
                    {
                        _toggleBottomElements[i] = true;
                        continue;
                    }
                }
                else
                    _toggleBottomElements[i] = false;
            }
        }

        private void BottomArea()
        {
            int active = 0;
            for (int i = 0; i < _toggleBottomElements.Length; i++)
            {
                if (_toggleBottomElements[i])
                {
                    active = i;
                }
            }

            _defaultGuiColor = GUI.color;
            GUI.color = _GuiColor;

            switch (active)
            {
                case 0:
#if UNITY_2019_3_OR_NEWER
                    GUILayout.Space(_verticalSpace - 1);
                    _script.selfInteractionObject = (GameObject)EditorGUILayout.ObjectField("Self Interaction (If exist):", _script.selfInteractionObject, typeof(GameObject), true);
                    GUILayout.Space(_verticalSpace - 1);
#else
                    GUILayout.Space(_verticalSpace);
                    _script.selfInteractionObject = (GameObject)EditorGUILayout.ObjectField("Self Interaction :", _script.selfInteractionObject, typeof(GameObject), true);
                    GUILayout.Space(_verticalSpace);
#endif
                    break;
                case 1:
                    EditorGUI.BeginChangeCheck();
                    GUILayout.Space(_verticalSpace);
                    opacityValue = EditorGUILayout.Slider("Gizmo Opacity :", _script.opacity, 0, 1f, GUILayout.Width(windowWidth));
                    GUILayout.Space(_verticalSpace);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_script, "Interactor Opacity Change");
                        _script.opacity = opacityValue;
                        SceneView.RepaintAll();
                    }
                    break;
                case 2:
                    GUILayout.Space(_verticalSpace);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    _layerName = NarrowTextField("Layer Name : ", _script.layerName);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_script, "Layer Name Change");
                        _script.layerName = _layerName;
                    }
                    EditorGUI.BeginChangeCheck();
                    _raycastDistance = NarrowFloatField("Raycast Distance :", _script.raycastDistance);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_script, "Raycast Distance Change");
                        _script.raycastDistance = _raycastDistance;
                    }
                    EditorGUILayout.EndHorizontal();
#if UNITY_2019_3_OR_NEWER
                    GUILayout.Space(2f);
#else
                    GUILayout.Space(_verticalSpace);
#endif
                    break;
                case 3:
                    GUILayout.Space(_verticalSpace);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60f);
                    EditorGUI.BeginChangeCheck();
                    _lookEnabled = GUILayout.Toggle(_script.lookAtTargetEnabled, "Enable", GUILayout.MaxHeight(17f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_script, "LookAtTarget Change");
                        _script.lookAtTargetEnabled = _lookEnabled;
                    }
                    GUILayout.Space(40f);
                    _script.alternateHead = (Transform)EditorGUILayout.ObjectField(_script.alternateHead, typeof(Transform), true);
                    EditorGUILayout.LabelField("Alternative Head", GUILayout.MaxWidth(100f));
                    EditorGUILayout.EndHorizontal();
#if UNITY_2019_3_OR_NEWER
                    GUILayout.Space(_verticalSpace);
#else
                    GUILayout.Space(5f);
#endif
                    break;
                case 4:
                    GUILayout.Space(_verticalSpace);
                    EditorGUILayout.BeginHorizontal();
                    if (WindowGUI.ButtonValue(Links.interactorScriptName, Links.interactorScriptDesc))
                    {
                        Links.InteractorScript();
                        return;
                    }
                    if (WindowGUI.ButtonValue(Links.interactorEditorScriptName, Links.interactorEditorScriptDesc))
                    {
                        Links.InteractorEditorScript();
                        return;
                    }
                    EditorGUILayout.EndHorizontal();
#if UNITY_2019_3_OR_NEWER
                    GUILayout.Space(2f);
#else
                    GUILayout.Space(5f);
#endif
                    break;
            }
            GUI.color = _defaultGuiColor;
        }
        public string NarrowTextField(string label, string text)
        {
            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(label));
            EditorGUIUtility.labelWidth = textDimensions.x;
            return EditorGUILayout.TextField(label, text);
        }
        public float NarrowFloatField(string label, float value)
        {
            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(label));
            EditorGUIUtility.labelWidth = textDimensions.x;
            return EditorGUILayout.FloatField(label, value);
        }
        #endregion

        public override void OnInspectorGUI()
        {
            if (!handlesOn && _script.debug)
            {
#if UNITY_2019_1_OR_NEWER
                SceneView.duringSceneGui += ShowHandles;
#else
                SceneView.onSceneGUIDelegate += ShowHandles;
#endif
                handlesOn = true;
            }

            GetDefaultColors();
            GUI.skin = _skin;
            DrawBackground();

            SetGUIColors();
            _frontPage = false;

            _logoChange = _script.logoChange;
            if (!_logoChange && _script.effectorLinks.Count != 0)
            {
                _logo = new Rect(1f, 4f, _logoBig.width, _logoBig.height);
                _logoSpace = _logoBig.height + 10f;
                _logoAreaSpace = _logoSmall.height - 16f;
                GUI.DrawTexture(_logo, _logoBig);
                TopRightLinks();

                if (Event.current.type == EventType.MouseUp && _logo.Contains(Event.current.mousePosition))
                {
                    _script.logoChange = !_logoChange;
                    EditorUtility.SetDirty(_script);
                    this.Repaint();
                }
            }
            else if (_script.effectorLinks.Count != 0)
            {
                _logo = new Rect(8f, 0, _logoSmall.width, _logoSmall.height);
                _logoSpace = _logoSmall.height;
                _logoAreaSpace = _logoBig.height + 12f; //16 is same space with big logo

                EditorGUILayout.BeginHorizontal();
                GUI.DrawTexture(_logo, _logoSmall);
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical();
#if UNITY_2019_3_OR_NEWER
#else
                GUILayout.Space(4f);
#endif
                EditorGUI.BeginChangeCheck();
                _debug = GUILayout.Toggle(_script.debug, "Debug View ");
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_script, "Interactor Debug");
                    _script.debug = _debug;

#if UNITY_2019_1_OR_NEWER
                    if (_debug)
                    {
                        SceneView.duringSceneGui += ShowHandles;
                        handlesOn = true;
                    }
                    else
                    {
                        SceneView.duringSceneGui -= ShowHandles;
                        handlesOn = false;
                    }
#else
                if (_debug)
                {
                    SceneView.onSceneGUIDelegate += ShowHandles;
                    handlesOn = true;
                }
                else
                {
                    SceneView.onSceneGUIDelegate -= ShowHandles;
                    handlesOn = false;
                }
#endif
                    SceneView.RepaintAll();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (Event.current.type == EventType.MouseUp && _logo.Contains(Event.current.mousePosition))
                {
                    _script.logoChange = !_logoChange;
                    EditorUtility.SetDirty(_script);
                    this.Repaint();
                }
            }
            #region Front Page
            else
            {
                _frontPage = true;
                _logoSpace = _logoBig.height + 10f;
                _logo = new Rect((EditorGUIUtility.currentViewWidth - _logoBig.width) * 0.5f, 3f, _logoBig.width, _logoBig.height);
                GUI.DrawTexture(_logo, _logoBig);
                TopRightLinks();
                GUILayout.Space(2f);

#if UNITY_2019_3_OR_NEWER
                windowY = 256f;
#else
                windowY = 245f;
#endif
                _tabButtonStyle.fixedWidth = EditorGUIUtility.currentViewWidth - 2f;

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                int leftBorder = _tabButtonStyle.border.left;
                int rightBorder = _tabButtonStyle.border.right;
                _tabButtonStyle.border.left = 2;
                _tabButtonStyle.border.right = 2;
                GUILayout.BeginArea(new Rect(0, _logoSpace, EditorGUIUtility.currentViewWidth + 20, 100f));
                if (GUILayout.Button("Start Interactor", _tabButtonStyle))
                {
                    Undo.RecordObject(_script, "Added First Effector");

                    if (InteractorTargetSpawner.Instance)
                        InteractorTargetSpawner.Instance.Close();
                    _script.savePath = null;
                    _script.effectorLinks.Add(new Interactor.EffectorLink());
                    _script.effectorLinks[0].effectorName = "New Effector";
                    RefreshTabNames();
                    RefreshSpawnerWindowHard();

                    if (InteractorTargetSpawner.Instance.HasSaveData())
                    {
                        if (!_script.debug)
                        {
                            _script.debug = true;
                            _debug = true;
#if UNITY_2019_1_OR_NEWER
                            if (_debug)
                            {
                                SceneView.duringSceneGui += ShowHandles;
                                handlesOn = true;
                            }
                            else
                            {
                                SceneView.duringSceneGui -= ShowHandles;
                                handlesOn = false;
                            }
#else
                            if (_debug)
                            {
                                SceneView.onSceneGUIDelegate += ShowHandles;
                                handlesOn = true;
                            }
                            else
                            {
                                SceneView.onSceneGUIDelegate -= ShowHandles;
                                handlesOn = false;
                            }
#endif
                        }
                        SceneView.RepaintAll();
                    }
                    else
                    {
                        _script.effectorLinks.RemoveAt(selectedEffectorTab);
                        RefreshSpawnerWindowSoft();
                    }
                    _tabButtonStyle.border.left = leftBorder;
                    _tabButtonStyle.border.right = rightBorder;
                    _tabButtonStyle.fixedWidth = _tabButtonStyleWidth;
                    GUIUtility.ExitGUI();
                }
                _tabButtonStyle.border.left = leftBorder;
                _tabButtonStyle.border.right = rightBorder;
                _tabButtonStyle.fixedWidth = _tabButtonStyleWidth;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.EndArea();

                GUILayout.Space(55f);

                if (windowRect.Contains(Event.current.mousePosition))
                    this.Repaint();

                GUILayout.Space(10f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Start Interactor to add your first effector.");
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(30f);
                EditorGUILayout.LabelField("Install Integrations", _hLine);

                CheckDefaultFiles();

                EditorGUILayout.BeginHorizontal();
                if (WindowGUI.ButtonValue("Final IK", "Before installing the integration, if you don't imported Final IK yet, import it first. You can change versions between Default version and Final IK anytime."))
                {
                    if (!_defaultFiles)
                    {
                        Debug.LogWarning("Final IK integration already installed.");
                        return;
                    }
                    _installFik = true;
                }
                EditorGUI.BeginDisabledGroup(true);
                if (WindowGUI.ButtonValue("Unity Animation Rigging", "Since it is using Burst and they are both changing constantly, this will take time."))
                {
                    Debug.Log("Unity Animation Rigging is not supported yet because it is still in development.");
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (WindowGUI.ButtonValue("Default Interactor IK", "If you installed Final IK integration, this will change back to default version with its examples."))
                {
                    if (_defaultFiles)
                    {
                        Debug.LogWarning("Default Interactor IK already installed.");
                        return;
                    }
                    _installDefault = true;
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10f);

                ResetGUIColors();

                if (Event.current.type == EventType.Repaint && _frontPage)
                {
                    InstallDefault();
                    InstallFinalIK();
                }
                return;
            }
            #endregion

            GUILayout.Space(_logoSpace);

            #region Effector Page
#if UNITY_2019_3_OR_NEWER
            GUILayout.BeginArea(new Rect(0, _logoSpace, windowWidth + 23, 100f));
            EditorGUI.BeginChangeCheck();
            _tabButtonStyle.fixedWidth = (windowWidth + 23) / (_script.effectorLinks.Count);
#else
            GUILayout.BeginArea(new Rect(0, _logoSpace, windowWidth + 20, 100f));
            EditorGUI.BeginChangeCheck();
            _tabButtonStyle.fixedWidth = (windowWidth + 20) / (_script.effectorLinks.Count);
#endif
            selectedEffectorTab = GUILayout.Toolbar(_script.selectedTab, _effectorTabs, _tabButtonStyle);
            _effectorlink = _script.effectorLinks[selectedEffectorTab];
            if (EditorGUI.EndChangeCheck())
            {
                if (selectedEffectorTab >= _script.effectorLinks.Count)
                {
                    selectedEffectorTab = _script.effectorLinks.Count - 1;
                }
                _script.selectedTab = selectedEffectorTab;
                GUI.FocusControl(null);
                SceneView.RepaintAll();
                RefreshSpawnerWindowSoft();
            }
            _tabButtonStyle.fixedWidth = _tabButtonStyleWidth;

            if (_logoChange)
            {
#if UNITY_2019_3_OR_NEWER
                windowY = 57f;
#else
                windowY = 55f;
#endif
                GUILayout.EndArea();
                GUILayout.Space(_logoAreaSpace - windowY - 20f);
                if (windowRect.Contains(Event.current.mousePosition))
                {
                    this.Repaint();
                }

                windowWidth = GetViewWidth();
                ResetGUIColors();

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(_script);
                }
                return;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Add New", "Add New Effector"), _addButton))
            {
                bool spawnerWasOpen = false;
                if (!InteractorTargetSpawner.Instance) //Opening Spawner window because it is responsible for saving and loading saveData
                    EditorWindow.GetWindow<InteractorTargetSpawner>();
                else spawnerWasOpen = true;

                _script.effectorLinks.Insert(selectedEffectorTab + 1, new Interactor.EffectorLink());
                _script.effectorLinks[selectedEffectorTab + 1].effectorName = "New Effector";
                RefreshTabNames();
                RefreshSpawnerWindowHard();

                if (!spawnerWasOpen && InteractorTargetSpawner.Instance)
                    InteractorTargetSpawner.Instance.Close();
                if (selectedEffectorTab < _script.effectorLinks.Count)
                {
                    selectedEffectorTab++;
                    _script.selectedTab = selectedEffectorTab;
                }
                GUIUtility.ExitGUI();
            }
            if (GUILayout.Button(new GUIContent("Delete This", "Delete Current Effector"), _deleteButton))
            {
                bool spawnerWasOpen = false;
                if (!InteractorTargetSpawner.Instance)
                    EditorWindow.GetWindow<InteractorTargetSpawner>();
                else spawnerWasOpen = true;

                _script.effectorLinks.RemoveAt(selectedEffectorTab);
                RefreshTabNames();
                if (_script.effectorLinks.Count >= 0)
                    RefreshSpawnerWindowHard();

                if (!spawnerWasOpen && InteractorTargetSpawner.Instance)
                    InteractorTargetSpawner.Instance.Close();
                if (selectedEffectorTab > 0)
                {
                    selectedEffectorTab--;
                    _script.selectedTab = selectedEffectorTab;
                }
                GUIUtility.ExitGUI();
            }
            if (GUILayout.Button(new GUIContent("Copy", "Copy Current Effector Rules"), _copyButton))
            {
                _tempEnabled = _script.effectorLinks[selectedEffectorTab].enabled;
                _effectorPos = _script.effectorLinks[selectedEffectorTab].posOffset;
                _hRange = _script.effectorLinks[selectedEffectorTab].angleXZ;
                _hRangeOff = _script.effectorLinks[selectedEffectorTab].angleOffset;
                _vRange = _script.effectorLinks[selectedEffectorTab].angleYZ;
                _vRangeOff = _script.effectorLinks[selectedEffectorTab].angleOffsetYZ;
                _effectorMaxDist = _script.effectorLinks[selectedEffectorTab].maxRadius;
                _effectorMinDist = _script.effectorLinks[selectedEffectorTab].minRadius;
                _copiedTab = true;
            }
            if (GUILayout.Button(new GUIContent("Paste", "Paste Copied Effector Rules To Current"), _pasteButton))
            {
                if (_copiedTab)
                {
                    Undo.RecordObject(_script, "Interactor Effector Paste");
                    _script.effectorLinks[selectedEffectorTab].enabled = _tempEnabled;
                    _script.effectorLinks[selectedEffectorTab].posOffset = _effectorPos;
                    _script.effectorLinks[selectedEffectorTab].angleXZ = _hRange;
                    _script.effectorLinks[selectedEffectorTab].angleOffset = _hRangeOff;
                    _script.effectorLinks[selectedEffectorTab].angleYZ = _vRange;
                    _script.effectorLinks[selectedEffectorTab].angleOffsetYZ = _vRangeOff;
                    _script.effectorLinks[selectedEffectorTab].maxRadius = _effectorMaxDist;
                    _script.effectorLinks[selectedEffectorTab].minRadius = _effectorMinDist;
                    EditorUtility.SetDirty(_script);
                    _copiedTab = false;
                }
                else Debug.LogWarning("You need to copy from an effector first.", _script);
            }
            if (GUILayout.Button(new GUIContent("Auto", "Set Rules With Auto Configure For Humanoid Avatars, Select The Effector Type First"), _autoButton))
            {
                Interactor.FullBodyBipedEffector type = _script.effectorLinks[selectedEffectorTab].effectorType;
                Animator animator;
                if (animator = _script.GetComponentInChildren<Animator>())
                {
                    AdjustCollider(animator);
                    Transform effector;
                    Vector3 center = _script.transform.position + _script.sphereCol.center;
                    Vector3 forwardDir = _script.transform.forward;
                    Vector3 upDir = _script.transform.up;

                    if ((int)type == 0)
                    {
                        effector = animator.GetBoneTransform(HumanBodyBones.Spine);
                        if (effector)
                        {
                            Undo.RecordObject(_script, "Interactor Auto Configure");

                            _script.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                            Transform foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                            float maxDist = 0.5f;
                            if (foot)
                                maxDist = (effector.position.y - foot.position.y) * 0.7f;
                            else
                                maxDist = (_script.transform.position.y - effector.position.y) * 0.7f;

                            _script.effectorLinks[selectedEffectorTab].posOffset = -(center - effector.position);
                            _script.effectorLinks[selectedEffectorTab].angleXZ = 140f;
                            _script.effectorLinks[selectedEffectorTab].angleOffset = 20f;
                            _script.effectorLinks[selectedEffectorTab].angleYZ = 160f;
                            _script.effectorLinks[selectedEffectorTab].angleOffsetYZ = 10f;
                            _script.effectorLinks[selectedEffectorTab].maxRadius = maxDist;
                            _script.effectorLinks[selectedEffectorTab].minRadius = maxDist * 0.1f;
                            _script.transform.rotation = Quaternion.LookRotation(forwardDir, upDir);
                            EditorUtility.SetDirty(_script);
                        }
                        else
                        {
                            Debug.LogWarning("Spine bone could not find on this Avatar. You can set the effector rules manually.", _script);
                        }
                    }
                    else if ((int)type == 5)
                    {
                        effector = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                        if (effector)
                        {
                            Transform lowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                            Transform hand = animator.GetBoneTransform(HumanBodyBones.LeftHand);

                            if (lowerArm && hand)
                            {
                                Undo.RecordObject(_script, "Interactor Auto Configure");

                                _script.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                                float maxDist = 0.5f;
                                maxDist = Vector3.Distance(effector.position, lowerArm.position) + Vector3.Distance(lowerArm.position, hand.position) + 0.2f;

                                _script.effectorLinks[selectedEffectorTab].posOffset = -(center - effector.position);
                                _script.effectorLinks[selectedEffectorTab].angleXZ = 130f;
                                _script.effectorLinks[selectedEffectorTab].angleOffset = -10f;
                                _script.effectorLinks[selectedEffectorTab].angleYZ = 120f;
                                _script.effectorLinks[selectedEffectorTab].angleOffsetYZ = 30f;
                                _script.effectorLinks[selectedEffectorTab].maxRadius = maxDist;
                                _script.effectorLinks[selectedEffectorTab].minRadius = maxDist * 0.3f;
                                _script.transform.rotation = Quaternion.LookRotation(forwardDir, upDir);
                                EditorUtility.SetDirty(_script);
                            }
                            else
                            {
                                Debug.LogWarning("LeftLowerArm or LeftHand bone could not find on this Avatar. You can set the effector rules manually.", _script);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("LeftUpperArm bone could not find on this Avatar. You can set the effector rules manually.", _script);
                        }
                    }
                    else if ((int)type == 6)
                    {
                        effector = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                        if (effector)
                        {
                            Transform lowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                            Transform hand = animator.GetBoneTransform(HumanBodyBones.RightHand);

                            if (lowerArm && hand)
                            {
                                Undo.RecordObject(_script, "Interactor Auto Configure");

                                _script.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                                float maxDist = 0.5f;
                                maxDist = Vector3.Distance(effector.position, lowerArm.position) + Vector3.Distance(lowerArm.position, hand.position) + 0.2f;

                                _script.effectorLinks[selectedEffectorTab].posOffset = -(center - effector.position);
                                _script.effectorLinks[selectedEffectorTab].angleXZ = 130f;
                                _script.effectorLinks[selectedEffectorTab].angleOffset = 60f;
                                _script.effectorLinks[selectedEffectorTab].angleYZ = 120f;
                                _script.effectorLinks[selectedEffectorTab].angleOffsetYZ = 30f;
                                _script.effectorLinks[selectedEffectorTab].maxRadius = maxDist;
                                _script.effectorLinks[selectedEffectorTab].minRadius = maxDist * 0.3f;
                                _script.transform.rotation = Quaternion.LookRotation(forwardDir, upDir);
                                EditorUtility.SetDirty(_script);
                            }
                            else
                            {
                                Debug.LogWarning("RightLowerArm or RightHand bone could not find on this Avatar. You can set the effector rules manually.", _script);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("RightUpperArm bone could not find on this Avatar. You can set the effector rules manually.", _script);
                        }
                    }
                    else if ((int)type == 7)
                    {
                        effector = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                        if (effector)
                        {
                            Transform lowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                            Transform foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);

                            if (lowerLeg && foot)
                            {
                                Undo.RecordObject(_script, "Interactor Auto Configure");

                                _script.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                                float maxDist = 0.5f;
                                maxDist = Vector3.Distance(effector.position, lowerLeg.position) + Vector3.Distance(lowerLeg.position, foot.position) + 0.05f;
                                Vector3 effectorPos = (foot.position + lowerLeg.position) * 0.5f;
                                effectorPos = new Vector3(foot.position.x, effectorPos.y, foot.position.z - 0.05f);

                                _script.effectorLinks[selectedEffectorTab].posOffset = -(center - effectorPos);
                                _script.effectorLinks[selectedEffectorTab].angleXZ = 100f;
                                _script.effectorLinks[selectedEffectorTab].angleOffset = 45f;
                                _script.effectorLinks[selectedEffectorTab].angleYZ = 100f;
                                _script.effectorLinks[selectedEffectorTab].angleOffsetYZ = 45f;
                                _script.effectorLinks[selectedEffectorTab].maxRadius = maxDist * 0.8f;
                                _script.effectorLinks[selectedEffectorTab].minRadius = maxDist * 0.1f;
                                _script.transform.rotation = Quaternion.LookRotation(forwardDir, upDir);
                                EditorUtility.SetDirty(_script);
                            }
                            else
                            {
                                Debug.LogWarning("LeftLowerLeg or LeftFoot bone could not find on this Avatar. You can set the effector rules manually.", _script);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("LeftUpperLeg bone could not find on this Avatar. You can set the effector rules manually.", _script);
                        }
                    }
                    else if ((int)type == 8)
                    {
                        effector = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                        if (effector)
                        {
                            Transform lowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                            Transform foot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

                            if (lowerLeg && foot)
                            {
                                Undo.RecordObject(_script, "Interactor Auto Configure");

                                _script.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                                float maxDist = 0.5f;
                                maxDist = Vector3.Distance(effector.position, lowerLeg.position) + Vector3.Distance(lowerLeg.position, foot.position) + 0.05f;
                                Vector3 effectorPos = (foot.position + lowerLeg.position) * 0.5f;
                                effectorPos = new Vector3(foot.position.x, effectorPos.y, foot.position.z - 0.05f);

                                _script.effectorLinks[selectedEffectorTab].posOffset = -(center - effectorPos);
                                _script.effectorLinks[selectedEffectorTab].angleXZ = 100f;
                                _script.effectorLinks[selectedEffectorTab].angleOffset = 45f;
                                _script.effectorLinks[selectedEffectorTab].angleYZ = 100f;
                                _script.effectorLinks[selectedEffectorTab].angleOffsetYZ = 45f;
                                _script.effectorLinks[selectedEffectorTab].maxRadius = maxDist * 0.8f;
                                _script.effectorLinks[selectedEffectorTab].minRadius = maxDist * 0.1f;
                                _script.transform.rotation = Quaternion.LookRotation(forwardDir, upDir);
                                EditorUtility.SetDirty(_script);
                            }
                            else
                            {
                                Debug.LogWarning("RightLowerLeg or RightFoot bone could not find on this Avatar. You can set the effector rules manually.", _script);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("RightUpperLeg bone could not find on this Avatar. You can set the effector rules manually.", _script);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Auto configuration is only possible for these Effector Types: Body, Left Hand, Right Hand, Left Foot, Right Foot", _script);
                    }
                }
                else
                {
                    Debug.LogWarning("Interactor could not find Animator component on this object or any of children of it. You can set the effector rules manually.", _script);
                }
            }
            if (GUILayout.Button(new GUIContent("Create Target", "Create targets automatically from your character with selected effector type."), _createTargetButton))
            {
                Interactor.FullBodyBipedEffector type = _script.effectorLinks[selectedEffectorTab].effectorType;
                Animator animator;
                if (animator = _script.GetComponentInChildren<Animator>())
                {
                    Transform target;
                    Transform createdTarget = null;
                    if ((int)type == 0)
                    {
                        target = animator.GetBoneTransform(HumanBodyBones.Spine);
                        if (target)
                        {
                            Undo.IncrementCurrentGroup();

                            string targetName = _script.effectorLinks[selectedEffectorTab].effectorName;
                            if (targetName == "New Effector") targetName = "BodyTarget";
                            else targetName += "Target";
                            createdTarget = new GameObject(targetName).transform;
                            Undo.RegisterCreatedObjectUndo(createdTarget.gameObject, "Interactor Create Target");
                            createdTarget.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                            InteractorTarget interactorTarget = createdTarget.gameObject.AddComponent<InteractorTarget>();
                            interactorTarget.effectorType = Interactor.FullBodyBipedEffector.Body;
                            interactorTarget.speedDebug = 0;

                            Undo.SetCurrentGroupName("Interactor Create Target");
                            EditorUtility.SetDirty(createdTarget.gameObject);
                            Debug.Log(targetName + " has been created in empty hierarchy at origin point of the scene. You can adjust its position and rotation before creating its prefab.", createdTarget);
                        }
                        else
                        {
                            Debug.LogWarning("Spine bone could not find on this Avatar. You can create the target manually.", _script);
                        }
                    }
                    else if ((int)type == 5)
                    {
                        target = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                        if (target)
                        {
                            Undo.IncrementCurrentGroup();

                            string targetName = _script.effectorLinks[selectedEffectorTab].effectorName;
                            if (targetName == "New Effector") targetName = "LeftHandTarget";
                            else targetName += "Target";
                            createdTarget = DuplicateHierarchy(target, null, true);
                            Undo.RegisterCreatedObjectUndo(createdTarget.gameObject, "Interactor Create Target");
                            createdTarget.name = targetName;
                            createdTarget.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                            InteractorTarget interactorTarget = createdTarget.gameObject.AddComponent<InteractorTarget>();
                            interactorTarget.effectorType = Interactor.FullBodyBipedEffector.LeftHand;
                            interactorTarget.speedDebug = 0;

                            Undo.SetCurrentGroupName("Interactor Create Target");
                            EditorUtility.SetDirty(createdTarget.gameObject);
                            Debug.Log(targetName + " has been created in empty hierarchy at origin point of the scene. You can adjust its position and rotation before creating its prefab.", createdTarget);
                        }
                        else
                        {
                            Debug.LogWarning("LeftHand bone could not find on this Avatar. You can create the target manually.", _script);
                        }
                    }
                    else if ((int)type == 6)
                    {
                        target = animator.GetBoneTransform(HumanBodyBones.RightHand);
                        if (target)
                        {
                            Undo.IncrementCurrentGroup();

                            string targetName = _script.effectorLinks[selectedEffectorTab].effectorName;
                            if (targetName == "New Effector") targetName = "RightHandTarget";
                            else targetName += "Target";
                            createdTarget = DuplicateHierarchy(target, null, true);
                            Undo.RegisterCreatedObjectUndo(createdTarget.gameObject, "Interactor Create Target");
                            createdTarget.name = targetName;
                            createdTarget.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                            InteractorTarget interactorTarget = createdTarget.gameObject.AddComponent<InteractorTarget>();
                            interactorTarget.effectorType = Interactor.FullBodyBipedEffector.RightHand;
                            interactorTarget.speedDebug = 0;

                            Undo.SetCurrentGroupName("Interactor Create Target");
                            EditorUtility.SetDirty(createdTarget.gameObject);
                            Debug.Log(targetName + " has been created in empty hierarchy at origin point of the scene. You can adjust its position and rotation before creating its prefab.", createdTarget);
                        }
                        else
                        {
                            Debug.LogWarning("RightHand bone could not find on this Avatar. You can create the target manually.", _script);
                        }
                    }
                    else if ((int)type == 7)
                    {
                        target = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                        if (target)
                        {
                            Undo.IncrementCurrentGroup();

                            string targetName = _script.effectorLinks[selectedEffectorTab].effectorName;
                            if (targetName == "New Effector") targetName = "LeftFootTarget";
                            else targetName += "Target";
                            createdTarget = DuplicateHierarchy(target, null, true);
                            Undo.RegisterCreatedObjectUndo(createdTarget.gameObject, "Interactor Create Target");
                            createdTarget.name = targetName;
                            createdTarget.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                            InteractorTarget interactorTarget = createdTarget.gameObject.AddComponent<InteractorTarget>();
                            interactorTarget.effectorType = Interactor.FullBodyBipedEffector.LeftFoot;
                            interactorTarget.speedDebug = 0;

                            Undo.SetCurrentGroupName("Interactor Create Target");
                            EditorUtility.SetDirty(createdTarget.gameObject);
                            Debug.Log(targetName + " has been created in empty hierarchy at origin point of the scene. You can adjust its position and rotation before creating its prefab.", createdTarget);
                        }
                        else
                        {
                            Debug.LogWarning("LeftFoot bone could not find on this Avatar. You can create the target manually.", _script);
                        }
                    }
                    else if ((int)type == 8)
                    {
                        target = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                        if (target)
                        {
                            Undo.IncrementCurrentGroup();

                            string targetName = _script.effectorLinks[selectedEffectorTab].effectorName;
                            if (targetName == "New Effector") targetName = "RightFootTarget";
                            else targetName += "Target";
                            createdTarget = DuplicateHierarchy(target, null, true);
                            Undo.RegisterCreatedObjectUndo(createdTarget.gameObject, "Interactor Create Target");
                            createdTarget.name = targetName;
                            createdTarget.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                            InteractorTarget interactorTarget = createdTarget.gameObject.AddComponent<InteractorTarget>();
                            interactorTarget.effectorType = Interactor.FullBodyBipedEffector.RightFoot;
                            interactorTarget.speedDebug = 0;

                            Undo.SetCurrentGroupName("Interactor Create Target");
                            EditorUtility.SetDirty(createdTarget.gameObject);
                            Debug.Log(targetName + " has been created in empty hierarchy at origin point of the scene. You can adjust its position and rotation before creating its prefab.", createdTarget);
                        }
                        else
                        {
                            Debug.LogWarning("RightFoot bone could not find on this Avatar. You can create the target manually.", _script);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Create Target is only possible for these Effector Types: Body, Left Hand, Right Hand, Left Foot, Right Foot", _script);
                    }
                }
                else
                {
                    Debug.LogWarning("Interactor could not find Animator component on this object or any of children of it. You can create your targets manually.", _script);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            _enabled = GUILayout.Toggle(_effectorlink.enabled, new GUIContent(" Enabled", "Enable Or Disable Current Effector"), _enableButton);
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Interactor Effector Change");
                GUI.FocusControl(null);
                _effectorlink.enabled = _enabled;
                SceneView.RepaintAll();
            }
            GUILayout.EndArea();

            GUILayout.Space(_logoAreaSpace);

            _defaultGuiColor = GUI.color;
            GUI.color = _GuiColor;

            EditorGUI.BeginChangeCheck();
            GUILayout.Space(_verticalSpace);
            _effectorName = EditorGUILayout.TextField("Name :", _effectorlink.effectorName, _textField);
            GUILayout.Space(_verticalSpace);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Interactor Name Change");
                RefreshTabName(_effectorName);
                RefreshSpawnerWindowSoft();
                SceneView.RepaintAll();
            }

            if (_script.sphereCol == null) _script.sphereCol = _script.GetComponent<SphereCollider>();

            EditorGUI.BeginChangeCheck();
            _effectorType = (Interactor.FullBodyBipedEffector)EditorGUILayout.EnumPopup("Effector Type :", _effectorlink.effectorType, _dropDown);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Effector Type Change");
                _effectorlink.effectorType = _effectorType;
                SceneView.RepaintAll();
            }

            GUILayout.Space(_verticalSpace);

            EditorGUI.BeginChangeCheck();
            _posOffset = EditorGUILayout.Vector3Field("Effector Position :", _effectorlink.posOffset);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Effector Position Change");
                _effectorlink.posOffset = _posOffset;
                SceneView.RepaintAll();
            }

            GUILayout.Space(_verticalSpace);

            EditorGUI.BeginChangeCheck();
            _angleXZ = EditorGUILayout.Slider("Horizontal Angle :", _effectorlink.angleXZ, 0, 360f, GUILayout.Width(windowWidth));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Effector H.Angle Change");
                _effectorlink.angleXZ = _angleXZ;
                SceneView.RepaintAll();
            }

            GUILayout.Space(_verticalSpace);

            EditorGUI.BeginChangeCheck();
            _angleOffset = EditorGUILayout.Slider("Horizontal Offset :", _effectorlink.angleOffset, -180f, 180f, GUILayout.Width(windowWidth));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Effector H.Offset Change");
                _effectorlink.angleOffset = _angleOffset;
                SceneView.RepaintAll();
            }

            GUILayout.Space(_verticalSpace);

            EditorGUI.BeginChangeCheck();
            _angleYZ = EditorGUILayout.Slider("Vertical Angle :", _effectorlink.angleYZ, 0, 360f, GUILayout.Width(windowWidth));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Effector V.Angle Change");
                _effectorlink.angleYZ = _angleYZ;
                SceneView.RepaintAll();
            }

            GUILayout.Space(_verticalSpace);

            EditorGUI.BeginChangeCheck();
            _angleOffsetYZ = EditorGUILayout.Slider("Vertical Offset :", _effectorlink.angleOffsetYZ, -180f, 180f, GUILayout.Width(windowWidth));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Effector V.Offset Change");
                _effectorlink.angleOffsetYZ = _angleOffsetYZ;
                SceneView.RepaintAll();
            }

            GUILayout.Space(_verticalSpace);

            EditorGUI.BeginChangeCheck();
            _maxRadius = EditorGUILayout.Slider("Effector Max Range :", _effectorlink.maxRadius, 0, _script.sphereCol.radius, GUILayout.Width(windowWidth));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Effector Distance Change");
                _effectorlink.maxRadius = _maxRadius;
                if (_effectorlink.minRadius > _effectorlink.maxRadius)
                    _effectorlink.minRadius = _effectorlink.maxRadius;
                SceneView.RepaintAll();
            }

            GUILayout.Space(_verticalSpace);

            EditorGUI.BeginChangeCheck();
            _minRadius = EditorGUILayout.Slider("Effector Min Range :", _effectorlink.minRadius, 0, _effectorlink.maxRadius, GUILayout.Width(windowWidth));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Effector Distance Change");
                _effectorlink.minRadius = _minRadius;
                SceneView.RepaintAll();
            }

            GUI.color = _defaultGuiColor;
            GUILayout.Space(31f);

            EditorGUILayout.LabelField("Other Options", _hLine);

            EditorGUILayout.BeginHorizontal();
            if (WindowGUI.ButtonValue("Spawner Window", "Edit target spawner list and enable SceneView window/menu."))
            {
                if (InteractorTargetSpawner.Instance)
                    InteractorTargetSpawner.Instance.Close();
                else EditorWindow.GetWindow<InteractorTargetSpawner>();
            }
            if (WindowGUI.ButtonValue("Self Interaction", "Show Self Interaction object. If you want to use one, assign below."))
            {
                ToggleBottomArea(0);
            }
            if (WindowGUI.ButtonValue("Gizmo Opacity", "Show slider for Gizmo opacity."))
            {
                ToggleBottomArea(1);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (WindowGUI.ButtonValue("Layer / Raycast", "Set a custom layer name other than \"Player\". Set raycast lenght for distance interactions (0 for disable)."))
            {
                ToggleBottomArea(2);
            }
            if (WindowGUI.ButtonValue("LookAtTarget", "Enables looking at interaction targets."))
            {
                ToggleBottomArea(3);
            }
            if (WindowGUI.ButtonValue("Codes", "Open Interactor or InteractorEditor scripts."))
            {
                ToggleBottomArea(4);
            }
            EditorGUILayout.EndHorizontal();

            if (_defaultBottom)
            {
                GUILayout.Space(26f);
            }
            else
            {
                BottomArea();
            }
            #endregion

            //
            //You can expose properties from Interactor (_script) here, you can use GUIStyles cached in GetStyles()
            //
            //Examples
            //_effectorlink.effectorName = EditorGUILayout.TextField("Name :", _effectorlink.effectorName, _textField);
            //_effectorlink.enabled = GUILayout.Toggle(_effectorlink.enabled, " Enabled", _enableButton);
            //

            #region Bottom
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            _debug = GUILayout.Toggle(_script.debug, "Debug View ");
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Interactor Debug");
                _script.debug = _debug;

#if UNITY_2019_1_OR_NEWER
                if (_debug)
                {
                    SceneView.duringSceneGui += ShowHandles;
                    handlesOn = true;
                }
                else
                {
                    SceneView.duringSceneGui -= ShowHandles;
                    handlesOn = false;
                }
#else
                if (_debug)
                {
                    SceneView.onSceneGUIDelegate += ShowHandles;
                    handlesOn = true;
                }
                else
                {
                    SceneView.onSceneGUIDelegate -= ShowHandles;
                    handlesOn = false;
                }
#endif
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (windowRect.Contains(Event.current.mousePosition))
            {
                this.Repaint();
            }

            ResetGUIColors();

            //Getting UI parameters to auto adjust
            if (Event.current.type == EventType.Repaint)
            {
                //windowX = GUILayoutUtility.GetLastRect().x;
                windowY = GUILayoutUtility.GetLastRect().y;
                windowWidth = GUILayoutUtility.GetLastRect().width;
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_script);
            }
            #endregion
        }

        private Transform DuplicateHierarchy(Transform source, Transform targetParent, bool mainParent)
        {
            Transform duplicateBone = null;
            if (mainParent)
            {
                duplicateBone = Instantiate(source, targetParent);
                Undo.RegisterCreatedObjectUndo(duplicateBone.gameObject, "Create child");
                duplicateBone.localPosition = source.localPosition;
                duplicateBone.localRotation = source.localRotation;
                duplicateBone.localScale = source.localScale;
            }

            for (int i = 1; i < source.childCount; i++)
            {
                DuplicateHierarchy(source.GetChild(i), source, false);
            }
            return duplicateBone;
        }

        private Rect _temp;
        private float GetViewWidth()
        {
            GUILayout.Label("", GUILayout.MaxHeight(0));
            if (Event.current.type == EventType.Repaint)
                _temp = GUILayoutUtility.GetLastRect();

            return _temp.width;
        }

        private void AdjustCollider(Animator animator)
        {
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            Transform foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            if (head && foot)
            {
                if (_script.sphereCol == null)
                    _script.sphereCol = _script.GetComponent<SphereCollider>();

                Vector3 forwardDir = _script.transform.forward;
                Vector3 upDir = _script.transform.up;
                _script.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                float dist = Mathf.Abs(head.position.y - foot.position.y) * 0.9f;
                float y = (head.position.y + foot.position.y) * 0.5f - _script.transform.position.y;
                Vector3 center = new Vector3(0, y, 0);
                _script.transform.rotation = Quaternion.LookRotation(forwardDir, upDir);

                bool check = Mathf.Approximately(dist, _script.sphereCol.radius);
                bool check2 = Mathf.Approximately(center.y, _script.sphereCol.center.y);
                if (!check || !check2)
                {
                    Undo.RecordObject(_script.sphereCol, "Interactor Collider Adjustment");

                    _script.sphereCol.center = center;
                    _script.sphereCol.radius = dist;
                    _script.sphereCol.isTrigger = true;
                    Debug.Log("Sphere Collider is set as trigger and adjusted to body center and size.", _script);
                }
            }
        }

        #region Integrations and File Handling
        private void CheckDefaultFiles()
        {
            if (InteractorIK.defaultFiles == 0) _defaultFiles = true;
            else _defaultFiles = false;
        }

        private void InstallDefault()
        {
            if (!_installDefault) return;

            if (InteractorTargetSpawner.Instance)
                InteractorTargetSpawner.Instance.Close();

            _installDefault = false;
            string defaultPack = "Assets/Interactor/Integrations/DefaultPack.unitypackage";
            if (System.IO.File.Exists(defaultPack))
            {
                AssetDatabase.ImportPackage(defaultPack, false);
                AssetDatabase.Refresh();
                Debug.Log("Interactor DefaultPack is successfully installed, version changed back to default.");
                RestartScene();
            }
            else
            {
                Debug.Log("DefaultPack is missing in Integrations folder. Please reimport Integration Packs(Assets/Interactor/Integrations/) or reinstall the Interactor.");
            }
        }

        private void InstallFinalIK()
        {
            if (!_installFik) return;

            if (InteractorTargetSpawner.Instance)
                InteractorTargetSpawner.Instance.Close();

            _installFik = false;
            string finalIKpath = "Assets/Plugins/RootMotion/FinalIK";
            string FinalIKPack = "Assets/Interactor/Integrations/FikPack.unitypackage";
            if (System.IO.Directory.Exists(finalIKpath))
            {
                if (System.IO.File.Exists(FinalIKPack))
                {
                    AssetDatabase.ImportPackage(FinalIKPack, false);
                    AssetDatabase.Refresh();
                    Debug.Log("Final IK integration successfully completed.");
                    RestartScene();
                }
                else
                {
                    Debug.LogWarning("Final IK integration Package is missing. Please reimport Integration Packs(Assets/Interactor/Integrations/) or reinstall the Interactor.");
                }
            }
            else
            {
                Debug.Log("Final IK folder is missing, if you changed the Final IK folder in your project, try installing the integration manually. Located in Assets/Interactor/Integrations folder.");
            }
        }

        private void RestartScene()
        {
            string exampleScene1 = "01_ProtoMan ExampleScene";
            string exampleScene2 = "02_UnityArmature ExampleScene";
            string exampleScene3 = "03_InteractorPaths ExampleScene";
            string exampleScene4 = "04_FirstPerson ExampleScene";
            string exampleScene5 = "05_InteractorAi ExampleScene";

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string path = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;

            if (currentScene == exampleScene1 || currentScene == exampleScene2 || currentScene == exampleScene3 || currentScene == exampleScene4 || currentScene == exampleScene5)
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
                Debug.Log(currentScene + " reopened...");
            }
        }
        #endregion

        #region Sphere & Handles
        private void ShowHandles(SceneView sceneView)
        {
            if (_script == null || _script.effectorLinks.Count == 0) return;
            if (!_script.effectorLinks[_script.selectedTab].enabled) return;
            if (!_script.debug || !UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded(_script) || !_script.gameObject.activeInHierarchy)
            {
#if UNITY_2019_1_OR_NEWER
                SceneView.duringSceneGui -= ShowHandles;
#else
                SceneView.onSceneGUIDelegate -= ShowHandles;
#endif
                handlesOn = false;
                return;
            }

            EditorGUI.BeginChangeCheck();
            _tempColor = HandleUtility.handleMaterial.color;
            _tempColor.a *= opacityValue;
            HandleUtility.handleMaterial.color = _tempColor * _changeHandleColor;
            //Handles.zTest = UnityEngine.Rendering.CompareFunction.Greater;

            if (_script.sphereCol == null)
                _script.sphereCol = _script.GetComponent<SphereCollider>();

            _posOffsetThis = _script.effectorLinks[_script.selectedTab].posOffset;
            _offsetCenterThis = _script.transform.position + _script.sphereCol.center + (_script.transform.right * _posOffsetThis.x) + (_script.transform.forward * _posOffsetThis.z) + (_script.transform.up * _posOffsetThis.y);

            _tempVecB = Handles.PositionHandle(_offsetCenterThis, Quaternion.LookRotation(_script.transform.forward));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Change Interactor Offset");
                _tempVecA = Quaternion.Inverse(_script.transform.rotation) * (_tempVecB - _script.transform.position - _script.sphereCol.center);
                _script.effectorLinks[_script.selectedTab].posOffset.x = Mathf.Round(_tempVecA.x * 1000f) / 1000f;
                _script.effectorLinks[_script.selectedTab].posOffset.y = Mathf.Round(_tempVecA.y * 1000f) / 1000f;
                _script.effectorLinks[_script.selectedTab].posOffset.z = Mathf.Round(_tempVecA.z * 1000f) / 1000f;
            }
            HandleUtility.handleMaterial.color = _tempColor;
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Active | GizmoType.NotInSelectionHierarchy | GizmoType.Pickable | GizmoType.NonSelected | GizmoType.Selected)]
        private static void Gizmos(Interactor script, GizmoType gizmoType)
        {
            if (script.effectorLinks.Count > 0 && script.selectedTab < script.effectorLinks.Count)
            {
                opacityValue = script.opacity;
                Draw3dSphere(script.effectorLinks[script.selectedTab], script.selectedTab, script);
            }
        }

        private static void Draw3dSphere(Interactor.EffectorLink effectorLink, int index, Interactor script)
        {
            if (!script.debug) return;
            if (!script.effectorLinks[index].enabled) return;

            if (script.sphereCol == null)
                script.sphereCol = script.GetComponent<SphereCollider>();

            _colCenter = script.sphereCol.center;
            _colCenter = (_colCenter.x * script.transform.right) + (_colCenter.y * script.transform.up) + (_colCenter.z * script.transform.forward);

            _colDiameter = script.sphereCol.radius;

            _tempVecA = Vector3.ClampMagnitude(effectorLink.posOffset, _colDiameter - effectorLink.maxRadius);

            effectorLink.posOffset.x = Mathf.Round(_tempVecA.x * 1000f) / 1000f;
            effectorLink.posOffset.y = Mathf.Round(_tempVecA.y * 1000f) / 1000f;
            effectorLink.posOffset.z = Mathf.Round(_tempVecA.z * 1000f) / 1000f;

            _posOffsetWithRot = effectorLink.posOffset;

            _colDiameterZ = Mathf.Sqrt((_colDiameter * _colDiameter) - (_posOffsetWithRot.x * _posOffsetWithRot.x));
            _colDiameterY = Mathf.Sqrt((_colDiameter * _colDiameter) - (_posOffsetWithRot.y * _posOffsetWithRot.y));

            _offsetCenter = _colCenter + script.transform.position + effectorLink.posOffset;
            _offsetCenterRot = script.transform.position + _colCenter + (script.transform.right * _posOffsetWithRot.x) + (script.transform.forward * _posOffsetWithRot.z) + (script.transform.up * _posOffsetWithRot.y);

            _colCenterWithPos = (_colCenter + script.transform.position);

            _colCenterWithScaleWithZMoved = _colCenterWithPos + (script.transform.right * _posOffsetWithRot.x);
            _colCenterWithScaleWithYMoved = _colCenterWithPos + (script.transform.up * _posOffsetWithRot.y);

            Handles.Label(_offsetCenterRot, effectorLink.effectorName);

            if (effectorLink.targetActive && Application.isPlaying)
            {
                float dist = Vector3.Distance(effectorLink.targetPosition, _offsetCenterRot);
                Vector3 tangents = (effectorLink.targetPosition + _offsetCenterRot + new Vector3(0, dist * 0.5f, 0)) / 2;

                Handles.DrawBezier(effectorLink.targetPosition, _offsetCenterRot, tangents, tangents, Color.white, null, 10f);
            }

            //Blue horizontal circle for Z plane
            Handles.color = new Color(0, 0, 1, 0.02f * opacityValue);
            Handles.DrawSolidDisc(script.transform.position + _colCenter + (script.transform.up * _posOffsetWithRot.y), script.transform.up, _colDiameterY);
            Handles.color = new Color(0, 0, 1, 0.25f * opacityValue);
            Handles.DrawWireDisc(script.transform.position + _colCenter + (script.transform.up * _posOffsetWithRot.y), script.transform.up, _colDiameterY);

            //Horizontal origin
            Handles.color = new Color(0.3f, 0.3f, 1, 0.7f * opacityValue);
            Handles.DrawWireDisc(_colCenter + script.transform.position, script.transform.up, _colDiameter);
            Handles.DrawDottedLine(script.transform.position + _colCenter + script.transform.forward * -_colDiameter, script.transform.position + _colCenter + script.transform.forward * _colDiameter, 2f);
            Handles.DrawDottedLine(script.transform.position + _colCenter + script.transform.right * -_colDiameter, script.transform.position + _colCenter + script.transform.right * _colDiameter, 4f);

            Handles.color = new Color(0, 0, 1, 0.1f * opacityValue);
            //Blue effector max radius
            Handles.DrawSolidArc(_offsetCenterRot,
                script.transform.up,
                Quaternion.AngleAxis(effectorLink.angleOffset - 90f, script.transform.up) * script.transform.forward,
                effectorLink.angleXZ,
                effectorLink.maxRadius);

            Handles.color = new Color(1, 0, 0, 0.1f * opacityValue);
            //Red effector min radius
            Handles.DrawSolidArc(_offsetCenterRot,
                script.transform.up,
                Quaternion.AngleAxis(effectorLink.angleOffset - 90f, script.transform.up) * script.transform.forward,
                effectorLink.angleXZ,
                effectorLink.minRadius);

            //Green vertical circle for Y plane
            Handles.color = new Color(0, 1, 0, 0.02f * opacityValue);
            Handles.DrawSolidDisc(script.transform.position + _colCenter + (script.transform.right * _posOffsetWithRot.x), script.transform.right, _colDiameterZ);
            Handles.color = new Color(0, 1, 0, 0.25f * opacityValue);
            Handles.DrawWireDisc(script.transform.position + _colCenter + (script.transform.right * _posOffsetWithRot.x), script.transform.right, _colDiameterZ);

            //Vertical origin
            Handles.color = new Color(0.3f, 1, 0.3f, 0.5f * opacityValue);
            Handles.DrawWireDisc(_colCenter + script.transform.position, script.transform.right, _colDiameter);
            Handles.DrawDottedLine(script.transform.position + _colCenter + script.transform.up * -_colDiameter, script.transform.position + _colCenter + script.transform.up * _colDiameter, 4f);
            Handles.DrawDottedLine(script.transform.position + _colCenter + script.transform.forward * -_colDiameter, script.transform.position + _colCenter + script.transform.forward * _colDiameter, 4f);

            Handles.color = new Color(0, 1, 0, 0.1f * opacityValue);
            //Green effector max radius
            Handles.DrawSolidArc(_offsetCenterRot,
                script.transform.right,
                Quaternion.AngleAxis(effectorLink.angleOffsetYZ - 90f, script.transform.right) * script.transform.forward,
                effectorLink.angleYZ,
                effectorLink.maxRadius);

            Handles.color = new Color(1, 0, 0, 0.1f * opacityValue);
            //Red effector min radius
            Handles.DrawSolidArc(_offsetCenterRot,
                script.transform.right,
                Quaternion.AngleAxis(effectorLink.angleOffsetYZ - 90f, script.transform.right) * script.transform.forward,
                effectorLink.angleYZ,
                effectorLink.minRadius);

            //Green effector disk
            Color handleColor = new Color(0.5f, 1, 0.5f, 0.1f * opacityValue);
            DrawTriggerDisk(effectorLink,
                _offsetCenter,
                _offsetCenterRot,
                _colCenterWithPos,
                _colCenterWithScaleWithZMoved,
                _colDiameterZ,
                effectorLink.angleOffsetYZ,
                effectorLink.angleYZ,
                script.transform.right,
                script.transform.forward,
                handleColor,
                true, script);
            //Blue effector disk
            handleColor = new Color(0.5f, 0.5f, 1, 0.1f * opacityValue);
            DrawTriggerDisk(effectorLink,
                _offsetCenter,
                _offsetCenterRot,
                _colCenterWithPos,
                _colCenterWithScaleWithYMoved,
                _colDiameterY,
                effectorLink.angleOffset,
                effectorLink.angleXZ,
                script.transform.up,
                script.transform.forward,
                handleColor,
                false, script);
        }

        private static void DrawTriggerDisk(Interactor.EffectorLink effectorLink,
            Vector3 offsetCenter,
            Vector3 offsetCenter2,
            Vector3 colCenterWithPos,
            Vector3 colCenterWithMovedScale,
            float colDiameterZ,
            float angleOffset,
            float angleAxis,
            Vector3 axis,
            Vector3 axis2,
            Color handleColor,
            bool vertical, Interactor script)
        {
            Handles.color = handleColor;
            if (_arcPoints == null)
            {
                _arcPoints = new List<Vector3>();
                _midPoints = new Vector3[3];
                _midLine = new Vector3[2];
                _sceneText = Resources.Load<GUISkin>("InteractorGUISkin").GetStyle("SceneText");
            }
            _arcPoints.Clear();
            _arcPoints.Add(offsetCenter2);
            _pointDist = Vector3.Distance(offsetCenter2, colCenterWithMovedScale);


            _halfAngle = angleAxis * 0.5f;
            _end = Mathf.CeilToInt(angleAxis / 5);
            if (_end < 3)
                _end = 3;

            for (int i = 0; i < _end; i++)
            {
                _angleSlice = (angleAxis / (_end - 1));
                _arcAngle = _angleSlice * i;
                _topAngle = _arcAngle + angleOffset;
                if (_topAngle >= 360 || _topAngle == 180)
                {
                    _topAngle -= 0.001f;
                }
                else if (_topAngle == 0 || _topAngle == -180)
                {
                    _topAngle += 0.001f;
                }
                _angleRest = 180 - _topAngle;

                if (vertical)
                {   //For vertical disk quarters
                    _pointAngle2 = Vector3.Angle(offsetCenter2 - colCenterWithMovedScale, script.transform.up);
                    //Left Bottom
                    if (offsetCenter.z > (colCenterWithPos.z))
                    {
                        _angleColDiameter = _angleRest + _pointAngle2;
                    }
                    //Right Bottom
                    else
                    {
                        _angleColDiameter = _angleRest - _pointAngle2;
                    }
                }
                else
                {
                    //For horizontal disk quarters
                    _pointAngle = Vector3.Angle(colCenterWithMovedScale - offsetCenter2, script.transform.right);

                    if (offsetCenter.z > (colCenterWithPos.z))
                    {
                        _angleColDiameter = _angleRest + _pointAngle;
                    }
                    else
                    {
                        _angleColDiameter = _angleRest - _pointAngle;
                    }
                }

                _angleDist = Mathf.Asin(_pointDist * Mathf.Sin(Mathf.Deg2Rad * _angleColDiameter) * (1 / colDiameterZ));
                _angleDist = Mathf.Rad2Deg * _angleDist;
                _angleEdge = (180 - _angleDist - _angleColDiameter);
                _edge = colDiameterZ * Mathf.Sin(Mathf.Deg2Rad * _angleEdge) * (1 / Mathf.Sin(Mathf.Deg2Rad * _angleColDiameter));
                _tempPoint = offsetCenter2 + (Quaternion.AngleAxis(_topAngle - 90f, axis) * axis2 * _edge);
                _arcPoints.Add(_tempPoint);

                //For debugging gizmo
                //Handles.Label(tempPoint, i.ToString());


                /*//Draw fading out side lines for middle line
                Vector3[] testLine = new Vector3[2];

                if (i <= _end * 0.5f && i > (_end * 0.5f) - 5)
                {
                    testLine[0] = _arcPoints[0];
                    testLine[1] = tempPoint;
                    float i_f = i * i;
                    float end_f = _end * _end;
                    float alpha = i_f / (end_f);
                    Handles.color = new Color(1, handleColor.g * 0.5f, handleColor.b * 0.5f, alpha * 0.5f * opacityValue);
                    Handles.DrawAAPolyLine(testLine);
                }
                else if(i > _end * 0.5f && i < (_end * 0.5f) + 4)
                {
                    testLine[0] = _arcPoints[0];
                    testLine[1] = tempPoint;
                    float i_f = i * i;
                    float end_f = _end * _end;
                    float alpha = ((_end - i - 1) * (_end - i - 1) / (end_f));
                    Handles.color = new Color(1, handleColor.g * 0.5f, handleColor.b * 0.5f, alpha * 0.5f * opacityValue);
                    Handles.DrawAAPolyLine(testLine);
                }
                Handles.color = handleColor;
                */
            }
            _arcPoints.Add(offsetCenter2);
            Handles.DrawAAConvexPolygon(_arcPoints.ToArray());

            //Draw middle polygon, middle line and its angle
            float _endFloat = _end;
            _midPoints[0] = _arcPoints[0];
            if ((_endFloat * 0.5f) % 1 != 0.5f)
            {
                _midPointA = Mathf.CeilToInt(_endFloat * 0.5f);
                _midPointB = _midPointA + 1;
                _midPoints[1] = _arcPoints[_midPointA];
                _midPoints[2] = _arcPoints[_midPointB];
            }
            else
            {
                _midPointA = Mathf.CeilToInt(_endFloat * 0.5f) - 1;
                _midPointB = _midPointA + 1;
                _midPoints[1] = (_arcPoints[_midPointA] + _arcPoints[_midPointB]) * 0.5f;
                _midPoints[2] = (_arcPoints[_midPointB] + _arcPoints[_midPointB + 1]) * 0.5f;
            }
            _midLine[0] = _midPoints[0];
            _midLine[1] = (_midPoints[1] + _midPoints[2]) * 0.5f;

            handleColor *= 0.5f;
            handleColor.a = 0.2f * opacityValue;
            Handles.color = handleColor;
            Handles.DrawAAConvexPolygon(_midPoints);
            Handles.color = new Color(1f, 1f, 1f, 0.2f * opacityValue);
            Handles.DrawAAPolyLine(_midLine);

            _halfLabel = (_halfAngle).ToString();
            Handles.Label(_midLine[1], _halfLabel, _sceneText);
            _fullLabel = (_halfAngle * 2).ToString();
            Handles.Label(_arcPoints[1], "0", _sceneText);
            Handles.Label(_arcPoints[_arcPoints.Count - 2], _fullLabel, _sceneText);
        }
        #endregion
    }
}
