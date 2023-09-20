using UnityEditor;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

namespace razz
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(InteractorTarget))]
    public class InteractorTargetEditor : Editor
    {
        private InteractorTarget _script { get { return target as InteractorTarget; } }
        #region InteractorTarget Variables
        private Transform _selectedTransform;
        private Transform[] _children;
        private Transform _rootNode;
        #endregion

        private bool _init;
        private Vector3 _transformPosition;

        private Color _boneColor = new Color(0, 0, 0, 0.75f);
        private Color _selectedChildrenColor = new Color(0.44f, 0.75f, 1f);
        private Color _targetColor = new Color(1f, 0.75f, 0.7f);
        private Color _circle = new Color(0.75f, 0.75f, 0.75f, 0.5f);
        private Color _selectedCircle = new Color(1f, 1f, 1f, 1f);
        private float _boneGizmosSize = 0.01f;
        private static Color _uiLineColor = new Color(0.5f, 0.5f, 0.5f);
        
        #region InteractorPathVariables
        private static int _activeInstance;
        private static int _activeEditorInstance;
        private static bool _resetOnPlay;
        private static bool _resetOnEditor;
        private Vector3 _closestPoint;
        private Vector3 _closestPointWorld;
        private float _currentLerp;
        private Transform _svCamTransform;
        private int _currentSegment;
        private Color _pathColor, _selectedPathColor;
        private GUISkin _skin;
        private float _totalPathLenght;
        private Vector3 _arrowDir;

        private InteractorTarget.BackPath _backPath;

        private AnimationCurve _animationCurve;
        private Easer _easer;
        private int _callStartEvent;
        private int _callEndEvent;
        private int _followTransform;

        private bool _speedPreview;
        private bool _inspectorPreview;
        private float _elapsed;
        private bool _halfDone;
        private float _weight;
        private float _sceneviewDeltatime;
        private DateTime _dt1;
        private DateTime _dt2;
        private bool _altOnce;

        private SerializedObject _so;
        private SerializedProperty p_intObj;
        private SerializedProperty p_effectorType;
        private SerializedProperty p_setPosition;
        private SerializedProperty p_setRotation;
        private SerializedProperty p_matchChildBones;
        private SerializedProperty p_matchSource;
        private SerializedProperty p_excludeFromBones;
        private SerializedProperty p_overrideEffector;
        private SerializedProperty p_oHorizontalAngle;
        private SerializedProperty p_oHorizontalOffset;
        private SerializedProperty p_oVerticalAngle;
        private SerializedProperty p_oVerticalOffset;
        private SerializedProperty p_oMaxRange;
        private SerializedProperty p_oMinRange;
        private SerializedProperty p_resetAfterInteraction;
        private SerializedProperty p_pullSpeed;
        private SerializedProperty p_stationaryPoints;
        private SerializedProperty p_dontScale;
        private SerializedProperty p_stationaryStartTangents;
        private SerializedProperty p_stationaryEndTangents;
        private SerializedProperty p_lockTimings;
        private SerializedProperty p_matchParentRotation;
        private SerializedProperty p_pathSegments;
        private SerializedProperty p_endEvents;
        private SerializedProperty p_followTransforms;

        private static bool pathDebug;
        private static bool secondTheme;
        private static readonly string PathDebugReg = "Interactor_TargetPathDebug";
        private static readonly string SecondThemeReg = "Interactor_TargetTheme";
        #endregion

        private void OnEnable()
        {
            SetProperties();
            if (_script.GetInstanceID() == _activeInstance)
            { //Continue to render handles when intTarget or intObj selected on play state change
                if (Application.isPlaying)
                {
                    if (!_resetOnPlay)
                    {
                        _resetOnPlay = true;
                        _activeInstance = -1;
                    }
                }
                else
                {
                    if (!_resetOnEditor)
                    {
                        _resetOnEditor = true;
                        _activeInstance = -1;
                    }
                }
            }

            if (!_init) Initialize();
            if (_rootNode == null) _rootNode = _script.transform;
            _backPath = _script.backPath;
            //In project or in hierarchy
            if (Selection.activeObject != null)
            {
                if (AssetDatabase.Contains(Selection.activeObject))
                {
                    if (!PrefabUtility.IsPartOfPrefabInstance(_script.gameObject) && PrefabUtility.IsPartOfPrefabAsset(_script.gameObject))
                        _script.asset = true;
                    else
                        _script.asset = false;

                    if (_script.asset) return;
                }
                else _script.asset = false;
            }

            GetColors();
            RenderFull();
        }

        private void OnDisable()
        {
            if (!_script) return;
            if (!PrefabUtility.IsPartOfPrefabInstance(_script.gameObject) && PrefabUtility.IsPartOfPrefabAsset(_script.gameObject)) return;
            if (_script.speedDebug == 0 || !_script.IntObj) return;

            if (_children.Length != 0 && Selection.activeObject != null && Selection.activeTransform != null)
            {
                if (Selection.activeGameObject != _script.gameObject && !_children.Contains(Selection.activeGameObject.transform) && Selection.activeGameObject != _script.IntObj.gameObject)
                {
                    PathOff();
                    GetColors();
                }
            }
        }

        private void SetProperties()
        {
            _so = this.serializedObject;
            p_intObj = _so.FindProperty("intObj");
            p_effectorType = _so.FindProperty("effectorType");
            p_setPosition = _so.FindProperty("setPosition");
            p_setRotation = _so.FindProperty("setRotation");
            p_matchChildBones = _so.FindProperty("matchChildBones");
            p_matchSource = _so.FindProperty("matchSource");
            p_excludeFromBones = _so.FindProperty("excludeFromBones");
            p_overrideEffector = _so.FindProperty("overrideEffector");
            p_oHorizontalAngle = _so.FindProperty("oHorizontalAngle");
            p_oHorizontalOffset = _so.FindProperty("oHorizontalOffset");
            p_oVerticalAngle = _so.FindProperty("oVerticalAngle");
            p_oVerticalOffset = _so.FindProperty("oVerticalOffset");
            p_oMaxRange = _so.FindProperty("oMaxRange");
            p_oMinRange = _so.FindProperty("oMinRange");

            p_resetAfterInteraction = _so.FindProperty("resetAfterInteraction");
            p_pullSpeed = _so.FindProperty("pullSpeed");
            p_stationaryPoints = _so.FindProperty("stationaryPoints");
            p_dontScale = _so.FindProperty("dontScale");
            p_stationaryStartTangents = _so.FindProperty("stationaryStartTangents");
            p_stationaryEndTangents = _so.FindProperty("stationaryEndTangents");
            p_lockTimings = _so.FindProperty("lockTimings");
            p_matchParentRotation = _so.FindProperty("matchParentRotation");
            p_pathSegments = _so.FindProperty("pathSegments");
            p_endEvents = _so.FindProperty("endEvents");
            p_followTransforms = _so.FindProperty("followTransforms");

            pathDebug = EditorPrefs.GetBool(PathDebugReg, true);
            EditorPrefs.SetBool(PathDebugReg, pathDebug);
            secondTheme = EditorPrefs.GetBool(SecondThemeReg, false);
            EditorPrefs.SetBool(SecondThemeReg, secondTheme);
        }

        private void Initialize()
        {
            _selectedTransform = Selection.activeTransform;
            if (!_selectedTransform)
                _selectedTransform = Selection.activeGameObject.transform;
            int selectedTargets = Selection.gameObjects.Length;
            if (selectedTargets > 1)
            {
                for (int i = 0; i < selectedTargets; i++)
                {
                    if (Selection.gameObjects[i] == _script.gameObject)
                    {
                        GameObject go = Selection.gameObjects[i];
                        _selectedTransform = go.transform;
                    }
                }
            }
            _children = _selectedTransform.GetComponentsInChildren<Transform>();
            _children = _script.ExcludedBones(_children);

            if (_script.targetSegmentPoints == null || _script.targetSegmentPoints.Length != _script.targetCount * InteractorTarget.SegmentPointsPerSegment)
            { //This will update targets created in v0.89
                Undo.RecordObject(_script, "InteractorTarget updated");
                ValidateSegments();
            }
            _totalPathLenght = _script.GetTargetLength();
            if (_script.backCount > 0)
                _totalPathLenght += _script.GetBackLength();
            _transformPosition = _selectedTransform.position;
            if (!Application.isPlaying) _script.interacting = false;
            _init = true;
        }

        private void GetColors()
        {
            if (!_skin) _skin = _skin = Resources.Load<GUISkin>("InteractorGUISkin");
            
            if (!secondTheme)
            {
                _boneColor = new Color(0, 0, 0, 0.75f);
                _selectedChildrenColor = new Color(0.44f, 0.75f, 1f);
                _targetColor = new Color(1f, 0.75f, 0.7f);
                _circle = new Color(0.75f, 0.75f, 0.75f, 0.5f);
                _selectedCircle = new Color(1f, 1f, 1f, 1f);

                _pathColor = new Color(0.02f, 0.02f, 0.05f, 1f);
                _selectedPathColor = new Color(0.02f, 0.02f, 0.1f, 1f);
            }
            else
            {
                _boneColor = new Color(0.75f, 0.75f, 0.75f, 0.75f);
                _selectedChildrenColor = new Color(1f, 0.25f, 0.14f);
                _targetColor = new Color(1f, 0.25f, 0.7f);
                _circle = new Color(0.2f, 0.2f, 0.2f, 1f);
                _selectedCircle = new Color(0, 0, 0, 1f);

                _pathColor = new Color(0.05f, 0.02f, 0.02f, 1f);
                _selectedPathColor = new Color(0.1f, 0.02f, 0.02f, 1f);
            }
        }

        private bool GetEffectorValues()
        {
            Interactor _interactor = FindObjectOfType<Interactor>();
            if (!_interactor)
            {
                Debug.LogWarning("Could not find an Interactor on this scene.", _script);
                return false;
            }

            for (int i = 0; i < _interactor.effectorLinks.Count; i++)
            {
                if (_interactor.effectorLinks[i].effectorType == _script.effectorType)
                {
                    _script.oHorizontalAngle = _interactor.effectorLinks[i].angleXZ;
                    _script.oHorizontalOffset = _interactor.effectorLinks[i].angleOffset;
                    _script.oVerticalAngle = _interactor.effectorLinks[i].angleYZ;
                    _script.oVerticalOffset = _interactor.effectorLinks[i].angleOffsetYZ;
                    _script.oMaxRange = _interactor.effectorLinks[i].maxRadius;
                    _script.oMinRange = _interactor.effectorLinks[i].minRadius;
                    return true;
                }
            }
            Debug.LogWarning("Could not find this effector type.", _script);
            return false;
        }

        private void RenderFull()
        {
            SceneView.duringSceneGui -= ShowBonesBasic;
            if (_activeInstance != _script.GetInstanceID() || _activeEditorInstance != this.GetInstanceID())
            {
                _activeInstance = _script.GetInstanceID();
                _activeEditorInstance = this.GetInstanceID();
                SceneView.duringSceneGui += ShowBones;
                PathOn();
            }
        }

        private void RenderBasic()
        {
            SceneView.duringSceneGui += ShowBonesBasic;
            SceneView.duringSceneGui -= ShowBones;
            PathOff();
            if (_activeInstance == _script.GetInstanceID())
            {
                _activeInstance = -1;
            }
        }

        private void RenderOff()
        {
            SceneView.duringSceneGui -= ShowBonesBasic;
            SceneView.duringSceneGui -= ShowBones;
            PathOff();
            if (_activeInstance == _script.GetInstanceID() && _activeEditorInstance == this.GetInstanceID())
            {
                _activeInstance = -1;
            }
        }

        private void PathOn()
        {
            SceneView.duringSceneGui += ShowPath;
            Undo.undoRedoPerformed += ValidateSegments;
        }
        private void PathOff()
        {
            SceneView.duringSceneGui -= ShowPath;
            Undo.undoRedoPerformed -= ValidateSegments;
        }

        private void ShowBonesBasic(SceneView sceneView)
        {
            if (_script == null) return;
            if (Selection.activeTransform == _script.transform)
                SceneView.duringSceneGui -= ShowBonesBasic;
            if (_rootNode != null && Selection.activeTransform != null)
            {
                Handles.color = _targetColor;
                if (Handles.Button(_script.transform.position, Quaternion.LookRotation(_script.transform.up, -_script.transform.forward), _boneGizmosSize * 2f, _boneGizmosSize * 2f, Handles.CircleHandleCap))
                {
                    Selection.activeGameObject = _script.gameObject;
                }

                foreach (var bone in _children)
                {
                    if (!bone.transform.parent) continue;
                    if (bone == _children[0]) continue;

                    var start = bone.transform.parent.position;
                    var end = bone.transform.position;

                    Handles.color = _boneColor;
                    Handles.DrawAAPolyLine(3f, start, end);
                }
            }
            else RenderOff();
        }

        private void ShowBones(SceneView sceneView)
        {
            if (_script == null) return;
            if (_children.Length != 0 && Selection.activeObject != null && Selection.activeTransform != null)
            {
                if (Selection.activeGameObject != _script.gameObject && !_children.Contains(Selection.activeGameObject.transform) && (!_script.IntObj || Selection.activeGameObject != _script.IntObj.gameObject))
                    RenderBasic();
            }
            else RenderOff();
            if (_activeEditorInstance != this.GetInstanceID())
            {
                if (_activeInstance == _script.GetInstanceID()) RenderOff();
                return;
            }

            //Check shortcuts for InteractorPath, it is here because ShowPath can be disabled
            Event e = Event.current;
            if (CheckModifierKey(e))
            {
                if (CheckAltKey(e))
                {
                    if (!_altOnce)
                    {
                        _altOnce = true;
                        pathDebug = !pathDebug;
                        EditorPrefs.SetBool(PathDebugReg, pathDebug);
                        SceneView.RepaintAll();
                        Repaint();
                    }
                }
                else _altOnce = false;

                int click = -1;
                if (e.type == EventType.MouseDown && e.button == 0)
                    click = 0;
                if (e.type == EventType.MouseDown && e.button == 1)
                    click = 1;
                if (e.type == EventType.MouseDown && e.button == 2)
                {
                    Undo.RecordObject(_script, "Speed Debug");

                    _script.speedDebug++;
                    if (_script.speedDebug > 3)
                        _script.speedDebug = 0;
                    Repaint();
                }
                if (CheckShiftKey(e)) ToggleSpeedPreview(true);
                else if (!_inspectorPreview) ToggleSpeedPreview(false);

                DrawSegmentSphere(click);
            }
            else
            {
                _altOnce = false;
                if (_speedPreview && !_inspectorPreview) ToggleSpeedPreview(false);
            }

            if (_rootNode != null && Selection.activeTransform != null)
            {
                Handles.color = _targetColor;
                if (Handles.Button(_script.transform.position, Quaternion.LookRotation(_script.transform.up, -_script.transform.forward), _boneGizmosSize * 3f, _boneGizmosSize * 2f, Handles.CircleHandleCap))
                {
                    Selection.activeGameObject = _script.gameObject;
                }

                foreach (var bone in _children)
                {
                    if (!bone.transform.parent) continue;

                    var start = bone.transform.parent.position;
                    var end = bone.transform.position;

                    if (Selection.activeGameObject == bone.gameObject)
                        Handles.color = _selectedCircle;
                    else
                        Handles.color = _circle;

                    Quaternion rot;
                    if (end - start != Vector3.zero)
                        rot = Quaternion.LookRotation(end - start);
                    else
                        rot = Quaternion.LookRotation(bone.transform.up, -bone.transform.forward);

                    if (!_speedPreview)
                    {
                        if (Handles.Button(bone.transform.position, rot, _boneGizmosSize, _boneGizmosSize, Handles.CircleHandleCap))
                            Selection.activeGameObject = bone.gameObject;
                    }

                    if (Selection.activeGameObject == _script.gameObject)
                        Handles.color = _selectedChildrenColor;
                    else if (Selection.activeGameObject == bone.parent.gameObject)
                        Handles.color = _selectedChildrenColor;
                    else
                        Handles.color = _boneColor;

                    if (bone == _children[0]) continue;

                    Matrix4x4 matr = Handles.matrix;

                    Vector3 previewOffset = Vector3.zero;
                    if (_speedPreview)
                    {
                        previewOffset = GetPositionEditor(_weight, !_halfDone);
                        start += previewOffset;
                        end += previewOffset;
                    }
                    
                    Handles.matrix = Matrix4x4.TRS(start + (end - start) / 2, rot, Vector3.one);
                    Handles.DrawWireCube(Vector3.zero, new Vector3(_boneGizmosSize, _boneGizmosSize, (end - start).magnitude));
                    Handles.matrix = matr;

                    if (bone.transform.parent.childCount == 1)
                        Handles.DrawAAPolyLine(15f, start, end);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (_script.targetCount == 0) AddFirstStraights();
            if (!_script.segmentsValidated || _selectedTransform.hasChanged) UpdatePosition();
            this.serializedObject.Update();
            if (_script.speedDebug != 0 && !_script.IntObj && _script.transform.parent != null)
            {
                Debug.LogWarning("Can not find InteractorObject of this InteractorTarget: " + _script.gameObject.name + ". You can assign manually, disabling Speed Debug...", _script);
                _script.speedDebug = 0;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(p_intObj, new GUIContent("Interactor Object", "Most of Interactor Path editor features won't work if InteractorObject is empty. Normally it will try to get it from parents but if you have it on another object other than this targets parent, assign manually."));

            #region InteractorTarget Settings
            InspectorLine();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Target Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(p_effectorType, new GUIContent("Effector Type", "Select the effector type this target belongs."));
            if (p_effectorType.enumValueIndex == 0)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(p_setPosition, new GUIContent("Set Position", "Enable if you want to change your effector bone position."));
                EditorGUILayout.PropertyField(p_setRotation, new GUIContent("Set Rotation", "Enable if you want to change your effector bone rotation."));
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(p_matchChildBones, new GUIContent("Match Child Bones", "Enable if you want to match rotations of each child bones with effector bones. Doesn't work for Body effector type since its children are almost every bone in the rig."));
            EditorGUILayout.LabelField(string.Format("Bones :{0}", _children == null ? 0 : _children.Length));
            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(p_matchSource, new GUIContent("Match Source", "Every target stores its child bones in a transform array if Match Child Bones option is active. It is nothing but if you have same child rotations all around, instead of storing each, they can share same target as source without storing their own so you won't use unnecessary memory for them. It is only for target's child rotations, so target's own rotation doesn't matter. (For example if you have 100 of fist hand gesture, 99 of them can use the other one as Match Source as optimization as long as that source is not destroyed)"));
            EditorGUILayout.PropertyField(p_excludeFromBones, new GUIContent("Exclude From Bones", "Assign the trasnform(s) if you have extra objects under this object as a child other than the original bones. Otherwise they'll be counted as one of this target's bones."), true);
            
            InspectorLine();
            EditorGUILayout.LabelField("Override Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(p_overrideEffector, new GUIContent("Override Effector Rules", "If this target needs to have different rules other than its effector type on Interactor, you can enable and adjust them here for just this interaction as an exception."));

            if (p_overrideEffector.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                if (GUILayout.Button(new GUIContent("Get Interactor Values", "Gets effector rules from Interactor for the same effector type."), GUILayout.Width(140f)))
                {
                    Undo.RegisterCompleteObjectUndo(_script, "Get Interactor Values");

                    if (GetEffectorValues()) EditorUtility.SetDirty(_script);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(p_oHorizontalAngle, new GUIContent("Horizontal Angle"));
                EditorGUILayout.PropertyField(p_oHorizontalOffset, new GUIContent("Horizontal Offset"));
                EditorGUILayout.PropertyField(p_oVerticalAngle, new GUIContent("Vertical Angle"));
                EditorGUILayout.PropertyField(p_oVerticalOffset, new GUIContent("Vertical Offset"));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(p_oMaxRange, new GUIContent("Max Range", "Max range still needs to be lower than radius of the sphere trigger of the Interactor to get detected. Also it needs to be higher than Min Range."));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(p_oMinRange, new GUIContent("Min Range", "Min Range needs to be lower than Max Range."));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            else EditorGUILayout.EndHorizontal();
            #endregion

            #region InteractorPath Settings
            InspectorLine();
            EditorGUILayout.BeginHorizontal();
            string pathMode = "Show";
            if (!pathDebug)
                pathMode = "Hide";
            if (GUILayout.Button(new GUIContent("Path: " + pathMode, "Shortcut: Hold Ctrl (or Command key) + Alt key \n\nGlobal toggle for Interactor Path debug. InteractorPaths show the speed and the paths with placeholder start position. Start position will be moved to effector bone when interacted."), GUILayout.Width(80f)))
            {
                pathDebug = !pathDebug;
                EditorPrefs.SetBool(PathDebugReg, pathDebug);
                SceneView.RepaintAll();
            }

            GUILayout.FlexibleSpace();
            string speedMode = "Off";
            if (_script.speedDebug == 1)
                speedMode = "TargetPath";
            else if (_script.speedDebug == 2)
                speedMode = "All";
            else if (_script.speedDebug == 3)
                speedMode = "All+";
            if (GUILayout.Button(new GUIContent("Speed: " + speedMode, "Shortcut: Hold Ctrl (or Command key) + Middle Mouse on SceneView \n\nShows speed change/time on path depending on InteractorObject speed settings. Caution, speed changes are relative to time, not positions. So faster the speed, more distance it will go. See top corner document for info."), GUILayout.Width(130f)))
            {
                Undo.RecordObject(_script, "Speed Debug");

                _script.speedDebug++;
                if (_script.speedDebug > 3)
                    _script.speedDebug = 0;
                SceneView.RepaintAll();
            }

            string inspectorPreview = "Off";
            if (_inspectorPreview)
                inspectorPreview = "On";
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Preview " + inspectorPreview, "Shortcut: Hold Ctrl (or Command key) + Shift key to animate preview \n\nAnimation speed will be exactly same speed with runtime, except the start position and rotations. Start position and rotations depend on your characters runtime animation and position."), GUILayout.Width(80f)))
            {
                _inspectorPreview = !_inspectorPreview;
                if (_inspectorPreview) ToggleSpeedPreview(true);
                else ToggleSpeedPreview(false);
                SceneView.RepaintAll();
            }

            GUILayout.FlexibleSpace();
            string colorMode = "Blue";
            if (secondTheme)
                colorMode = "Red";
            if (GUILayout.Button(new GUIContent("Color: " + colorMode, "Change bone gizmo color on SceneView. Useful when it is hard to see because of background."), GUILayout.Width(80f)))
            {
                secondTheme = !secondTheme;
                EditorPrefs.SetBool(SecondThemeReg, secondTheme);
                GetColors();
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("InteractorPath Settings", EditorStyles.boldLabel);

            GUILayout.Label("Total Path Length: " + _totalPathLenght.ToString("F3") + "m (" + _script.AllCount + " segments)");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(p_resetAfterInteraction, new GUIContent("Reset After Interaction", "Start position will move to effector bone position and path will scale accordingly. Original path will be restored when this object is destroyed but if you have issues when using same interaction, you can reset to original after each interaction end."));
            EditorGUILayout.PropertyField(p_pullSpeed, new GUIContent("Pull Speed", "When interaction is in second half, which is BackPath, if target and effector moves away of each other, this Pull Speed will increase the speed to end the interaction earlier. When player interacts and moves away from object while interacting, its hand will stay in the middle because back path length would increase and returning bone would have to go more, which makes it stay in the middle depending on the player distance to object. That can be worse when end duration is longer or distance increase is faster. Pull Speed helps to fix that. Value will be multiplied exponentially if distance is increasing."));
            EditorGUILayout.EndHorizontal();
            if (_script.bezierMode)
            {
                if (GUILayout.Button(new GUIContent("Change To Straight Path", "Shortcut: Hold Ctrl (or Command key) + Right Click on target(not back path) path when there is only one segment left. \nChange if you want to use straight path for this interaction. When interacted, effector bone will directly go to target."), GUILayout.Height(30f)))
                {
                    Undo.RegisterCompleteObjectUndo(_script, "Path Mode Change");

                    _script.bezierMode = false;
                    ClearAll();
                    AddFirstTargetSegment();
                    _script.backPath = InteractorTarget.BackPath.Same;
                    _backPath = InteractorTarget.BackPath.Same;
                    AddReversedSegments();
                    _selectedTransform.hasChanged = true;
                    UpdatePosition();
                    EditorUtility.SetDirty(_script);
                    SceneView.RepaintAll();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(p_stationaryPoints, new GUIContent("Stationary Points", "When interacted, start position will move to effector bone position and whole path scale and rotate accordingly. Enable this if you don't want to move other points and tangents. Then only start position will change."));
                EditorGUILayout.PropertyField(p_dontScale, new GUIContent("Don't Scale", "Similar to Stationary Points but it will rotate the path without scaling it."));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(p_stationaryStartTangents, new GUIContent("Stationary Start Tangents", "When start position changes at the beginning of interaction, this allows to move its tangents with it (Start tangent of the first segment and end tangent of the last segment)."));
                EditorGUILayout.PropertyField(p_stationaryEndTangents, new GUIContent("Stationary End Tangents", "When start position and the whole path changes at the beginning of interaction, this allows to lock the target tangents with target point (End tangent of the last target segment(forward) and start tangent of the first back segment)."));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(p_lockTimings, new GUIContent("Lock Timings", "This will lock all segment times so when a segments' length changes, its speed will also change to fit same time. It doesn't matter when all segments change uniformly. But when only one of them change (like on Stationary Points option or FollowTargets to change specific point) those segments' time will also change because it will take different time to end that segment, which will distrupt other times and you won't get your exact Custom Curve time/distance ratio (like in the preview). So enable this when you use Stationary Points or FollowTargets to get same timings with your Custom Curve on InteractorObject speed settings."));
                EditorGUILayout.PropertyField(p_matchParentRotation, new GUIContent("Blend Parent Rotations", "If you use Blend Match Source for getting child bone rotations for segment ends below, you can also get target's rotation as well by enabling this. On segment ends that you assigned a Blend Match Source, your parent bone (hand for example) will get blended and will have same rotation as well as your target child rotations."));
                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                _callStartEvent = EditorGUILayout.IntField(new GUIContent("Call Event On Start: ", "Event index you wish to call from bottom Events list when interaction starts."), _script.callStartEvent);
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("(-1 for none)");
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    if (_callStartEvent > -2 && _callStartEvent < _script.endEvents.Count)
                    {
                        Undo.RecordObject(_script, "Path Start Event");

                        _script.callStartEvent = _callStartEvent;
                    }
                    else
                    {
                        _callStartEvent = _script.callStartEvent;
                        Debug.Log("You can only set -1 for none or legit event index. You can add events down below and set their index number to be called on start.", _script);
                    }
                }
                //SceneView.RepaintAll();
            }
            else
            {
                if (GUILayout.Button(new GUIContent("Change To Bezier Path", "Shortcut: Hold Ctrl (or Command key) + Left Click on straight path. \nChange if you want to use curved paths for this interaction. Then you can edit its segments and curves. This allows more flexible interactions and also helps several of them combine into one."), GUILayout.Height(30f)))
                {
                    Undo.RegisterCompleteObjectUndo(_script, "Path Mode Change");

                    _script.bezierMode = true;
                    ClearAll();
                    AddFirstTargetSegment();
                    _script.backPath = InteractorTarget.BackPath.Same;
                    _backPath = InteractorTarget.BackPath.Same;
                    AddReversedSegments();
                    _selectedTransform.hasChanged = true;
                    UpdatePosition();
                    EditorUtility.SetDirty(_script);
                    SceneView.RepaintAll();
                }
            }
            #endregion

            #region TargetPath
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("Target Path (Count: " + _script.targetCount + ")", "The path will be from effector bone to target."), EditorStyles.boldLabel);
            GUILayout.Label("Target Path Length: " + _script.GetTargetLength().ToString("F3") + "m");
            p_pathSegments.isExpanded = EditorGUILayout.Foldout(p_pathSegments.isExpanded, new GUIContent("Target Path Segments"), true);
            EditorGUI.indentLevel += 1;
            if (p_pathSegments.isExpanded)
            {
                for (int segment = 0; segment < _script.targetCount; segment++)
                {
                    if (p_pathSegments.arraySize <= segment) continue;

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(p_pathSegments.GetArrayElementAtIndex(segment), new GUIContent("Segment " + segment.ToString()), false);
                    Color temp = GUI.skin.label.normal.textColor;
                    GUI.skin.label.normal.textColor = Color.blue;
                    GUILayout.Label(" (Target Segment " + segment.ToString() + ") (" + _script.GetSegmentLength(segment).ToString("F3") + "m)");
                    GUI.skin.label.normal.textColor = temp;
                    GUILayout.EndHorizontal();
                    if (p_pathSegments.GetArrayElementAtIndex(segment).isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUI.BeginChangeCheck();
                        Vector3 startPosition = EditorGUILayout.Vector3Field(new GUIContent("Start Position ", "Start point of this segment. Also end point of the previous segment if that exists."), _script.GetStartPosition(segment));
                        GUI.enabled = false;
                        if (_script.bezierMode)
                            GUI.enabled = true;
                        Vector3 endPosition = Vector3.zero;
                        if (segment == _script.targetCount - 1)
                        {
                            GUI.enabled = false;
                            endPosition = EditorGUILayout.Vector3Field(new GUIContent("End Position ", "End point of this segment. You can't edit because it is actually the target's position."), _script.GetEndPosition(segment));
                            if (_script.bezierMode)
                                GUI.enabled = true;
                        }
                        else
                        {
                            endPosition = EditorGUILayout.Vector3Field(new GUIContent("End Position ", "End point of this segment. Also start point of the next segment if that exists."), _script.GetEndPosition(segment));
                        }
                        Vector3 startTangent = EditorGUILayout.Vector3Field(new GUIContent("Start Tangent ", "Tangent of the start point of this segment. Also known as Control point to edit curved paths."), _script.GetStartTangent(segment));
                        Vector3 endTangent = EditorGUILayout.Vector3Field(new GUIContent("End Tangent ", "Tangent of the end point of this segment. Also known as Control point to edit curved paths."), _script.GetEndTangent(segment));
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(_script, "Target Path Point Change");

                            _script.SetStartTangent(segment, startTangent);
                            _script.SetEndTangent(segment, endTangent);
                            if (segment == 0) //first target segment
                            {
                                _script.SetMidEndPoint(segment, endPosition, false);
                                _script.SetStartPosition(0, startPosition);
                            }
                            else //mid & last target segments
                            {
                                _script.SetMidStartPoint(segment, startPosition, false);
                                _script.SetMidEndPoint(segment, endPosition, false);
                            }
                            //_script.SetEndRotationWorld(segment, endRotation);
                            _script.SetSegmentDirty(segment);
                            if (_backPath == InteractorTarget.BackPath.Same)
                            {
                                ClearBackSegments();
                                AddReversedSegments();
                            }
                            ValidateSegments();
                            SceneView.RepaintAll();
                        }

                        EditorGUI.BeginChangeCheck();
                        Quaternion endRotation = Quaternion.identity;
                        if (segment == _script.targetCount - 1)
                        {
                            GUI.enabled = false;
                            endRotation = Quaternion.Euler(EditorGUILayout.Vector3Field(new GUIContent("End Rotation ", "You can't edit this end rotation because this is not a middle point. This point is the target and will get its rotation."), _script.GetEndRotationWorld(segment).eulerAngles));
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(new GUIContent("Reset End Rotation", "End Rotation offset is in world space unlike the points(Because bone orientations can be different on skeletons so we can't use local rotations). So it changes with the path direction. Normally it won't effect and won't be calculated as offset unless you change it. If you change it and don't want to be calculated anymore or don't want to use any rotation offset anymore, you can reset here."), GUILayout.Width(120f)))
                                endRotation = _script.GetPathDirection(segment);
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            endRotation = Quaternion.Euler(EditorGUILayout.Vector3Field(new GUIContent("End Rotation ", "Rotation offset for this segment end. If this rotation value is different than the current world space rotation (the value you get with Reset End Rotation button), it will be added to interpolated rotation (effector bone to target) as an offset at this end point (the offset will also added as interpolation between start point of this segment and end point of next segment)."), _script.GetEndRotationWorld(segment).eulerAngles));
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(new GUIContent("Reset End Rotation", "End Rotation offset is in world space and changes with the path direction. Normally it won't effect and won't be calculated as offset unless you change it. If you change it and don't want to be calculated anymore, use the reset button."), GUILayout.Width(120f)))
                                endRotation = _script.GetPathDirection(segment);
                            EditorGUILayout.EndHorizontal();
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(_script, "Target Path Rotation Change");

                            _script.SetEndRotationWorld(segment, endRotation);
                            SceneView.RepaintAll();
                        }

                        GUI.enabled = true;
                        EditorGUILayout.Space();
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.BeginHorizontal();
                        _callEndEvent = EditorGUILayout.IntField(new GUIContent("Call Event On End: ", "Event index you wish to call when this segment ends from bottom Events list."), _script.pathSegments[segment].callEndEvent);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField("(-1 for none)");
                        EditorGUILayout.EndHorizontal();
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (_callEndEvent > -2 && _callEndEvent < _script.endEvents.Count)
                            {
                                Undo.RecordObject(_script, "Path Event Selection");

                                _script.pathSegments[segment].callEndEvent = _callEndEvent;
                            }
                            else
                            {
                                _callEndEvent = _script.pathSegments[segment].callEndEvent;
                                Debug.Log("You can only set -1 for none or legit event index. You can add events down below and set their index number to be called on this segment end.", _script);
                            }
                        }
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.BeginHorizontal();
                        string followString = "(-1 for none)";
                        if (segment == _script.targetCount - 1)
                        {
                            followString = "(Only middle points)";
                            GUI.enabled = false;
                        }
                        _followTransform = EditorGUILayout.IntField(new GUIContent("Follow Transform: ", "Transform index you wish this segments' end point to follow from bottom FollowTransforms list."), _script.pathSegments[segment].followTransform);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField(followString);
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (_followTransform > -2 && _followTransform < _script.followTransforms.Count)
                            {
                                Undo.RecordObject(_script, "Path FollowTransform Selection");

                                _script.pathSegments[segment].followTransform = _followTransform;
                            }
                            else
                            {
                                _followTransform = _script.pathSegments[segment].followTransform;
                                Debug.Log("You can only set -1 for none or legit FollowTransform index. You can add transforms down below and set their index number to be followed by this segment end point.", _script);
                            }
                        }

                        if (segment == _script.targetCount - 1)
                        {
                            GUI.enabled = false;
                        }
                        EditorGUILayout.PropertyField(p_pathSegments.GetArrayElementAtIndex(segment).FindPropertyRelative("blendMatchSource"), new GUIContent("Blend Match Source", "If this segment end is a middle point, you can change your match source to get its child rotations (Parent rotation doesn't matter). It is similar to Match Source in terms of optimization and won't cost you anything extra because the target you assign here is already in the scene. On the other hand, this allows you to change your gestures in mid points and provides lots of creative freedom."));
                        EditorGUI.indentLevel--;

                        GUI.enabled = false;
                        if (_script.bezierMode)
                            GUI.enabled = true;
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("Add Target Segment", "Shortcut: Hold Ctrl (or Command key) + Left Click on a path position that you want to add new segment.")))
                        {
                            Undo.RecordObject(_script, "Added Segment");
                            _closestPoint = _script.segmentPoints[(segment * InteractorTarget.SegmentPointsPerSegment) + (int)(InteractorTarget.SegmentPointsPerSegment * 0.5f)] - _transformPosition;
                            _closestPointWorld = _closestPoint + _transformPosition;
                            AddSegment(segment, _closestPoint, CalculateLerp(segment));
                            if (_script.backPath == InteractorTarget.BackPath.Same)
                            {
                                ClearBackSegments();
                                AddReversedSegments();
                            }
                            ValidateSegments();
                            SceneView.RepaintAll();
                        }
                        GUILayout.FlexibleSpace();
                        if (segment < _script.targetCount - 1)
                        {
                            if (GUILayout.Button(new GUIContent("Remove Target Segment", "Shortcut: Hold Ctrl (or Command key) + Right Click on a path you want to remove.")))
                            {
                                Undo.RecordObject(_script, "Removed Segment");
                                RemoveSegment(segment);
                                if (_script.backPath == InteractorTarget.BackPath.Same)
                                {
                                    ClearBackSegments();
                                    AddReversedSegments();
                                }
                                ValidateSegments();
                                SceneView.RepaintAll();
                            }
                        }
                        GUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                        GUI.enabled = true;
                    }
                }
            }
            EditorGUI.indentLevel -= 1;
            #endregion

            #region BackPath
            if (_script.backCount == 0)
            {
                SetBackPath();
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Back Path (Count: " + _script.backCount + ")", "The path will be from target to effector bone, which is actually going back to default position. \n\nModes: \nSame will be same path with TargetPath but reversed. All speed changes and curves will be same. \nStraight will be a straight path instead of curved TargetPath. \nSeperate will be completely different curved path from TargetPath."), EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            if (!_script.bezierMode)
                GUI.enabled = false;
            _backPath = (InteractorTarget.BackPath)EditorGUILayout.EnumPopup(_script.backPath);
            GUI.enabled = true;
            if (EditorGUI.EndChangeCheck())
            { //Target Path is %100 bezier
                Undo.RegisterCompleteObjectUndo(_script, "Back Path Change");

                _script.backPath = _backPath;
                SetBackPath();
                EditorUtility.SetDirty(_script);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("Back Path Length: " + _script.GetBackLength().ToString("F3") + "m");
            p_pathSegments.isExpanded = EditorGUILayout.Foldout(p_pathSegments.isExpanded, new GUIContent("Back Path Segments", "These segments are actually the continuing part of the main segment list (TargetPath)."), true);
            EditorGUI.indentLevel += 1;
            if (p_pathSegments.isExpanded)
            {
                for (int i = 0; i < _script.backCount; i++)
                {
                    int segment = i + _script.targetCount;
                    if (p_pathSegments.arraySize <= segment) continue;

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(p_pathSegments.GetArrayElementAtIndex(segment), new GUIContent("Segment " + segment.ToString()), false);
                    Color temp = GUI.skin.label.normal.textColor;
                    GUI.skin.label.normal.textColor = Color.red;
                    GUILayout.Label(" (Back Segment " + i.ToString() + ") (" + _script.GetSegmentLength(segment).ToString("F3") + "m)");
                    GUI.skin.label.normal.textColor = temp;
                    GUILayout.EndHorizontal();
                    if (p_pathSegments.GetArrayElementAtIndex(segment).isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        GUI.enabled = false;
                        if (_script.backPath == InteractorTarget.BackPath.Seperate)
                            GUI.enabled = true;
                        EditorGUI.BeginChangeCheck();
                        Vector3 startPosition;
                        if (segment == _script.targetCount)
                        {
                            GUI.enabled = false;
                            startPosition = EditorGUILayout.Vector3Field(new GUIContent("Start Position ", "Start point of this segment. You can't edit because it is actually the target's position."), _script.GetStartPosition(segment));
                            if (_script.backPath == InteractorTarget.BackPath.Seperate)
                                GUI.enabled = true;
                        }
                        else
                        {
                            startPosition = EditorGUILayout.Vector3Field(new GUIContent("Start Position ", "Start point of this segment. Also end point of the previous segment if that exists."), _script.GetStartPosition(segment));
                        }
                        Vector3 endPosition = EditorGUILayout.Vector3Field(new GUIContent("End Position ", "End point of this segment. Also start point of the next segment if that exists."), _script.GetEndPosition(segment));
                        Vector3 startTangent = EditorGUILayout.Vector3Field(new GUIContent("Start Tangent ", "Tangent of the start point of this segment. Also known as Control point to edit curved paths."), _script.GetStartTangent(segment));
                        Vector3 endTangent = EditorGUILayout.Vector3Field(new GUIContent("End Tangent ", "Tangent of the end point of this segment. Also known as Control point to edit curved paths."), _script.GetEndTangent(segment));
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(_script, "Back Path Point Change");

                            _script.SetStartTangent(segment, startTangent);
                            _script.SetEndTangent(segment, endTangent);
                            if (segment == _script.AllCount - 1) //last back
                            {
                                _script.SetMidStartPoint(segment, startPosition, false);
                                _script.SetStartPosition(0, endPosition);
                            }
                            else //first & mid back segments
                            {
                                _script.SetMidStartPoint(segment, startPosition, false);
                                _script.SetMidEndPoint(segment, endPosition, false);
                            }
                            _script.SetSegmentDirty(segment);
                            ValidateSegments();
                            SceneView.RepaintAll();
                        }

                        EditorGUI.BeginChangeCheck();
                        Quaternion endRotation = Quaternion.identity;
                        if (segment == _script.AllCount - 1)
                        {
                            GUI.enabled = false;
                            endRotation = Quaternion.Euler(EditorGUILayout.Vector3Field(new GUIContent("End Rotation ", "You can't edit this end rotation because this is not a middle point. This point is the effector bone and will get its rotation."), _script.GetEndRotationWorld(segment).eulerAngles));
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(new GUIContent("Reset End Rotation", "End Rotation offset is in world space and changes with the path direction. Normally it won't effect and won't be calculated as offset unless you change it. If you change it and don't want to be calculated anymore, use the reset button."), GUILayout.Width(120f)))
                                endRotation = _script.GetPathDirection(segment);
                            EditorGUILayout.EndHorizontal();
                            if (_script.backPath == InteractorTarget.BackPath.Seperate)
                                GUI.enabled = true;
                        }
                        else
                        {
                            endRotation = Quaternion.Euler(EditorGUILayout.Vector3Field(new GUIContent("End Rotation ", "Rotation offset for this segment end. If this rotation value is different than the current world space rotation (the value you get with Reset End Rotation button), it will be added to interpolated rotation (target to effector bone) as an offset at this end point (the offset will also added as interpolation between start point of this segment and end point of next segment)."), _script.GetEndRotationWorld(segment).eulerAngles));
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(new GUIContent("Reset End Rotation", "End Rotation offset is in world space and changes with the path direction. Normally it won't effect and won't be calculated as offset unless you change it. If you change it and don't want to be calculated anymore, use the reset button."), GUILayout.Width(120f)))
                                endRotation = _script.GetPathDirection(segment);
                            EditorGUILayout.EndHorizontal();
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(_script, "Back Path Rotation Change");

                            _script.SetEndRotationWorld(segment, endRotation);
                            SceneView.RepaintAll();
                        }

                        GUI.enabled = true;
                        EditorGUILayout.Space();
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.BeginHorizontal();
                        _callEndEvent = EditorGUILayout.IntField(new GUIContent("Call Event On End: ", "Event index you wish to call when this segment ends from bottom Events list."), _script.pathSegments[segment].callEndEvent);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField("(-1 for none)");
                        EditorGUILayout.EndHorizontal();
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (_callEndEvent > -2 && _callEndEvent < _script.endEvents.Count)
                            {
                                Undo.RecordObject(_script, "Back Path Change");

                                _script.pathSegments[segment].callEndEvent = _callEndEvent;
                            }
                            else
                            {
                                _callEndEvent = _script.pathSegments[segment].callEndEvent;
                                Debug.Log("You can only set -1 for none or legit event index. You can add events down below and set their index number to be called on this segment end.", _script);
                            }
                        }
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.BeginHorizontal();
                        string followString = "(-1 for none)";
                        if (segment == _script.AllCount - 1)
                        {
                            followString = "(Only middle points)";
                            GUI.enabled = false;
                        }
                        _followTransform = EditorGUILayout.IntField(new GUIContent("Follow Transform: ", "Transform index you wish this segments' end point to follow from bottom FollowTransforms list."), _script.pathSegments[segment].followTransform);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField(followString);
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (_followTransform > -2 && _followTransform < _script.followTransforms.Count)
                            {
                                Undo.RecordObject(_script, "Path FollowTransform Selection");

                                _script.pathSegments[segment].followTransform = _followTransform;
                            }
                            else
                            {
                                _followTransform = _script.pathSegments[segment].followTransform;
                                Debug.Log("You can only set -1 for none or legit FollowTransform index. You can add transforms down below and set their index number to be followed by this segment end point.", _script);
                            }
                        }

                        if (segment == _script.AllCount - 1)
                        {
                            GUI.enabled = false;
                        }
                        EditorGUILayout.PropertyField(p_pathSegments.GetArrayElementAtIndex(segment).FindPropertyRelative("blendMatchSource"), new GUIContent("Blend Match Source", "If this segment end is a middle point, you can change your match source to get its child rotations (Parent rotation doesn't matter). It is similar to Match Source in terms of optimization and won't cost you anything extra because the target you assign here is already in the scene. On the other hand, this allows you to change your gestures in mid points and provides lots of creative freedom."));
                        EditorGUI.indentLevel--;


                        GUI.enabled = false;
                        if (_script.backPath == InteractorTarget.BackPath.Seperate)
                            GUI.enabled = true;
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("Add Back Segment", "Shortcut: Hold Ctrl (or Command key) + Left Click on a path position that you want to add new segment. Back Path mode needs to be Seperate.")))
                        {
                            Undo.RecordObject(_script, "Added Segment");
                            _closestPoint = _script.segmentPoints[(segment * InteractorTarget.SegmentPointsPerSegment) + (int)(InteractorTarget.SegmentPointsPerSegment * 0.5f)] - _transformPosition;
                            _closestPointWorld = _closestPoint + _transformPosition;
                            AddSegment(segment, _closestPoint, CalculateLerp(segment));

                            ValidateSegments();
                            SceneView.RepaintAll();
                        }
                        GUILayout.FlexibleSpace();
                        if (segment < _script.AllCount - 1)
                        {
                            if (GUILayout.Button(new GUIContent("Remove Back Segment", "Shortcut: Hold Ctrl (or Command key) + Right Click on a path you want to remove.")))
                            {
                                Undo.RecordObject(_script, "Removed Segment");
                                RemoveSegment(segment);
                                ValidateSegments();
                                SceneView.RepaintAll();
                            }
                        }
                        GUI.enabled = true;
                        GUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                    }
                }
            }
            EditorGUI.indentLevel -= 1;
            EditorGUI.indentLevel -= 1;
            #endregion

            #region Events & FollowTransforms
            EditorGUILayout.Space();
            InspectorLine();
            EditorGUILayout.LabelField(new GUIContent("Events", "Add events if you wish to call when any segment ends or at start of interaction. Assign this events' index to the segment you want from upper path list (Call Event On End or Call Event on Start). When the effector bone pass that end point, the event you assigned will be called. \nThis is a pretty efficient system. If you don't use any events, event list will be null and won't use any resource. You'll just use what you need. Also enables to use same events for more than one time for several segments without adding new event."), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            int eventCount = 0;
            if (_script.endEvents != null)
                eventCount = _script.endEvents.Count;
            for (int i = 0; i < eventCount; i++)
            {
                EditorGUILayout.PropertyField(p_endEvents.GetArrayElementAtIndex(i), new GUIContent("End Event " + i.ToString()));
            }
            GUILayout.BeginHorizontal();
            int maxEventCount = _script.AllCount + 1;
            if (eventCount >= maxEventCount)
                GUI.enabled = false;
            if (GUILayout.Button(new GUIContent("Add Event", "Add events if you wish to call when any segment ends. Assign this events' index to the segment you want from upper path list (Call Event On End). When the effector bone pass that end point, the event you assigned will be called.")))
            {
                Undo.RecordObject(_script, "Added Event");

                if (_script.endEvents == null)
                    _script.endEvents = new List<UnityEngine.Events.UnityEvent>();
                _script.endEvents.Add(new UnityEngine.Events.UnityEvent());
            }
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            if (eventCount == 0)
                GUI.enabled = false;
            if (GUILayout.Button("Remove Event"))
            {
                Undo.RecordObject(_script, "Removed Event");

                for (int i = 0; i < _script.AllCount; i++)
                {
                    if (_script.pathSegments[i].callEndEvent == eventCount - 1)
                    {
                        string segment = "Target Segment ";
                        int segmentCount = i;
                        if (i >= _script.targetCount)
                        {
                            segment = "Back Segment ";
                            segmentCount -= _script.targetCount;
                        }

                        _script.pathSegments[i].callEndEvent = -1;
                        Debug.Log(segment + segmentCount + " was using this event. So its Call Event now set as -1 since this event was removed.", _script);
                    }
                }
                if (_script.callStartEvent == eventCount - 1)
                {
                    _script.callStartEvent = -1;
                    _callStartEvent = -1;
                    Debug.Log("Call Event On Start was using this event. So its index now set as -1 since this event was removed.", _script);
                }
                _script.endEvents.RemoveAt(eventCount - 1);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel -= 1;

            InspectorLine();
            EditorGUILayout.LabelField(new GUIContent("FollowTransforms", "Add transform slots to be followed by any middle segment points in mid interaction. Requires middle segments which only enabled in Bezier path mode."), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            int transformCount = 0;
            if (_script.followTransforms != null)
                transformCount = _script.followTransforms.Count;
            for (int i = 0; i < transformCount; i++)
            {
                EditorGUILayout.PropertyField(p_followTransforms.GetArrayElementAtIndex(i), new GUIContent("Follow Transform " + i.ToString()));
            }
            GUILayout.BeginHorizontal();
            int maxTransformCount = _script.AllCount - 2;
            if (transformCount >= maxTransformCount)
                GUI.enabled = false;
            if (GUILayout.Button(new GUIContent("Add FollowTransform", "Add transform slots to be followed by any middle segment points in mid interaction. Requires middle segments which only enabled in Bezier path mode.")))
            {
                Undo.RecordObject(_script, "Added Transform");

                if (_script.followTransforms == null)
                    _script.followTransforms = new List<Transform>();
                _script.followTransforms.Add(null);
            }
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            if (transformCount == 0)
                GUI.enabled = false;
            if (GUILayout.Button("Remove Transform"))
            {
                Undo.RecordObject(_script, "Removed Transform");

                for (int i = 0; i < _script.AllCount; i++)
                {
                    if (_script.pathSegments[i].followTransform == transformCount - 1)
                    {
                        string segment = "Target Segment ";
                        int segmentCount = i;
                        if (i >= _script.targetCount)
                        {
                            segment = "Back Segment ";
                            segmentCount -= _script.targetCount;
                        }

                        _script.pathSegments[i].followTransform = -1;
                        Debug.Log(segment + segmentCount + " was using this transform. So its FollowTransform now set as -1 since this transform was removed.", _script);
                    }
                }
                _script.followTransforms.RemoveAt(transformCount - 1);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            #endregion

            this.serializedObject.ApplyModifiedProperties();
            EditorGUI.indentLevel -= 1;
        }
        public static void InspectorLine(int thickness = 1, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, _uiLineColor);
        }

        #region InteractorPath
        private void ShowPath(SceneView sceneView)
        {
            if (target == null || !_script.gameObject.activeSelf) return;
            if (_script == null) return;
            if (!pathDebug) return;
            if (_script.targetCount == 0) AddFirstStraights();
            if (!_script.segmentsValidated || _selectedTransform.hasChanged) UpdatePosition();
            if (_script.speedDebug != 0 && !_script.IntObj)
            {
                Debug.LogWarning("Can not find InteractorObject of this InteractorTarget: " + _script.gameObject.name + ". You can assign manually, disabling Speed Debug...", _script);
                _script.speedDebug = 0;
            }

            if (_backPath != _script.backPath)
            {
                _backPath = _script.backPath;
                SetBackPath();
            }

            if (_currentSegment > _script.targetCount - 1 && _backPath != InteractorTarget.BackPath.Seperate)
                _currentSegment = _script.targetCount - 1;

            Vector3 startPosition = _script.GetStartPositionWorld(_currentSegment);
            Vector3 endPosition = _script.GetEndPositionWorld(_currentSegment);
            Vector3 startTangent = _script.GetStartTangentWorld(_currentSegment);
            Vector3 endTangent = _script.GetEndTangentWorld(_currentSegment);
            Quaternion endRotation = _script.GetEndRotationWorld(_currentSegment);
            Quaternion segmentDir = _script.GetPathDirection(_currentSegment);

            Handles.color = _pathColor;
            Handles.DrawDottedLine(startPosition, startTangent, 5f);
            Handles.DrawDottedLine(endPosition, endTangent, 5f);
            Handles.DrawDottedLine(startTangent, endTangent, 5f);

            _arrowDir = (_script.GetStartTangent(0) - _script.GetStartPosition(0)).normalized;
            Quaternion arrowRot;
            if (_arrowDir != Vector3.zero)
                arrowRot = Quaternion.LookRotation(_arrowDir);
            else if (_script.GetEndPosition(_script.targetCount - 1) - _script.GetStartPosition(0) != Vector3.zero)
                arrowRot = Quaternion.LookRotation(_script.GetEndPosition(_script.targetCount - 1) - _script.GetStartPosition(0));
            else
                arrowRot = Quaternion.identity;
            Handles.ArrowHandleCap(0, _script.GetStartPositionWorld(0) - (_arrowDir * 0.118f), arrowRot, 0.1f, EventType.Repaint);
            Handles.DrawWireCube(_script.GetEndPositionWorld(_script.targetCount - 1), Vector3.one * 0.02f);

            Quaternion endRotLocal = (_script.GetEndRotationLocal(_currentSegment) * segmentDir).normalized;
            EditorGUI.BeginChangeCheck();
            if (Tools.current == Tool.Rotate)
            {
                if (_currentSegment != _script.targetCount - 1 && _currentSegment != _script.AllCount - 1)
                {
                    endRotLocal = Handles.RotationHandle(endRotLocal, endPosition);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "InteractorPath Rotation Change");

                endRotation = endRotLocal;
                _script.SetEndRotationWorld(_currentSegment, endRotation);
            }

            EditorGUI.BeginChangeCheck();
            if (Tools.current != Tool.Rotate)
            {
                if (Tools.pivotRotation == PivotRotation.Global)
                {
                    if (_currentSegment >= _script.targetCount)
                    {
                        endPosition = Handles.PositionHandle(endPosition, Quaternion.identity);
                        if (_currentSegment != _script.targetCount)
                            startPosition = Handles.PositionHandle(startPosition, Quaternion.identity);
                    }
                    else
                    {
                        startPosition = Handles.PositionHandle(startPosition, Quaternion.identity);
                        if (_currentSegment != _script.targetCount - 1)
                            endPosition = Handles.PositionHandle(endPosition, Quaternion.identity);
                    }
                    if (_script.bezierMode)
                    {
                        startTangent = Handles.PositionHandle(startTangent, Quaternion.identity);
                        endTangent = Handles.PositionHandle(endTangent, Quaternion.identity);
                    }
                }
                else
                {
                    if (_currentSegment >= _script.targetCount)
                    {
                        if (_currentSegment != _script.AllCount - 1)
                            endPosition = Handles.PositionHandle(endPosition, endRotLocal);
                        else
                            endPosition = Handles.PositionHandle(endPosition, segmentDir);
                        if (_currentSegment != _script.targetCount)
                            startPosition = Handles.PositionHandle(startPosition, (_script.GetEndRotationLocal(_currentSegment - 1) * segmentDir).normalized);

                        if (_script.bezierMode)
                        {
                            startTangent = Handles.PositionHandle(startTangent, segmentDir);
                            endTangent = Handles.PositionHandle(endTangent, segmentDir);
                        }
                    }
                    else
                    {
                        if (_currentSegment != 0)
                            startPosition = Handles.PositionHandle(startPosition, (_script.GetEndRotationLocal(_currentSegment - 1) * segmentDir).normalized);
                        else
                            startPosition = Handles.PositionHandle(startPosition, segmentDir);
                        if (_currentSegment != _script.targetCount - 1)
                            endPosition = Handles.PositionHandle(endPosition, endRotLocal);

                        if (_script.bezierMode)
                        {
                            startTangent = Handles.PositionHandle(startTangent, segmentDir);
                            endTangent = Handles.PositionHandle(endTangent, segmentDir);
                        }
                    }
                }
            }
            Handles.SphereHandleCap(0, startTangent, Quaternion.identity, 0.0035f, EventType.Repaint);
            Handles.SphereHandleCap(0, endTangent, Quaternion.identity, 0.0035f, EventType.Repaint);
            //Segment text
            if (!_skin) GetColors();
            Handles.BeginGUI();
            Vector3 pos = (startTangent + endTangent) * 0.5f;
            if (pos.y < startPosition.y)
                pos.y = startPosition.y;
            if (pos.y < endPosition.y)
                pos.y = endPosition.y;
            if (pos.y < startTangent.y)
                pos.y = startTangent.y;
            if (pos.y < endTangent.y)
                pos.y = endTangent.y;
            pos.y += 0.06f;
            Vector2 pos2D = HandleUtility.WorldToGUIPoint(pos);
            Color tempColor = GUI.color;
            GUI.color = _selectedCircle;
            GUI.Label(new Rect(pos2D.x, pos2D.y, 100, 100), "Segment " + _currentSegment, _skin.GetStyle("label"));
            GUI.color = tempColor;
            Handles.EndGUI();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "InteractorPath Point Change");

                if (Tools.pivotRotation != PivotRotation.Global)
                {
                    if ((_script.GetStartPosition(_currentSegment).x > 0 && (startPosition - _transformPosition).x < 0) || (_script.GetStartPosition(_currentSegment).y > 0 && (startPosition - _transformPosition).y < 0) || (_script.GetStartPosition(_currentSegment).z > 0 && (startPosition - _transformPosition).z < 0))
                    {
                        Debug.Log("When you change direction of the InteractorPath in the Sceneview, you can change to Global orientation mode if you have weird results. Local oriented position handles are just for minor adjustments. You can delete this warning.");
                    }
                }

                if (_currentSegment < _script.targetCount) //target segments
                {
                    _script.SetStartTangentWorld(_currentSegment, startTangent);
                    _script.SetEndTangentWorld(_currentSegment, endTangent);
                    if (_currentSegment == 0) //first target segment
                    {
                        _script.SetMidEndPoint(_currentSegment, endPosition, true);
                        _script.SetStartPositionWorld(0, startPosition);
                    }
                    else //mid & last target segments
                    {
                        _script.SetMidStartPoint(_currentSegment, startPosition, true);
                        _script.SetMidEndPoint(_currentSegment, endPosition, true);
                    }
                }
                else //back segments
                {
                    _script.SetStartTangentWorld(_currentSegment, startTangent);
                    _script.SetEndTangentWorld(_currentSegment, endTangent);
                    if (_currentSegment == _script.AllCount - 1) //last back
                    {
                        _script.SetMidStartPoint(_currentSegment, startPosition, true);
                        _script.SetStartPositionWorld(0, endPosition);
                    }
                    else //first & mid back segments
                    {
                        _script.SetMidStartPoint(_currentSegment, startPosition, true);
                        _script.SetMidEndPoint(_currentSegment, endPosition, true);
                    }
                }
                _script.SetSegmentDirty(_currentSegment);

                if (_backPath == InteractorTarget.BackPath.Same)
                {
                    ClearBackSegments();
                    AddReversedSegments();
                }
                ValidateSegments();
            }

            Handles.color = _selectedChildrenColor;
            Handles.DrawDottedLine(_script.GetStartPositionWorld(0), _script.GetEndPositionWorld(_script.targetCount - 1), 2f);

            DrawSegments(segmentDir);
            RenderSpeed();
        }

        private static bool CheckModifierKey(Event currentEvent)
        {
            return currentEvent.command || currentEvent.control;
        }
        private static bool CheckShiftKey(Event currentEvent)
        {
            return currentEvent.shift;
        }
        private static bool CheckAltKey(Event currentEvent)
        {
            return currentEvent.alt;
        }

        private void ToggleSpeedPreview(bool show)
        {
            if (!_script.IntObj)
            {
                if (_speedPreview)
                {
                    EditorApplication.update -= PreviewUpdate;
                    _speedPreview = false;
                }
                return;
            }

            if (show)
            {
                if(!_speedPreview)
                {//Start preview
                    ResetPreview();
                    EditorApplication.update += PreviewUpdate;
                }
                _speedPreview = true;
            }
            else
            {
                if (_speedPreview)
                {//Stop & Reset preview
                    EditorApplication.update -= PreviewUpdate;
                }
                _speedPreview = false;
            }
        }
        private void ResetPreview()
        {
            _elapsed = -0.75f; //Wait time before preview
            _halfDone = false;
            _weight = 0;
            _dt1 = DateTime.Now;
            _dt2 = DateTime.Now;
        }
        private void PreviewUpdate()
        {
            if (_activeEditorInstance != this.GetInstanceID() || !_script || !_script.IntObj)
            {
                ToggleSpeedPreview(false);
                return;
            }

            float targetDuration = _script.IntObj.targetDuration;
            float backDuration = _script.IntObj.backDuration;
            if (_script.IntObj.easeType == EaseType.CustomCurve)
            {
                if (_animationCurve == null)
                    _animationCurve = _script.IntObj.speedCurve;
            }
            _easer = Ease.FromType(_script.IntObj.easeType);

            if (_elapsed < 0)
            {
                _elapsed += _sceneviewDeltatime;
            }
            else if (_elapsed < targetDuration && !_halfDone)
            {
                _elapsed += _sceneviewDeltatime;
                _weight = Mathf.Clamp01(_easer(_elapsed / targetDuration, _animationCurve));
            }
            else if (_elapsed >= targetDuration && !_halfDone)
            {
                _halfDone = true;
                _elapsed = 0;
            }

            if (_elapsed < backDuration && _halfDone)
            {
                _elapsed += _sceneviewDeltatime;
                if (_script.IntObj.easeType == EaseType.CustomCurve)
                    _weight = Mathf.Clamp01(_easer(1f + (_elapsed / backDuration), _animationCurve));
                else
                    _weight = Mathf.Clamp01(1f - _easer(_elapsed / backDuration));
            }
            else if (_elapsed >= backDuration && _halfDone)
                ResetPreview();

            SceneView.RepaintAll();
            CalcSceneviewDeltaTime();
        }
        private void CalcSceneviewDeltaTime()
        {
            _dt2 = DateTime.Now;
            _sceneviewDeltatime = (float)((_dt2.Ticks - _dt1.Ticks) / 10000000.0);
            _dt1 = _dt2;
        }

        private void DrawSegmentSphere(int button)
        {
            if (!_script.segmentsValidated) ValidateSegments();

            if (_backPath != InteractorTarget.BackPath.Seperate)
                _closestPointWorld = HandleUtility.ClosestPointToPolyLine(_script.targetSegmentPoints);
            else
                _closestPointWorld = HandleUtility.ClosestPointToPolyLine(_script.segmentPoints);

            _closestPoint = _closestPointWorld - _transformPosition;
            _currentSegment = ClosestCurve();

            if (!_svCamTransform) _svCamTransform = SceneView.lastActiveSceneView.camera.transform;

            Quaternion handleRot = Quaternion.LookRotation(_svCamTransform.position - _closestPointWorld, _svCamTransform.up);
            float capDistance = Vector3.Distance(_closestPointWorld, Camera.current.transform.position);

            if (button == 0)
            {
                Undo.RegisterCompleteObjectUndo(_script, "Add Segment");

                if (!_script.bezierMode)
                {
                    _script.bezierMode = true;
                    ClearAll();
                    AddFirstTargetSegment();
                    _script.backPath = InteractorTarget.BackPath.Same;
                    _backPath = InteractorTarget.BackPath.Same;
                    AddReversedSegments();
                    _selectedTransform.hasChanged = true;
                    UpdatePosition();
                    EditorUtility.SetDirty(_script);
                    SceneView.RepaintAll();
                    return;
                }

                AddSegment(_currentSegment, _closestPoint, _currentLerp);
                if (_script.backPath == InteractorTarget.BackPath.Same)
                {
                    ClearBackSegments();
                    AddReversedSegments();
                }
                ValidateSegments();
            }
            else if (button == 1 && _script.targetCount > 0)
            {
                if (!_script.bezierMode) return;

                Undo.RegisterCompleteObjectUndo(_script, "Remove Segment");
                
                if (_script.targetCount == 1 && _currentSegment == 0)
                {
                    _script.bezierMode = false;
                    ClearAll();
                    AddFirstTargetSegment();
                    _script.backPath = InteractorTarget.BackPath.Same;
                    _backPath = InteractorTarget.BackPath.Same;
                    AddReversedSegments();
                    _selectedTransform.hasChanged = true;
                    UpdatePosition();
                    EditorUtility.SetDirty(_script);
                    SceneView.RepaintAll();
                    return;
                }

                RemoveSegment(_currentSegment);
                if (_script.backPath == InteractorTarget.BackPath.Same)
                {
                    ClearBackSegments();
                    AddReversedSegments();
                }
                ValidateSegments();
            }

            if (Handles.Button(_closestPointWorld, handleRot, 0.02f * capDistance, 0.5f * capDistance, Handles.CircleHandleCap))
            {
                //Click handled with OnScene Event.current
            }
        }

        private int ClosestCurve()
        {
            float distance = 100f;
            float currentDist;
            int index = 0;
            int checkLimit = _script.targetCount;
            if (_backPath == InteractorTarget.BackPath.Seperate)
                checkLimit = _script.AllCount;

            for (int i = 0; i < checkLimit; i++)
            {
                currentDist = HandleUtility.DistancePointBezier(_closestPointWorld, _script.GetStartPositionWorld(i), _script.GetEndPositionWorld(i), _script.GetStartTangentWorld(i), _script.GetEndTangentWorld(i));
                if (distance > currentDist)
                {
                    distance = currentDist;
                    index = i;
                }
            }
            _currentLerp = CalculateLerp(index);
            return index;
        }

        private float CalculateLerp(int segment)
        {
            float distanceSqrMag = 100f;
            float currentDistance;
            int index = 0;
            int baseIndex = segment * InteractorTarget.SegmentPointsPerSegment;
            for (int i = 1; i < InteractorTarget.SegmentPointsPerSegment; i++)
            {
                currentDistance = (_script.segmentPoints[i + baseIndex] - _closestPointWorld).sqrMagnitude;
                if (distanceSqrMag > currentDistance)
                {
                    distanceSqrMag = currentDistance;
                    index = i;
                }
            }
            float ratio = Mathf.Clamp01((float)index / (float)(InteractorTarget.SegmentPointsPerSegment));
            return ratio;
        }

        private void DrawSegments(Quaternion rot)
        {
            int drawLimit = _script.AllCount;
            if (_backPath == InteractorTarget.BackPath.Same)
                drawLimit = _script.targetCount;

            for (int segment = 0; segment < drawLimit; segment++)
            {
                Vector3[] currentSegmentPoints = new Vector3[InteractorTarget.SegmentPointsPerSegment];
                for (int i = 0; i < InteractorTarget.SegmentPointsPerSegment; i++)
                {
                    currentSegmentPoints[i] = _script.segmentPoints[i + (segment * InteractorTarget.SegmentPointsPerSegment)];
                }

                float width = 8f;
                if (segment == _currentSegment)
                {
                    width = 25f;
                    Handles.color = _selectedPathColor;
                }
                else Handles.color = _pathColor;
                Handles.DrawAAPolyLine(width, currentSegmentPoints);

                Handles.color = _pathColor;
                if (_script.targetCount > 1 && segment < _script.targetCount - 1)
                {
                    Handles.CubeHandleCap(0, currentSegmentPoints[currentSegmentPoints.Length - 1], rot, 0.003f, EventType.Repaint);
                }
                else if (_script.backCount > 1 && segment < _script.AllCount - 1)
                {
                    Handles.CubeHandleCap(0, currentSegmentPoints[currentSegmentPoints.Length - 1], rot, 0.003f, EventType.Repaint);
                }
            }
        }

        private void RenderSpeed()
        {
            if (_script.speedDebug == 0) return;
            if (_script.asset) return;
            if (!_script.IntObj)
            {
                Debug.LogWarning("Can not find InteractorObject of this InteractorTarget: " + _script.gameObject.name + ". You can assign manually, disabling Speed Debug...", _script);
                _script.speedDebug = 0;
                return;
            }

            if (_script.IntObj.easeType == EaseType.CustomCurve)
            {
                _animationCurve = _script.IntObj.speedCurve;
                if (Selection.activeGameObject != null && Selection.activeGameObject == _script.IntObj.gameObject)
                    RescaleAnimationCurve();

                if (_animationCurve.keys.Length > 3 && !_script.interacting)
                { //Render key positions
                    for (int i = 1; i < _animationCurve.keys.Length - 1; i++)
                    {
                        Handles.color = _selectedChildrenColor;
                        if (_animationCurve.keys[i].time < 1f)
                        {
                            Handles.DrawWireCube(GetPositionEditor(_animationCurve.keys[i].value, true) + _transformPosition, Vector3.one * 0.016f);
                        }
                        else if (_animationCurve.keys[i].time > 1f)
                        {
                            if (_backPath != InteractorTarget.BackPath.Same)
                                Handles.DrawWireCube(GetPositionEditor(1f - (1f - _animationCurve.keys[i].value), false) + _transformPosition, Vector3.one * 0.016f);
                        }
                    }
                }
            }

            int renderLimit = InteractorTarget.SegmentPointsPerSegment * _script.AllCount;
            if (_script.speedDebug == 1)
                renderLimit = InteractorTarget.SegmentPointsPerSegment * _script.targetCount;
            int targetLimit = InteractorTarget.SegmentPointsPerSegment * _script.targetCount;

            float[] distances = new float[renderLimit + 1];
            float targetInterval = 1f / (InteractorTarget.SegmentPointsPerSegment * _script.targetCount);
            float backInterval = 1f / (InteractorTarget.SegmentPointsPerSegment * _script.backCount);
            for (int i = 1; i <= renderLimit; i++)
            {
                if (i > targetLimit)
                {
                    int backi = i - targetLimit;
                    float positionValue = GetSpeedValues((float)backi * backInterval, true);
                    float positionValuePrev = GetSpeedValues((float)(backi - 1) * backInterval, true);
                    float real = positionValue - positionValuePrev;

                    distances[i] = real / backInterval;
                    if (float.IsNaN(distances[i]))
                        distances[i] = 0;
                }
                else
                {
                    float positionValue = GetSpeedValues((float)i * targetInterval, false);
                    float positionValuePrev = GetSpeedValues((float)(i - 1) * targetInterval, false);
                    float real = positionValue - positionValuePrev;

                    distances[i] = real / targetInterval;
                    if (float.IsNaN(distances[i]))
                        distances[i] = 0;
                }
            }

            for (int i = 0; i < renderLimit; i++)
            {
                if (i >= targetLimit)
                {
                    float ratio = distances[i + 1];
                    float size = Mathf.InverseLerp(-8f, 8f, ratio * 3f);
                    Color speedColor = new Color(ratio + 0.5f, ratio, ratio - 0.5f);
                    
                    Vector3 offset = Vector3.zero;
                    if (_backPath == InteractorTarget.BackPath.Same)
                    {
                        if (_script.bezierMode)
                            offset = Vector3.Cross(_arrowDir, _script.GetStartPosition(0) - _script.GetEndPosition(_script.targetCount - 1)) * 0.08f;
                        else
                            offset = Vector3.Cross(_arrowDir, Vector3.up) * 0.016f;
                        if (Vector3.Dot(Camera.current.transform.forward, offset) < 0)
                            offset *= -1;
                        speedColor = new Color(ratio + 0.5f, ratio, ratio - 0.5f, 0.5f);
                    }
                    Handles.color = speedColor;

                    Handles.SphereHandleCap(0, _script.segmentPoints[i] + offset, Quaternion.identity, 0.036f * _script.GetSegmentLength(i / InteractorTarget.SegmentPointsPerSegment) * size, EventType.Repaint);
                    if (_script.speedDebug == 3 && _backPath != InteractorTarget.BackPath.Same)
                    {
                        if (!_skin) GetColors();
                        Vector3 fontOffset = new Vector3(0, 0.01f, 0);
                        Handles.Label(_script.segmentPoints[i] - fontOffset, ratio.ToString("F1") + "x", _skin.GetStyle("label"));
                    }
                }
                else
                {
                    float ratio = distances[i + 1];
                    float size = Mathf.InverseLerp(-8f, 8f, ratio * 3f);
                    Color speedColor = new Color(ratio + 0.5f, ratio, ratio - 0.5f);
                    Handles.color = speedColor;

                    Handles.SphereHandleCap(0, _script.segmentPoints[i], Quaternion.identity, 0.036f * _script.GetSegmentLength(i / InteractorTarget.SegmentPointsPerSegment) * size, EventType.Repaint);
                    if (_script.speedDebug == 3)
                    {
                        if (!_skin) GetColors();
                        Handles.Label(_script.segmentPoints[i], "   " + ratio.ToString("F1") + "x", _skin.GetStyle("label"));
                    }
                }
            }
        }
        private float GetSpeedValues(float posRatio, bool back)
        {
            if (_script.IntObj.easeType == EaseType.CustomCurve)
            {
                _easer = Ease.FromType(EaseType.CustomCurve);
                if (back) return -_easer((1f + posRatio), _animationCurve);
                else return _animationCurve.Evaluate(posRatio);
            }
            else
            {
                _easer = Ease.FromType(_script.IntObj.easeType);
                return _easer(posRatio);
            }
        }
        private void RescaleAnimationCurve()
        {//Rescales the animation curve between 0 and 2f with at least 3 keys and sets mid key always to 1f to divide curve into two halfs (target & back).
            Keyframe[] keyframes;
            if (_animationCurve == null) keyframes = new Keyframe[0];
            else keyframes = _animationCurve.keys;

            if (keyframes.Length < 3)
            {
                keyframes = new Keyframe[3];
                keyframes[0].value = 0;
                keyframes[0].time = 0;
                keyframes[1].value = 1f;
                keyframes[1].time = 1f;
                keyframes[2].value = 0;
                keyframes[2].time = 2f;
            }
            else
            {
                if (keyframes[0].value != 0 || keyframes[0].time != 0)
                {
                    keyframes[0].value = 0;
                    keyframes[0].time = 0;
                }
                bool correctMidPointExist = false;
                for (int i = 1; i < keyframes.Length - 1; i++)
                {
                    if (keyframes[i].value > 1f)
                        keyframes[i].value = 1f;
                    if (keyframes[i].value < 0)
                        keyframes[i].value = 0;
                    if (keyframes[i].time > 1.98f)
                        keyframes[i].time = 1.98f;
                    if (keyframes[i].time < 0.02f)
                        keyframes[i].time = 0.02f;
                    if (keyframes[i].time == 1f && keyframes[i].value == 1f)
                        correctMidPointExist = true;
                }
                if (keyframes[_animationCurve.keys.Length - 1].value != 0 || keyframes[_animationCurve.keys.Length - 1].time != 2f)
                {
                    keyframes[_animationCurve.keys.Length - 1].value = 0;
                    keyframes[_animationCurve.keys.Length - 1].time = 2f;
                }
                if (!correctMidPointExist)
                {
                    keyframes[1].time = 1f;
                    keyframes[1].value = 1f;
                }
            }
            _animationCurve.keys = keyframes;
        }
        private float GetPreLength(int segmentPoint)
        {
            int temp = segmentPoint % InteractorTarget.SegmentPointsPerSegment;
            if (segmentPoint == 0)
                return 0;
            else if (temp == 0)
                return GetPreLength(segmentPoint - 1);

            bool wholeLastSegment = false;
            int segmentPointRaw = segmentPoint % InteractorTarget.SegmentPointsPerSegment;
            if (segmentPointRaw == 19)
                wholeLastSegment = true;
            int segment = segmentPoint / InteractorTarget.SegmentPointsPerSegment;

            float length = 0;
            if (_script.backCount > 0 && segment >= _script.targetCount)
            {
                if (wholeLastSegment)
                {
                    segment++;
                    segmentPointRaw = 0;
                }
                for (int i = _script.targetCount; i < segment; i++)
                    length += _script.GetSegmentLength(i);
            }
            else
            {
                if (wholeLastSegment)
                {
                    segment++;
                    segmentPointRaw = 0;
                }
                for (int i = 0; i < segment; i++)
                    length += _script.GetSegmentLength(i);
            }

            for (int i = 1; i < segmentPointRaw + 1; i++)
            {
                if (i % InteractorTarget.SegmentPointsPerSegment == 0) continue;

                length += Vector3.Distance(_script.segmentPoints[segment * InteractorTarget.SegmentPointsPerSegment + i], _script.segmentPoints[segment * InteractorTarget.SegmentPointsPerSegment + i - 1]);
            }
            return length;
        }
        private Vector3 GetPositionEditor(float weight, bool toTarget)
        { //Similar to _script GetPosition but we won't modify its some other variables
            int currentSegment = 0;
            float currentSegmentLerp = 0;
            float lengthCheck = 0;
            float lerpedLength;
            int start, end;

            if (toTarget)
            {
                lerpedLength = weight * _script.GetTargetLength();
                start = 0;
                end = _script.targetCount;
            }
            else
            {
                lerpedLength = (1f - weight) * _script.GetBackLength();
                start = _script.targetCount;
                end = _script.AllCount;
            }
            for (int i = start; i < end; i++)
            {
                lengthCheck += _script.GetSegmentLength(i);

                if (lerpedLength <= lengthCheck)
                {
                    currentSegmentLerp = (lerpedLength - (lengthCheck - _script.GetSegmentLength(i))) / _script.GetSegmentLength(i);
                    currentSegment = i;
                    break;
                }
            }
            if (!_script.bezierMode)
            {
                if (toTarget)
                    return Vector3.Lerp(_script.GetStartPosition(0), _script.GetEndPosition(0), weight);
                else
                    return Vector3.Lerp(_script.GetStartPosition(1), _script.GetEndPosition(1), (1f - weight));
            }
            if (_script.backPath == InteractorTarget.BackPath.Straight && !toTarget)
            {
                return Vector3.Lerp(_script.GetStartPosition(_script.targetCount), _script.GetEndPosition(_script.targetCount), (1f - weight));
            }

            float segmentLerp, segmentLerpPow2, SegmentLerpPow3, subtractedLerpVal, subtractedLerpValPow2, subtractedLerpValPow3;

            segmentLerp = currentSegmentLerp;
            segmentLerpPow2 = segmentLerp * segmentLerp;
            SegmentLerpPow3 = segmentLerpPow2 * segmentLerp;
            subtractedLerpVal = 1 - segmentLerp;
            subtractedLerpValPow2 = subtractedLerpVal * subtractedLerpVal;
            subtractedLerpValPow3 = subtractedLerpValPow2 * subtractedLerpVal;

            return (subtractedLerpValPow3 * _script.GetStartPosition(currentSegment)) +
                           (3 * subtractedLerpValPow2 * segmentLerp * _script.GetStartTangent(currentSegment)) +
                           (3 * subtractedLerpVal * segmentLerpPow2 * _script.GetEndTangent(currentSegment)) +
                           (SegmentLerpPow3 * _script.GetEndPosition(currentSegment));
        }

        private void ValidateSegments()
        {
            if (_script == null) return;
            if (_script.targetCount == 0)
            {
                AddFirstStraights();
                return;
            }

            int totalSegmentPoints = InteractorTarget.SegmentPointsPerSegment * _script.AllCount;
            _script.segmentPoints = new Vector3[totalSegmentPoints];
            List<Vector3> allPoints = new List<Vector3>();

            if (_script.bezierMode)
            {
                for (int i = 0; i < _script.targetCount; i++)
                {
                    Vector3[] currentSegmentPoints = Handles.MakeBezierPoints(_script.GetStartPositionWorld(i), _script.GetEndPositionWorld(i), _script.GetStartTangentWorld(i), _script.GetEndTangentWorld(i), InteractorTarget.SegmentPointsPerSegment);
                    allPoints.AddRange(currentSegmentPoints);
                }
                if (_script.pathSegments[_script.targetCount].bezier)
                {
                    for (int i = _script.targetCount; i < _script.AllCount; i++)
                    {
                        Vector3[] currentSegmentPoints = Handles.MakeBezierPoints(_script.GetStartPositionWorld(i), _script.GetEndPositionWorld(i), _script.GetStartTangentWorld(i), _script.GetEndTangentWorld(i), InteractorTarget.SegmentPointsPerSegment);
                        allPoints.AddRange(currentSegmentPoints);
                    }
                }
                else
                {
                    int targetLimit = InteractorTarget.SegmentPointsPerSegment;
                    float lerp;
                    for (int i = targetLimit; i < totalSegmentPoints; i++)
                    {
                        lerp = 1f - ((float)(i - targetLimit) / (float)(targetLimit - 1f));
                        allPoints.Add(Vector3.Lerp(_script.GetStartPositionWorld(0), _script.GetEndPositionWorld(_script.targetCount - 1), lerp));
                    }
                }
                _script.segmentPoints = allPoints.ToArray();
            }
            else
            {
                int targetLimit = InteractorTarget.SegmentPointsPerSegment * _script.targetCount;
                float lerp;
                for (int i = 0; i < totalSegmentPoints; i++)
                {
                    if (i < targetLimit)
                        lerp = (float)i / (float)(targetLimit - 1f);
                    else
                        lerp = 1f - ((float)(i - targetLimit) / (float)(targetLimit - 1f));

                    _script.segmentPoints[i] = Vector3.Lerp(_script.GetStartPositionWorld(0), _script.GetEndPositionWorld(0), lerp);
                }
            }

            _totalPathLenght = _script.GetTargetLength();
            if (_script.backCount > 0)
                _totalPathLenght += _script.GetBackLength();

            if (_backPath != InteractorTarget.BackPath.Seperate)
            {
                UpdateTargetSegmentPointArray();
            }
            _script.segmentsValidated = true;
        }
        private void UpdateTargetSegmentPointArray()
        {//This array editor only and only holds target segmentPoints (first half). Needed for ctrl segment point selection in sceneview when BackPath is not seperate.
            _script.targetSegmentPoints = new Vector3[_script.targetCount * InteractorTarget.SegmentPointsPerSegment];
            System.Array.Copy(_script.segmentPoints, _script.targetSegmentPoints, _script.targetSegmentPoints.Length);
        }

        private void UpdatePosition()
        {
            Undo.RecordObject(_script, "InteractorTarget Segment points updated");

            _transformPosition = _selectedTransform.position;
            ValidateSegments();
            _selectedTransform.hasChanged = false;
        }

        public void AddSegment(int segment, Vector3 position, float lerp)
        {
            Vector3 endPosition;
            Vector3 startTangent;
            Vector3 endTangent;

            if (segment < _script.AllCount)
            {
                if (segment >= _script.targetCount) _script.backCount++;

                endPosition = _script.GetEndPosition(segment);
                startTangent = CalculateTangent(segment, lerp, 2);
                endTangent = CalculateTangent(segment, lerp, 3);

                Vector3 newStartTangent = CalculateTangent(segment, lerp, 0);
                Vector3 newEndTangent = CalculateTangent(segment, lerp, 1);
                _script.SetEndPosition(segment, position);
                _script.SetStartTangent(segment, newStartTangent);
                _script.SetEndTangent(segment, newEndTangent);

                int callEndEvent = _script.pathSegments[segment].callEndEvent;
                int followTransform = _script.pathSegments[segment].followTransform;
                InteractorTarget blendMatchSource = _script.pathSegments[segment].blendMatchSource;
                _script.pathSegments[segment].callEndEvent = -1;
                _script.pathSegments[segment].followTransform = -1;
                _script.pathSegments[segment].blendMatchSource = null;

                _script.pathSegments.Insert(segment + 1, new InteractorTarget.PathSegment(position, endPosition, startTangent, endTangent, _script.GetEndRotationWorld(segment), 0));
                _script.SetEndRotationWorld(segment, _script.GetPathDirection(segment));

                _script.pathSegments[segment + 1].callEndEvent = callEndEvent;
                _script.pathSegments[segment + 1].followTransform = followTransform;
                _script.pathSegments[segment + 1].blendMatchSource = blendMatchSource;

                _script.SetSegmentDirty(segment);
            }
        }
        private void AddFirstTargetSegment()
        {
            Vector3 endPosition = Vector3.zero;
            Vector3 startTangent = new Vector3(0.2f, 0.1f, 0);
            Vector3 endTangent = new Vector3(0, 0.1f, 0);
            Vector3 startPosition;
            if (_children != null && _children.Length > 1)
            {
                startPosition = _selectedTransform.position - _children[_children.Length - 1].position;
                startPosition *= 2f;
                startPosition.y = 0;
                startTangent = new Vector3(startPosition.x, startPosition.y + 0.1f, startPosition.z);
            }
            else
                startPosition = new Vector3(0.2f, 0, 0);
            if (!_script.bezierMode)
            {
                _script.pathSegments.Insert(0, new InteractorTarget.PathSegment(startPosition, endPosition, 0));
            }
            else
            {
                _script.storedFirstPosition = startPosition;
                _script.pathSegments.Insert(0, new InteractorTarget.PathSegment(startPosition, endPosition, startTangent, endTangent, Quaternion.identity, 0));
            }
        }
        private void AddFirstStraights()
        {
            _script.bezierMode = false;
            AddFirstTargetSegment();
            _script.backPath = InteractorTarget.BackPath.Same;
            _backPath = InteractorTarget.BackPath.Same;
            AddReversedSegments();
            _selectedTransform.hasChanged = true;
            UpdatePosition();
            SceneView.RepaintAll();
        }
        public void AddFirstBackSegment()
        {
            Vector3 startTangent = _script.GetEndPosition(_script.targetCount - 1) + ((_script.GetEndTangent(_script.targetCount - 1) - _script.GetEndPosition(_script.targetCount - 1)) * 0.5f);

            Vector3 endTangent = _script.GetStartPosition(0) + ((_script.GetStartTangent(0) - _script.GetStartPosition(0)) * 0.5f);

            _script.pathSegments.Add(new InteractorTarget.PathSegment(_script.GetEndPosition(_script.targetCount - 1), _script.GetStartPosition(0), startTangent, endTangent, Quaternion.identity, 0));
            _script.backCount = 1;
        }
        private void AddReversedSegments()
        {
            ClearBackSegments();
            InteractorTarget.PathSegment tempSegment;
            InteractorTarget.PathSegment newBackSegment;
            for (int i = 0; i < _script.targetCount; i++)
            {
                tempSegment = _script.pathSegments[_script.targetCount - 1 - i];
                if (_backPath == InteractorTarget.BackPath.Same)
                    tempSegment.endRotation = _script.pathSegments[i].endRotation;
                newBackSegment = new InteractorTarget.PathSegment(tempSegment.endPosition, tempSegment.startPosition, tempSegment.endTangent, tempSegment.startTangent, tempSegment.endRotation, tempSegment.length);
                newBackSegment.bezier = tempSegment.bezier;
                _script.pathSegments.Add(newBackSegment);
                _script.backCount++;
            }
        }
        private void ClearBackSegments()
        {
            if (_script.backCount > 0)
            {
                for (int i = 0; i < _script.backCount; i++)
                {
                    _script.pathSegments.RemoveAt(_script.AllCount - 1);
                }
                _script.backCount = 0;
                if (_currentSegment > _script.targetCount - 1)
                    _currentSegment = _script.targetCount - 1;
            }
        }
        private void SetBackPath()
        {
            if (_backPath == InteractorTarget.BackPath.Same)
            {
                ClearBackSegments();
                AddReversedSegments();
                ValidateSegments();
                SceneView.RepaintAll();
            }
            if (_backPath == InteractorTarget.BackPath.Straight)
            {
                ClearBackSegments();
                InteractorTarget.PathSegment newBackSegment = new InteractorTarget.PathSegment(_script.GetEndPosition(_script.targetCount - 1), _script.GetStartPosition(0), 0);
                _script.pathSegments.Add(newBackSegment);
                _script.backCount++;
                ValidateSegments();
                SceneView.RepaintAll();
            }
            if (_backPath == InteractorTarget.BackPath.Seperate)
            {
                ClearBackSegments();
                AddFirstBackSegment();
                ValidateSegments();
                SceneView.RepaintAll();
            }
        }
        private Vector3 CalculateTangent(int segment, float lerp, int tangentMode)
        {
            Vector3 midStart = (_script.GetStartPosition(segment) + (_script.GetStartTangent(segment) - _script.GetStartPosition(segment)) * lerp);
            Vector3 midEnd = (_script.GetEndPosition(segment) + (_script.GetEndTangent(segment) - _script.GetEndPosition(segment)) * (1 - lerp));
            Vector3 midTangent = (_script.GetStartTangent(segment) + (_script.GetEndTangent(segment) - _script.GetStartTangent(segment)) * lerp);
            Vector3 midTanStart = (midStart + (midTangent - midStart) * lerp);
            Vector3 midTanEnd = (midEnd + (midTangent - midEnd) * (1 - lerp));

            switch (tangentMode)
            {
                case 0: return midStart;
                case 1: return midTanStart;
                case 2: return midTanEnd;
                case 3: return midEnd;
            }
            return Vector3.zero;
        }
        public bool RemoveSegment(int segment)
        {
            if (!_script.SegmentExist(segment)) return false;
            if (segment >= _script.targetCount && _script.backCount == 1) return false;
            if (segment < _script.targetCount && _script.targetCount == 1) return false;
            if (segment == 0) segment++;
            if (segment == _script.targetCount) segment++;
            if (segment >= _script.targetCount) _script.backCount--;

            _script.SetEndPosition(segment - 1, _script.GetEndPosition(segment));
            _script.SetEndTangent(segment - 1, _script.GetEndTangent(segment));
            _script.SetEndRotationWorld(segment - 1, _script.GetEndRotationWorld(segment));
            _script.pathSegments[segment - 1].callEndEvent = _script.pathSegments[segment].callEndEvent;
            _script.pathSegments[segment - 1].followTransform = _script.pathSegments[segment].followTransform;
            _script.pathSegments[segment - 1].blendMatchSource = _script.pathSegments[segment].blendMatchSource;

            _script.pathSegments.RemoveAt(segment);
            _script.SetSegmentDirty(segment - 1);
            _script.SetSegmentDirty(segment);
            return true;
        }
        public void ClearAll()
        {
            _script.pathSegments.Clear();
            if (_script.endEvents != null) _script.endEvents.Clear();
            if (_script.followTransforms != null) _script.followTransforms.Clear();
            _script.backCount = 0;
            _script.stationaryPoints = false;
            _script.dontScale = false;
            _script.stationaryStartTangents = false;
            _script.stationaryEndTangents = false;
            _script.targetTotalLength = 0;
            _script.backTotalLength = 0;
            _totalPathLenght = 0;
        }
        #endregion
    }
}
