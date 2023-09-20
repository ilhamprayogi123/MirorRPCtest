using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace razz
{
    public class InteractorTargetSpawner : EditorWindow
    {
        public static InteractorTargetSpawner Instance { get; private set; }
        public static MenuGUI menu;
        public static ToolbarGUI toolbargui;
        public static bool isOpen;
        public static bool spawnerSceneviewMenu;
        public static int selected;

        private int selectedTab;
        private string[] effectorTabs;
        private Interactor.EffectorLink effectorlink;
        private string _savePath;
        private bool _initiated;
        private static InteractorObject _selectedInteractorObject;
        private static InteractionTypeSettings _selectedSettingAsset;

        private GUISkin _skin;
        private Color _textColor = new Color(0.823f, 0.921f, 0.949f);
        private Color _textFieldColor = new Color(0.172f, 0.192f, 0.2f);
        private Color _defaultTextColor = new Color(0, 0, 0, 0);
        private Color _defaultMiniButtonTextColor;
        private Color _defaultToolbarButtonTextColor;
        private Color _defaultTextFieldColor;
        private Vector2 _defaultPadding, _defaultOverflow;
        private float _defaultFixedWidth;
        private Color _defaultGuiColor;
        private Rect windowRect;

        [SerializeField] private static Interactor _interactor;
        [SerializeField] private static bool _excludePlayerMask;
        [SerializeField] public static int surfaceRotation;
        [SerializeField] public static bool addComponentsOnParent;
        [SerializeField] public static bool addPivotOnPoint;
        [SerializeField] public static long rightClickTimer = 100;

        [SerializeField] private int[] _listPointer;
        [SerializeField] private SaveData _saveData;
        [SerializeField] private List<TabStruct> _tabPrefabList;
        [SerializeField] private static List<GameObject> _spawnSettings;

        [HideInInspector] public static List<GameObject> prefabList = new List<GameObject>();

        [SerializeField]
        public struct TabStruct
        {
            [SerializeField]
            public List<GameObject> tabPrefabs;

            public TabStruct(List<GameObject> tabPrefab)
            {
                tabPrefabs = tabPrefab;
            }
        }

        [MenuItem("Window/Interactor/Interactor Target Spawner")]
        static void PrefabSpawnerPanel()
        {
            if (!isOpen && !SceneView.lastActiveSceneView.maximized)
            {
                Instance = GetWindow<InteractorTargetSpawner>();
            }
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += UndoRedoRefresh;
            if (EditorApplication.isUpdating && _saveData)
            {
                SkipInitAndSaveCheck();
                if (_tabPrefabList == null) Load();
                return;
            }

            _defaultGuiColor = GUI.color;
            Instance = this;
            Init();
            if (!_initiated)
            {
                Close();
                return;
            }

            _skin = Resources.Load<GUISkin>("InteractorGUISkin");

            if (toolbargui == null)
            {
                menu = new MenuGUI();
                toolbargui = new ToolbarGUI();
                toolbargui.Setup(menu, _interactor);
            }
            else
            {
                toolbargui.Setup(menu, _interactor);
            }
            isOpen = true;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoRefresh;
            if (Selection.activeObject != null)
            {
                if (AssetDatabase.Contains(Selection.activeObject)) return;
            }

            if (!Application.isPlaying && _initiated && Application.isEditor && !EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isUpdating)
            {
                Save();
                if (SceneView.lastActiveSceneView)
                {
                    if (!SceneView.lastActiveSceneView.maximized)
                    {
                        toolbargui.Disable(menu);
                        isOpen = false;
                    }
                }
            }
        }

        private void Init()
        {
            Instance.minSize = new Vector2(400f, 680f);
            Instance.maxSize = new Vector2(600f, 1200f);
            _interactor = Interactor.Instance;
            if (_interactor == null)
            {
                Debug.LogWarning("Can't find an Interactor script on any scene object.");
                _initiated = false;
                return;
            }
            if (_tabPrefabList != null)
                _tabPrefabList.Clear();
            else _tabPrefabList = new List<TabStruct>();

            _initiated = SavePathCheck();
        }

        public void NewInit()
        {
            if (!Application.isEditor && EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isUpdating)
                return;

            Instance.minSize = new Vector2(400f, 680f);
            Instance.maxSize = new Vector2(600f, 1200f);
            _interactor = Interactor.Instance;
            if (_interactor == null)
            {
                Debug.LogWarning("Can't find an Interactor script on any scene object.");
                _initiated = false;
                return;
            }
            //Save previous saveData before changing to new
            Save();

            _tabPrefabList = new List<TabStruct>();
            _initiated = SavePathCheck();
            if (!_initiated) return;

            if (toolbargui == null)
            {
                menu = new MenuGUI();
                toolbargui = new ToolbarGUI();
                toolbargui.Setup(menu, _interactor);
            }
            else
            {
                toolbargui.Setup(menu, _interactor);
            }
            isOpen = true;
        }

        private void SkipInitAndSaveCheck()
        {
            Instance = this;
            if (toolbargui == null)
            {
                menu = new MenuGUI();
                toolbargui = new ToolbarGUI();
                toolbargui.Setup(menu, _interactor);
            }
            else
            {
                toolbargui.Setup(menu, _interactor);
            }
            isOpen = true;
        }

        public bool HasSaveData()
        {
            if (_saveData) return true;
            else return false;
        }

        private void GetDefaultColors()
        {
            if (_defaultTextColor.a == 0)
            {
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

        private bool SavePathCheck()
        {
            GUI.color = _defaultGuiColor;
            _savePath = _interactor.savePath;
            InteractorUtilities.SaveBundle saveBundle = new InteractorUtilities.SaveBundle();
            saveBundle = InteractorUtilities.LoadAsset(_savePath);
            if (saveBundle.savePath != null && saveBundle.savePath != "" && saveBundle.saveData != null && saveBundle.saveData.listPointer != null)
            {
                if (saveBundle.saveData.listPointer.Length != _interactor.effectorLinks.Count)
                {
                    Debug.LogWarning("Selected save file has " + saveBundle.saveData.listPointer.Length + " effector/s but your current Interactor has " + _interactor.effectorLinks.Count + " effector/s. Save file could not loaded!", _interactor);
                    _interactor.savePath = null;
                    EditorUtility.SetDirty(_interactor);
                    return false;
                }
            }
            _saveData = saveBundle.saveData;
            _savePath = saveBundle.savePath;

            if (_savePath == null || _savePath == "")
            {
                Debug.LogWarning("Can't continue without a save file.");
                return false;
            }

            if (_interactor.savePath != _savePath)
            {
                _interactor.savePath = _savePath;
                EditorUtility.SetDirty(_interactor);
            }

            if (_saveData == null)
                Create(true);
            else Load();

            return true;
        }

        public void Repainter()
        {
            if (Instance != null)
            {
                _interactor = Interactor.Instance;
                if (_interactor == null)
                {
                    _initiated = false;
                    Close();
                    return;
                }

                selectedTab = _interactor.selectedTab;
                Instance.Repaint();
            }
        }

        private void Load()
        {
            _excludePlayerMask = _saveData.excludePlayerMask;
            surfaceRotation = _saveData.surfaceRotation;
            addComponentsOnParent = _saveData.addComponentsOnParent;
            addPivotOnPoint = _saveData.addPivotOnPoint;
            rightClickTimer = _saveData.rightClickTimer;
            _tabPrefabList = new List<TabStruct>();
            if (_saveData.spawnSettings != null)
                _spawnSettings = new List<GameObject>(_saveData.spawnSettings);
            else _spawnSettings = new List<GameObject>();

            int pointer = 0;
            for (int i = 0; i < _saveData.listPointer.Length; i++)
            {
                _tabPrefabList.Add(new TabStruct(new List<GameObject>()));

                for (int a = 0; a < _saveData.listPointer[i]; a++)
                {
                    if (_saveData.tabPrefabs[pointer + a] == null)
                    {
                        _tabPrefabList[i].tabPrefabs.Add(null);
                    }
                    else
                    {
                        _tabPrefabList[i].tabPrefabs.Add(_saveData.tabPrefabs[pointer + a]);
                    }
                }
                pointer += _saveData.listPointer[i];
            }
        }

        private void Create(bool newfile)
        {
            _saveData = CreateInstance<SaveData>();
            _saveData.tabPrefabs = new List<GameObject>();
            for (int i = 0; i < _interactor.effectorLinks.Count; i++)
            {
                _saveData.tabPrefabs.Add(null);
            }
            _saveData.listPointer = new int[_interactor.effectorLinks.Count];

            AssetDatabase.CreateAsset(_saveData, _savePath);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            if (newfile)
            {
                Debug.Log("Save file created at " + _savePath);
            }
        }

        private void Delete()
        {
            if (_saveData != null)
            {
                AssetDatabase.DeleteAsset(_savePath);
                AssetDatabase.Refresh();
            }
        }

        public void Save()
        {
            _saveData.excludePlayerMask = _excludePlayerMask;
            _saveData.surfaceRotation = surfaceRotation;
            _saveData.addComponentsOnParent = addComponentsOnParent;
            _saveData.addPivotOnPoint = addPivotOnPoint;
            _saveData.rightClickTimer = rightClickTimer;
            _listPointer = new int[_tabPrefabList.Count];
            _saveData.spawnSettings = _spawnSettings;

            for (int i = 0; i < _tabPrefabList.Count; i++)
            {
                _listPointer[i] = _tabPrefabList[i].tabPrefabs.Count;
            }
            _saveData.listPointer = _listPointer;

            _saveData.tabPrefabs = new List<GameObject>();
            for (int i = 0; i < _tabPrefabList.Count; i++)
            {
                for (int a = 0; a < _tabPrefabList[i].tabPrefabs.Count; a++)
                {
                    if (_tabPrefabList[i].tabPrefabs[a] == null)
                    {
                        _saveData.tabPrefabs.Add(null);
                    }
                    else
                    {
                        _saveData.tabPrefabs.Add(_tabPrefabList[i].tabPrefabs[a]);
                    }
                }
            }

            EditorUtility.SetDirty(_saveData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static List<GameObject> GetPrefabList()
        {//Send prefab list to sceneview menu
            if (Instance == null)
                PrefabSpawnerPanel();
            if (Instance._tabPrefabList == null || Instance._tabPrefabList.Count == 0)
                Instance.Load();

            prefabList.Clear();

            for (int i = 0; i < Instance._tabPrefabList.Count; i++)
            {
                if (Instance._tabPrefabList[i].tabPrefabs.Count > 0)
                {
                    if (Instance._tabPrefabList[i].tabPrefabs[0] != null)
                    {
                        for (int a = 0; a < Instance._tabPrefabList[i].tabPrefabs.Count; a++)
                        {
                            if (!prefabList.Contains(Instance._tabPrefabList[i].tabPrefabs[a]))
                            {
                                prefabList.Add(Instance._tabPrefabList[i].tabPrefabs[a]);
                            }
                        }
                    }
                }
            }

            if (prefabList.Count == 0)
                Debug.LogWarning("No prefabs attached!");

            return prefabList;
        }

        public static void SpawnPrefab(GameObject selectedPrefab, Vector2 mousePos)
        {
            if (selectedPrefab == null)
            {
                Debug.LogWarning("Selected prefab is null, please remove from list or assign a prefab.");
                return;
            }

            Vector3 screenPosition = mousePos;
            screenPosition.x /= SceneView.lastActiveSceneView.position.width;
            screenPosition.y /= SceneView.lastActiveSceneView.position.height;
            //set Z to a sensible non-zero value so the raycast goes in the right direction
            screenPosition.z = 1;
            //invert Y because UIs are top-down and cameras are bottom-up
            screenPosition.y = 1 - screenPosition.y;

            LayerMask playerMask;
            if (_excludePlayerMask)
                playerMask = ~LayerMask.GetMask(_interactor.layerName);
            else playerMask = ~0;

            Ray ray = SceneView.lastActiveSceneView.camera.ViewportPointToRay(screenPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f, playerMask))
            {
                if (PrefabUtility.GetPrefabAssetType(selectedPrefab) == PrefabAssetType.Regular)
                {
                    Undo.IncrementCurrentGroup();
                    int undoID = Undo.GetCurrentGroup();
                    GameObject newTarget;
                    GameObject pivotGO = null;
                    newTarget = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
                    Undo.RegisterCreatedObjectUndo(newTarget, "Spawned Interactor Target");
                    newTarget.name = selectedPrefab.name;
                    Vector3 prefabPosition = newTarget.transform.position;
                    newTarget.transform.position = hit.point + prefabPosition;

                    if (addPivotOnPoint)
                    {
                        pivotGO = new GameObject();
                        Undo.RegisterCreatedObjectUndo(pivotGO, "Spawned Pivot Target");
                        Undo.CollapseUndoOperations(undoID);
                        pivotGO.name = newTarget.name + " Pivot";
                        pivotGO.transform.position = hit.point;
                        pivotGO.transform.rotation = Quaternion.identity;

                        Undo.SetTransformParent(pivotGO.transform, hit.transform, "Pivot Parented");
                        Undo.CollapseUndoOperations(undoID);
                        Undo.SetTransformParent(newTarget.transform, pivotGO.transform, "Spawn Target Parented");
                        Undo.CollapseUndoOperations(undoID);
                    }
                    else
                    {
                        Undo.SetTransformParent(newTarget.transform, hit.transform, "Spawn Target Parented");
                        Undo.CollapseUndoOperations(undoID);
                    }

                    Debug.DrawLine(hit.point, hit.point + hit.normal * 0.02f, Color.red, 1f);
                    if (surfaceRotation == 1)
                    {
                        if (addPivotOnPoint)
                        {
                            pivotGO.transform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
                        }
                        else
                        {
                            Vector3 rotateAxis = Vector3.Cross(hit.normal, Vector3.forward);
                            float rotateAngle = Vector3.Angle(-hit.normal, Vector3.forward);
                            if (rotateAngle == 180) rotateAxis = Vector3.up;
                            newTarget.transform.RotateAround(hit.point, rotateAxis, rotateAngle);
                        }
                    }
                    else if (surfaceRotation == 2)
                    {
                        if (addPivotOnPoint)
                        {
                            pivotGO.transform.rotation = Quaternion.LookRotation(hit.point - SceneView.lastActiveSceneView.camera.transform.position, SceneView.lastActiveSceneView.camera.transform.up);
                        }
                        else
                        {
                            Vector3 camHorizontalVector = Vector3.ProjectOnPlane(SceneView.lastActiveSceneView.camera.transform.position - hit.point, Vector3.up);
                            float yAngle = Vector3.SignedAngle(newTarget.transform.position - hit.point, camHorizontalVector, Vector3.up);
                            newTarget.transform.RotateAround(hit.point, Vector3.up, yAngle);

                            float zAngle = Vector3.SignedAngle(newTarget.transform.position - hit.point, SceneView.lastActiveSceneView.camera.transform.position - hit.point, SceneView.lastActiveSceneView.camera.transform.right);
                            newTarget.transform.RotateAround(hit.point, SceneView.lastActiveSceneView.camera.transform.right, zAngle);
                        }
                    }
                    else if (surfaceRotation == 3)
                    {
                        if (addPivotOnPoint)
                        {
                            Vector3 temp = pivotGO.transform.eulerAngles;
                            Quaternion tempQ = Quaternion.LookRotation(hit.point - SceneView.lastActiveSceneView.camera.transform.position, SceneView.lastActiveSceneView.camera.transform.up);
                            temp.y = tempQ.eulerAngles.y;
                            pivotGO.transform.rotation = Quaternion.Euler(temp);
                        }
                        else
                        {
                            Vector3 camHorizontalVector = Vector3.ProjectOnPlane(SceneView.lastActiveSceneView.camera.transform.position - hit.point, Vector3.up);
                            float yAngle = Vector3.SignedAngle(newTarget.transform.position - hit.point, camHorizontalVector, Vector3.up);
                            newTarget.transform.RotateAround(hit.point, Vector3.up, yAngle);
                        }
                    }

                    InteractorObject _interactorObject = null;
                    if (addComponentsOnParent && CheckPrefabTypeSettings())
                    {
                        if (!(_interactorObject = hit.transform.GetComponent<InteractorObject>()))
                        {
                            Undo.AddComponent(hit.transform.gameObject, typeof(InteractorObject));
                            Undo.CollapseUndoOperations(undoID);
                            _interactorObject = hit.transform.gameObject.GetComponent<InteractorObject>();
                            UnityEditorInternal.ComponentUtility.CopyComponent(_selectedInteractorObject);
                            UnityEditorInternal.ComponentUtility.PasteComponentValues(_interactorObject);

                            Debug.Log("Added InteractorObject preset settings to parent of spawned InteractorTarget prefab: " + hit.transform.gameObject.name + " as " + _spawnSettings[selected].name, hit.transform.gameObject);
                        }

                        if (addPivotOnPoint) _interactorObject.pivot = pivotGO;
                    }
                    Selection.activeObject = newTarget;
                }
                else Debug.LogWarning("Prefab issue while spawning!");
            }
        }

        private static bool CheckPrefabTypeSettings()
        {
            if (_spawnSettings[selected] == null)
            {
                Debug.LogWarning("Selected interaction type has no assigned setting file on Spawner window.");
                return false;
            }

            _selectedInteractorObject = _spawnSettings[selected].GetComponent<InteractorObject>();

            if (!_selectedInteractorObject)
            {
                Debug.LogWarning("Prefab has no InteractorObject component for Add Component settings.");
            }
            else if (_selectedInteractorObject.interaction == 0)
            {
                Debug.LogWarning("Prefab has no selected interaction type for Add Component settings.", _selectedInteractorObject);
            }

            _selectedSettingAsset = ReturnSettingAsset(_selectedInteractorObject);
            if (_selectedSettingAsset == null)
            {
                Debug.LogWarning("Prefab has no selected interaction type setting asset for Add Component settings.", _selectedInteractorObject);
            }
            else
            {
                return true;
            }
            return false;
        }

        public static List<GameObject> GetSpawnSettings()
        {
            if (_spawnSettings == null)
            {
                _spawnSettings = new List<GameObject>();
                _spawnSettings.Add(null);
                return null;
            }

            for (int i = 0; i < _spawnSettings.Count; i++)
            {
                if (_spawnSettings[i] == null)
                {
                    return null;
                }
            }
            return _spawnSettings;
        }

        public static InteractionTypeSettings ReturnSettingAsset(InteractorObject interactorObject)
        {
            int interaction = interactorObject.interaction;

            if (interaction >= 10 && interaction < 20)
            {
                if (interactorObject.defaultSettings != null)
                {
                    return interactorObject.defaultSettings;
                }
            }
            else if (interaction >= 20 && interaction < 30)
            {
                if (interactorObject.manualSettings != null)
                {
                    return interactorObject.manualSettings;
                }
            }
            else if (interaction >= 30 && interaction < 40)
            {
                if (interactorObject.touchSettings != null)
                {
                    return interactorObject.touchSettings;
                }
            }
            else if (interaction >= 40 && interaction < 50)
            {
                if (interactorObject.distanceSettings != null)
                {
                    return interactorObject.distanceSettings;
                }
            }
            else if (interaction >= 50 && interaction < 60)
            {
                if (interactorObject.climbableSettings != null)
                {
                    return interactorObject.climbableSettings;
                }
            }
            else if (interaction >= 60 && interaction < 70)
            {
                if (interactorObject.multipleSettings != null)
                {
                    return interactorObject.multipleSettings;
                }
            }
            else if (interaction >= 70 && interaction < 80)
            {
                if (interactorObject.selfSettings != null)
                {
                    return interactorObject.selfSettings;
                }
            }
            else if (interaction >= 80 && interaction < 90)
            {
                if (interactorObject.pickableSettings != null)
                {
                    return interactorObject.pickableSettings;
                }
            }
            else if (interaction >= 90 && interaction < 100)
            {
                if (interactorObject.pushSettings != null)
                {
                    return interactorObject.pushSettings;
                }
            }
            else if (interaction >= 100 && interaction < 110)
            {
                if (interactorObject.coverSettings != null)
                {
                    return interactorObject.coverSettings;
                }
            }

            Debug.LogWarning("Selected InteractorObject has no assigned interaction type settings asset!");
            return null;
        }

        public void RefreshTabs()
        {
            if (!_initiated)
            {
                Debug.LogWarning("InteractorTargetSpawner could not initiated.");
                if (toolbargui != null)
                {
                    toolbargui.Disable(menu);
                }
                Instance.Close();
                return;
            }

            if (_tabPrefabList == null)
                _tabPrefabList = new List<TabStruct>();

            int countDif = _interactor.effectorLinks.Count - _tabPrefabList.Count;

            if (countDif > 0)
            {
                if (countDif > 1)
                {
                    for (int i = 0; i < countDif; i++)
                    {
                        _tabPrefabList.Add(new TabStruct(new List<GameObject>()));
                    }
                }
                else
                {
                    if (_tabPrefabList.Count == 0)
                    {
                        _tabPrefabList.Add(new TabStruct(new List<GameObject>()));
                    }
                    else
                    {
                        _tabPrefabList.Insert(_interactor.selectedTab + 1, new TabStruct(new List<GameObject>()));
                        _tabPrefabList[_interactor.selectedTab + 1].tabPrefabs.Add(null);
                    }
                }

                Delete();
                Create(false);
                Save();
                Repaint();
            }
            else if (countDif < 0)
            {
                if (countDif < -1)
                {
                    _tabPrefabList.RemoveAt(_interactor.selectedTab);

                    for (int i = 1; i < -countDif; i++)
                    {
                        _tabPrefabList.RemoveAt(_tabPrefabList.Count - 1);
                    }
                }
                else
                {
                    _tabPrefabList.RemoveAt(_interactor.selectedTab);
                }

                Delete();
                Create(false);
                Save();
                Repaint();
            }
        }

        private void UndoRedoRefresh()
        {
            bool addPivot = addPivotOnPoint;
            bool addComp = addComponentsOnParent;
            int selectedSetting = selected;
            int surfRot = surfaceRotation;
            Load();
            addPivotOnPoint = addPivot;
            addComponentsOnParent = addComp;
            selected = selectedSetting;
            surfaceRotation = surfRot;
            Repaint();
        }

        private void OnGUI()
        {
            if (!_initiated)
            {
                Debug.LogWarning("InteractorTargetSpawner could not initiated.");
                if (toolbargui != null)
                {
                    toolbargui.Disable(menu);
                }
                Instance.Close();
                return;
            }

            if (_interactor == null)
            {
                Init();
                if (!_initiated) return;
            }
            /*if (Selection.activeTransform != _interactor.transform && menu.enabled)
            {
                menu.Disable();
            }*/
            if (_saveData == null && _savePath != null)
            {
                Close();
                EditorGUIUtility.ExitGUI();
                Debug.LogWarning("SaveData is not exist.");
            }
            GetDefaultColors();
            GUI.skin = _skin;
            DrawBackground();
            SetGUIColors();

            GUIStyle tabButtonStyle = new GUIStyle(_skin.GetStyle("Button"));
            _defaultPadding.x = tabButtonStyle.padding.left;
            _defaultPadding.y = tabButtonStyle.padding.right;
            _defaultOverflow.x = tabButtonStyle.overflow.left;
            _defaultOverflow.y = tabButtonStyle.overflow.right;
            _defaultFixedWidth = tabButtonStyle.fixedWidth;

            tabButtonStyle.padding.left = 0;
            tabButtonStyle.padding.right = 0;
            tabButtonStyle.overflow.left = 0;
            tabButtonStyle.overflow.right = 0;
            tabButtonStyle.fixedWidth = EditorGUIUtility.currentViewWidth / (_interactor.effectorLinks.Count);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("SaveData : " + _saveData.name + ".asset", _skin.GetStyle("label"));

            if (EditorGUIUtility.isProSkin)
                GUI.color = Color.red;
            else
                GUI.color = new Color(0.75f, 0, 0);

            if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(19), GUILayout.Height(21)))
            {
                Undo.RecordObject(_interactor, "Save Path Change");
                _interactor.savePath = null;
                EditorUtility.SetDirty(_interactor);

                bool checkpath = SavePathCheck();
                if (!checkpath)
                {
                    _initiated = false;
                    ResetGUIColors();
                    return;
                }
            }
            GUI.color = _defaultGuiColor;
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);

            _interactor = (Interactor)EditorGUILayout.ObjectField("Interactor GameObject :", _interactor, typeof(Interactor), true);

            if (_interactor == null)
            {
                ResetGUIColors();
                return;
            }
            if (_interactor.effectorLinks.Count == 0)
            {
                GUILayout.Space(20f);
                EditorGUILayout.HelpBox("There is no effector on Interactor GameObject. Add effectors first.", MessageType.Error);
                ResetGUIColors();
                return;
            }

            if (_tabPrefabList == null)
            {
                _tabPrefabList = new List<TabStruct>();

                for (int i = 0; i < _interactor.effectorLinks.Count; i++)
                {
                    _tabPrefabList.Add(new TabStruct(new List<GameObject>()));
                    _tabPrefabList[i].tabPrefabs.Add(null);
                }
            }

            GUILayout.BeginVertical();
            effectorTabs = new string[_interactor.effectorLinks.Count];

            for (int i = 0; i < _interactor.effectorLinks.Count; i++)
            {
                if (_interactor.effectorLinks[i].effectorName == "")
                {
                    _interactor.effectorLinks[i].effectorName = i.ToString();
                }
                effectorTabs[i] = _interactor.effectorLinks[i].effectorName;
            }

            GUILayout.Space(10f);

            EditorGUI.BeginChangeCheck();
            selectedTab = GUILayout.Toolbar(_interactor.selectedTab, effectorTabs, tabButtonStyle);
            if (EditorGUI.EndChangeCheck())
            {
                _interactor.selectedTab = selectedTab;
                SceneView.RepaintAll();
                GUI.FocusControl(null);
                InteractorEditor.Repainter();
            }
            effectorlink = _interactor.effectorLinks[selectedTab];

            RefreshTabs();

            GUILayout.Space(5f);
            GUILayout.Label(" Place " + effectorlink.effectorName + " prefabs below.");
            GUILayout.Space(3f);

            GUILayout.BeginHorizontal();
            if (_tabPrefabList[selectedTab].tabPrefabs.Count == 0)
            {
                _tabPrefabList[selectedTab].tabPrefabs.Add(null);
            }

            EditorGUI.BeginChangeCheck();
            _tabPrefabList[selectedTab].tabPrefabs[0] = (GameObject)EditorGUILayout.ObjectField(" Main Prefab", _tabPrefabList[selectedTab].tabPrefabs[0], typeof(GameObject), true);

            if (EditorGUIUtility.isProSkin)
                GUI.color = Color.red;
            else
                GUI.color = new Color(0.75f, 0, 0);

            if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(21), GUILayout.Height(15)))
            {
                _tabPrefabList[selectedTab].tabPrefabs.Clear();
                _tabPrefabList[selectedTab].tabPrefabs.Add(null);
            }
            GUI.color = _defaultGuiColor;
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(_saveData, "Spawner Prefab Change");
                Save();
            }

            GUILayout.Space(5f);
            GUILayout.Label(" You can add alternative prefabs.");
            GUILayout.Space(5f);

            if (_tabPrefabList[selectedTab].tabPrefabs[0] != null)
            {
                for (int i = 1; i < _tabPrefabList[selectedTab].tabPrefabs.Count; i++)
                {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginHorizontal();
                    _tabPrefabList[selectedTab].tabPrefabs[i] = (GameObject)EditorGUILayout.ObjectField("Alternative Prefab " + (i + 1), _tabPrefabList[selectedTab].tabPrefabs[i], typeof(GameObject), true);

                    if (EditorGUIUtility.isProSkin)
                        GUI.color = Color.red;
                    else
                        GUI.color = new Color(0.75f, 0, 0);

                    if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(21), GUILayout.Height(15)))
                    {
                        _tabPrefabList[selectedTab].tabPrefabs.RemoveAt(i);
                    }
                    GUI.color = _defaultGuiColor;
                    GUILayout.EndHorizontal();
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RegisterCompleteObjectUndo(_saveData, "Spawner Prefab Change");
                        Save();
                    }
                }
                GUILayout.Space(5f);
                DropAreaGUI();
                GUILayout.Space(5f);
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Space(5f);
                Rect drop_area = GUILayoutUtility.GetRect(0.0f, 150.0f, GUILayout.ExpandWidth(true));
                GUI.Box(drop_area, "Main Prefabs is not exist.", _skin.GetStyle("DropArea"));
                GUILayout.Space(5f);
                EditorGUI.EndDisabledGroup();
            }

            GUILayout.Space(20f);

            if (_tabPrefabList[selectedTab].tabPrefabs[0] == null)
            {
                EditorGUILayout.LabelField("Prefab Count : 0", _skin.GetStyle("HorizontalLine"));
            }
            else
            {
                EditorGUILayout.LabelField("Prefab Count : " + _tabPrefabList[selectedTab].tabPrefabs.Count, _skin.GetStyle("HorizontalLine"));
            }

            GUILayout.Space(-10f);

            GUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.Label(" SceneView Window for Spawner ");
            GUILayout.FlexibleSpace();
            spawnerSceneviewMenu = (bool)EditorGUILayout.Toggle(spawnerSceneviewMenu, _skin.GetStyle("toggle"), GUILayout.Width(40));
            GUILayout.Space(8f);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                if (addComponentsOnParent && GetSpawnSettings() == null)
                {
                    addComponentsOnParent = false;
                    Debug.LogWarning("Add Component option disabled because one or more spawn presets are null. (Spawner Window -> SceneView Window for Spawner)");
                }
                SceneView.RepaintAll();
            }

            GUILayout.Space(4f);
            EditorGUILayout.LabelField("", _skin.GetStyle("HorizontalLine"));

            EditorGUI.BeginChangeCheck();
            if (spawnerSceneviewMenu)
            {
                if (_spawnSettings == null)
                    _spawnSettings = new List<GameObject>();
                if (_spawnSettings.Count == 0)
                    _spawnSettings.Insert(0, null);

                EditorGUI.BeginChangeCheck();
                GUILayout.Label("   Spawn Presets (Prefabs with InteractorObjects)", _skin.GetStyle("LabelSceneview"));
                GUILayout.Space(4f);
                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                _spawnSettings[0] = (GameObject)EditorGUILayout.ObjectField(" Spawn Settings " + 0, _spawnSettings[0], typeof(GameObject), false);
                if (EditorGUI.EndChangeCheck())
                {
                    if (_spawnSettings[0] != null && PrefabUtility.GetPrefabAssetType(_spawnSettings[0]) == PrefabAssetType.NotAPrefab)
                    {
                        _spawnSettings[0] = null;
                        addComponentsOnParent = false;
                        Debug.LogWarning("Only prefab assets allowed!");
                    }
                    else
                    {
                        InteractorObject intObjCheck = _spawnSettings[0].GetComponent<InteractorObject>();
                        if (!intObjCheck)
                        {
                            _spawnSettings[0] = null;
                            addComponentsOnParent = false;
                            Debug.LogWarning("Prefab has no InteractorObject component for spawn preset.");
                        }
                        else if (intObjCheck.interaction == 0)
                        {
                            _spawnSettings[0] = null;
                            addComponentsOnParent = false;
                            Debug.LogWarning("Prefab has no selected interaction type.");
                        }
                        else if (ReturnSettingAsset(intObjCheck) == null)
                        {
                            _spawnSettings[0] = null;
                            addComponentsOnParent = false;
                        }
                    }
                }

                if (EditorGUIUtility.isProSkin)
                    GUI.color = Color.red;
                else
                    GUI.color = new Color(0.75f, 0, 0);

                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(21), GUILayout.Height(15)))
                {
                    _spawnSettings[0] = null;
                    if (addComponentsOnParent)
                    {
                        addComponentsOnParent = false;
                        SceneView.RepaintAll();
                        Debug.LogWarning("Add Component option disabled because one or more spawn presets are null. (Spawner Window -> SceneView Window for Spawner)");
                    }
                }
                GUI.color = _defaultGuiColor;
                GUILayout.EndHorizontal();

                if (_spawnSettings.Count > 1)
                {
                    for (int i = 1; i < _spawnSettings.Count; i++)
                    {
                        EditorGUI.BeginChangeCheck();
                        GUILayout.BeginHorizontal();
                        _spawnSettings[i] = (GameObject)EditorGUILayout.ObjectField(" Spawn Settings " + i, _spawnSettings[i], typeof(GameObject), false);

                        if (EditorGUIUtility.isProSkin)
                            GUI.color = Color.red;
                        else
                            GUI.color = new Color(0.75f, 0, 0);

                        if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(21), GUILayout.Height(15)))
                        {
                            _spawnSettings.RemoveAt(i);
                        }
                        GUI.color = _defaultGuiColor;
                        GUILayout.EndHorizontal();
                        if (EditorGUI.EndChangeCheck())
                        {
                            bool skipSave = false;
                            if (i < _spawnSettings.Count && _spawnSettings[i] != null && PrefabUtility.GetPrefabAssetType(_spawnSettings[i]) == PrefabAssetType.NotAPrefab)
                            {
                                skipSave = true;
                                _spawnSettings[i] = null;
                                Debug.LogWarning("Only prefab assets allowed!");
                            }
                            else if (i < _spawnSettings.Count && _spawnSettings[i] != null)
                            {
                                InteractorObject intObjCheck = _spawnSettings[i].GetComponent<InteractorObject>();
                                if (!intObjCheck)
                                {
                                    skipSave = true;
                                    _spawnSettings[i] = null;
                                    Debug.LogWarning("Prefab has no InteractorObject component for spawn preset.");
                                }
                                else if (intObjCheck.interaction == 0)
                                {
                                    skipSave = true;
                                    _spawnSettings[i] = null;
                                    Debug.LogWarning("Prefab has no selected interaction type.");
                                }
                                else if (ReturnSettingAsset(intObjCheck) == null)
                                {
                                    skipSave = true;
                                    _spawnSettings[i] = null;
                                }
                            }

                            if (!skipSave)
                            {
                                Undo.RegisterCompleteObjectUndo(_saveData, "Spawner Settings Change");
                                Save();
                            }
                        }
                    }
                }

                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (EditorGUIUtility.isProSkin)
                    GUI.color = Color.red;
                else
                    GUI.color = new Color(0.75f, 0, 0);
                if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(21), GUILayout.Height(15)))
                {
                    _spawnSettings.Add(null);
                    if (addComponentsOnParent)
                    {
                        addComponentsOnParent = false;
                        Debug.LogWarning("Add Component option disabled because one or more spawn presets are null. (Spawner Window -> SceneView Window for Spawner)");
                    }
                }
                GUI.color = _defaultGuiColor;
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(_saveData, "Spawner Settings Add");
                    Save();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginHorizontal();
                //GUILayout.FlexibleSpace();
                GUILayout.Label(" Calculate Rotations on Spawn ");
                //GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                //GUILayout.Space(10f);
                GUILayout.BeginHorizontal();
                GUILayout.Space(10f);
                surfaceRotation = WindowGUI.Slider(surfaceRotation, 0f, 3f, 80f);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(5f);
                if (surfaceRotation == 1)
                {
                    GUILayout.Label("Surface Rotation", _skin.GetStyle("HorizontalLine"));
                }
                else if (surfaceRotation == 2)
                {
                    GUILayout.Label("Camera to Object Direction", _skin.GetStyle("HorizontalLine"));
                }
                else if (surfaceRotation == 3)
                {
                    GUILayout.Label("Camera to Object (Y only)", _skin.GetStyle("HorizontalLine"));
                }
                else
                {
                    GUILayout.Label("Default Prefab Rotation", _skin.GetStyle("HorizontalLine"));
                }
                GUILayout.EndVertical();

                GUILayout.Space(10f);

                GUILayout.BeginHorizontal();
                GUILayout.Label(" Exclude Player Layer On Raycasts ");
                GUILayout.FlexibleSpace();
                _excludePlayerMask = (bool)EditorGUILayout.Toggle(_excludePlayerMask, _skin.GetStyle("toggle"), GUILayout.Width(40));
                GUILayout.Space(8f);
                GUILayout.EndHorizontal();

                GUILayout.Space(2f);

                GUILayout.BeginHorizontal();
                GUILayout.Label(" Add Required Components ");
                GUILayout.FlexibleSpace();
                addComponentsOnParent = (bool)EditorGUILayout.Toggle(addComponentsOnParent, _skin.GetStyle("toggle"), GUILayout.Width(40));
                GUILayout.Space(8f);
                GUILayout.EndHorizontal();

                GUILayout.Space(2f);

                GUILayout.BeginHorizontal();
                GUILayout.Label(" Add Pivot Object On Spawn Point ");
                GUILayout.FlexibleSpace();
                addPivotOnPoint = (bool)EditorGUILayout.Toggle(addPivotOnPoint, _skin.GetStyle("toggle"), GUILayout.Width(40));
                GUILayout.Space(8f);
                GUILayout.EndHorizontal();

                GUILayout.Space(4f);

                GUILayout.BeginHorizontal();
                GUILayout.Label(" Right click time (ms) ");
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                GUILayout.Space(2f);
                rightClickTimer = (int)EditorGUILayout.IntField((int)rightClickTimer, _skin.GetStyle("textfield"), GUILayout.Width(40));
                GUILayout.EndVertical();
                GUILayout.Space(10f);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(_saveData, "Spawner Setting Change");
            }

            if (windowRect.Contains(Event.current.mousePosition))
            {
                Repaint();
            }

            ResetGUIColors();

            tabButtonStyle.padding.left = (int)_defaultPadding.x;
            tabButtonStyle.padding.right = (int)_defaultPadding.y;
            tabButtonStyle.overflow.left = (int)_defaultOverflow.x;
            tabButtonStyle.overflow.right = (int)_defaultOverflow.y;
            tabButtonStyle.fixedWidth = _defaultFixedWidth;
        }

        private void DrawBackground()
        {
            windowRect = new Rect(0, 0, EditorGUIUtility.currentViewWidth, Instance.position.height);
            GUI.Box(windowRect, "", _skin.GetStyle("BackgroundStyle"));
        }

        private void DropAreaGUI()
        {
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 150.0f, GUILayout.ExpandWidth(true));
            GUI.Box(drop_area, "Drag and Drop Prefabs Here", _skin.GetStyle("DropArea"));

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (GameObject dragged_object in DragAndDrop.objectReferences)
                        {
                            if (!_tabPrefabList[selectedTab].tabPrefabs.Contains(dragged_object))
                            {
                                _tabPrefabList[selectedTab].tabPrefabs.Add(dragged_object);
                            }
                        }
                        Undo.RegisterCompleteObjectUndo(_saveData, "Spawner Setting Change");
                        Save();
                    }
                    break;
            }
        }
    }
}
