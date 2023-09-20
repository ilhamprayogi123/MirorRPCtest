using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace razz
{
    [HelpURL("https://negengames.com/interactor/components.html#extradebugscs")]
    public class ExtraDebugs : MonoBehaviour
    {
        public bool useInteractorTab = true;
        [Range(0, 4)] public int selectedTab; //You can increase the range if you need
        [Range(0.5f, 2f)] public float size = 1f;
        [Range(0, 1f)] public float horizontalPos = 1f;
        [Range(0, 1f)] public float verticalPos = 0;
        [Range(1, 8)] public int maxTargets = 4;
        public bool stackHorizontal = false;
        [Range(0, 60)] public float removeTime = 3f;

        private bool _init;
        private Interactor _interactor;
        private int _selectedTabPrev;
        private GUISkin _skin;
        private GUIStyle _line, _extraDebugStyle;
        private TextAnchor _defaultAnchor;
        private Color _defaultFontColor;
        private Color _red = new Color(1f, 0.1f, 0.1f);
        private Color _green = new Color(0.1f, 1f, 0.1f);
        private float _fontSize;
        private int _defaultFontSize;
        private float _overSizeRatio = 1f;
        private float _finalSize;

        private readonly float _margin = 10f;
        private readonly float _padding = 8f;
        private readonly float _boxWidth = 200f;
        private readonly float _boxHeight = 250f;
        private float _sizedMargin;
        private float _sizedPadding;
        private float _sizedBoxWidth, _sizedBoxHeight;
        private float _screenX, _screenY;
        private int _max;
        private float _neededX, _neededY;
        private float _posX, _posY;
        private Rect _boxArea, _drawArea;
        private Rect _bar, _barBG;
        private Texture2D _tex;
        private GUIStyle _barStyle;
        private Color _defaultGUIColor;
        private Color _barColor = new Color(0.07f, 0.07f, 0.9f);
        private Color _barBGColor = new Color(0.2f, 0.2f, 0.2f);
        private Color _barBGBackColor = new Color(0.9f, 0.07f, 0.07f);

        private List<DebugData> _debugDatas;
        private List<DebugData> _debugDatasRemoved;
        private List<int> _targetIDs;

        public class DebugData
        {
            public InteractorTarget target = null;
            public string targetName = "NULLED";
            public InteractorObject intObj = null;
            public string intObjName = "NULLED";
            public InteractionTypes intType = (InteractionTypes)(-1);
            public bool waiting;
            public bool reposition;
            public float distance = 0;
            public float minDist = 0;
            public float maxDist = 0;
            public bool angleCheck = false;
            public bool obstacleEnabled = false;
            public bool obstacle = false;
            public string obstacleName = "No Obstacle";
            public bool used = false;
            public bool addToUseables = false;
            public int useables = 0;
            public float progress = 0;
            public bool pause = false;
            public int lastStatus = 0;

            public bool updateEnded;

            public DebugData(InteractorTarget target, InteractorObject intObj, InteractionTypes intType)
            {
                if (target)
                {
                    this.target = target;
                    this.targetName = target.name;
                }
                if (intObj)
                {
                    this.intObj = intObj;
                    this.intObjName = intObj.name;
                }
                this.intType = intType;
            }
        }

        private void Start()
        {
            Init();
        }
        private void OnDestroy()
        {
            if (_init)
            {
                _extraDebugStyle.fontSize = _defaultFontSize;
                _extraDebugStyle.normal.textColor = _defaultFontColor;
                GUI.color = _defaultGUIColor;
            }
        }
        private void OnGUI()
        {
            CheckTargets();
            DrawDebug();
        }

        private void OnValidate()
        {
            if (useInteractorTab) return;

            if (_selectedTabPrev != selectedTab)
            {
                _selectedTabPrev = selectedTab;
                ResetDebugData();
            }
        }

        private void Init()
        {
            _skin = Resources.Load<GUISkin>("InteractorGUISkin");
            _line = _skin.GetStyle("HorizontalLine");
            _extraDebugStyle = _skin.GetStyle("ExtraDebug");
            _defaultGUIColor = GUI.color;
            _fontSize = _extraDebugStyle.fontSize;
            _defaultAnchor = _extraDebugStyle.alignment;
            _defaultFontSize = _extraDebugStyle.fontSize;
            _defaultFontColor = _extraDebugStyle.normal.textColor;
            _barStyle = new GUIStyle();
            _barStyle.normal.textColor = Color.white;
            _tex = Texture2D.whiteTexture;
            _barStyle.normal.background = _tex;
            _interactor = GetComponent<Interactor>();
            if (!(_interactor = GetComponent<Interactor>()))
            {
                Debug.LogWarning("Interactor could not found for ExtraDebugs. ExtraDebugs need to be same object with Interactor!", this);
                return;
            }
#if UNITY_EDITOR
            if (useInteractorTab)
            {
                selectedTab = _interactor.selectedTab;
                _selectedTabPrev = selectedTab;
            }
#endif
            ResetDebugData();
            _init = true;
        }

        private void DrawDebug()
        {
            _sizedMargin = _margin * size;
            _sizedPadding = _padding * size;
            _sizedBoxWidth = (_boxWidth * size) - (_sizedMargin * 2);
            _sizedBoxHeight = (_boxHeight * size) + (_sizedPadding * 2);
            _screenX = Screen.width;
            _screenY = Screen.height;
            _max = Mathf.Min(maxTargets, _debugDatas.Count + _debugDatasRemoved.Count);

            _neededX = (_max * (_sizedBoxWidth + _sizedMargin)) + _sizedMargin;
            _neededY = (_max * (_sizedBoxHeight + _sizedMargin)) + _sizedMargin;

            if (stackHorizontal)
            {
                if (_screenX < _neededX) _overSizeRatio = _screenX / _neededX;
                else _overSizeRatio = 1f;
            }
            else
            {
                if (_screenY < _neededY) _overSizeRatio = _screenY / _neededY;
                else _overSizeRatio = 1f;
            }

            _finalSize = size * _overSizeRatio;
            if (_overSizeRatio < 1f)
            {
                _sizedMargin = _margin * _finalSize;
                _sizedPadding = _padding * _finalSize;
                _sizedBoxWidth = (_boxWidth * _finalSize) - (_sizedMargin * 2);
                _sizedBoxHeight = (_boxHeight * _finalSize) + (_sizedPadding * 2);
            }

            for (int i = 0; i < _max; i++)
            {
                if (stackHorizontal)
                {
                    _posX = Mathf.Min((_screenX - _sizedBoxWidth - _sizedMargin) - (_sizedBoxWidth * (_max - 1 - i)) - (_sizedMargin * (_max - 1 - i)), (_sizedBoxWidth * i) + (_sizedMargin * (i + 1)) + (_screenX * horizontalPos));
                    _posY = Mathf.Min((_screenY - _sizedBoxHeight - _sizedMargin), _sizedMargin + (_screenY * verticalPos));
                }
                else
                {
                    _posX = Mathf.Max(((_screenX * horizontalPos) - _sizedBoxWidth - _sizedMargin), _sizedMargin);
                    _posY = Mathf.Min((_screenY - _sizedBoxHeight - _sizedMargin) - (_sizedBoxHeight * (_max - 1 - i)) - (_sizedMargin * (_max - 1 - i)), (_sizedBoxHeight * i) + (_sizedMargin * (i + 1)) + (_screenY * verticalPos));
                }

                _boxArea = new Rect(_posX, _posY, _sizedBoxWidth, _sizedBoxHeight);
                GUI.Box(_boxArea, "");
                _drawArea = new Rect(_boxArea.x + _sizedPadding, _boxArea.y + _sizedPadding, _boxArea.width - _sizedPadding * 2, _boxArea.height - _sizedPadding * 2);

                GUILayout.BeginArea(_drawArea);
                {
                    if (i < _debugDatas.Count)
                    {
                        if (_debugDatas[i].updateEnded)
                            DrawData(i);
                        else DrawUnupdatedData(_debugDatas[i], _defaultFontColor);
                    }
                    else
                    {
                        if (_debugDatasRemoved[i - _debugDatas.Count].updateEnded)
                            DrawRemovedData(i - _debugDatas.Count);
                        else DrawUnupdatedData(_debugDatasRemoved[i - _debugDatas.Count], _red);
                    }
                }
                GUILayout.EndArea();
                _extraDebugStyle.normal.textColor = _defaultFontColor;
                _extraDebugStyle.fontSize = _defaultFontSize;
            }
        }

        private void DrawData(int index)
        {
            _extraDebugStyle.normal.textColor = _defaultFontColor;
            _extraDebugStyle.fontSize = Mathf.RoundToInt(_fontSize * _finalSize);
            GUILayout.Label(_debugDatas[index].intObjName, _extraDebugStyle);
            GUILayout.Label("", _line);
            GUILayout.Space(4f * _finalSize);
            
            GUILayout.BeginVertical();
            _extraDebugStyle.fontSize = Mathf.RoundToInt(_fontSize * _finalSize * 0.75f);
            GUILayout.Label(IsTypeValid(_debugDatas[index].intType), _extraDebugStyle);
            GUILayout.Label(_debugDatas[index].targetName + " ID: " + _targetIDs[index].ToString(), _extraDebugStyle);
            GUILayout.Space(2f * _finalSize);

            _extraDebugStyle.normal.textColor = Color.white;
            GUILayout.Label(LastStatusText(_debugDatas[index].lastStatus, index), _extraDebugStyle);
            DrawProgressBar(_debugDatas[index].progress);
            GUILayout.Label("", _extraDebugStyle);
            GUILayout.Space(2f * _finalSize);

            _extraDebugStyle.normal.textColor = _defaultFontColor;
            if (_debugDatas[index].used)
            {
                GUILayout.Label("OBJECT IS BUSY", _extraDebugStyle);
                GUILayout.Space(2f * _finalSize);
            }
            else
            {
                GUILayout.Label("OBJECT IS AVAILABLE", _extraDebugStyle);
                GUILayout.Space(2f * _finalSize);
            }

            _extraDebugStyle.normal.textColor = _red;
            if (_debugDatas[index].minDist != 0)
            {
                bool distanceCheck = false;
                if (_debugDatas[index].distance > _debugDatas[index].minDist && _debugDatas[index].distance < _debugDatas[index].maxDist)
                {
                    distanceCheck = true;
                    _extraDebugStyle.normal.textColor = _green;
                }

                GUILayout.Label(_debugDatas[index].minDist.ToString("F2") + "         " + _debugDatas[index].distance.ToString("F2") + "         " + _debugDatas[index].maxDist.ToString("F2"), _extraDebugStyle);

                if (distanceCheck)
                {
                    if (!_debugDatas[index].angleCheck)
                    {
                        _extraDebugStyle.normal.textColor = _red;
                        GUILayout.Label("ANGLES FAILED", _extraDebugStyle);
                        GUILayout.Label("NO OBSTACLE CHECKS", _extraDebugStyle);
                        GUILayout.Label("", _extraDebugStyle);
                    }
                    else
                    {
                        GUILayout.Label("ANGLES PASSED", _extraDebugStyle);

                        if (_debugDatas[index].obstacleEnabled)
                        {
                            if (_debugDatas[index].obstacle)
                            {
                                _extraDebugStyle.normal.textColor = _red;
                                GUILayout.Label("OBSTACLE HIT: ", _extraDebugStyle);
                                GUILayout.Label(_debugDatas[index].obstacleName, _extraDebugStyle);
                            }
                            else
                            {
                                _extraDebugStyle.normal.textColor = _green;
                                GUILayout.Label("OBSTACLES CHECKED", _extraDebugStyle);
                                GUILayout.Label("NO OBSTACLE", _extraDebugStyle);
                            }
                        }
                        else
                        {
                            if (_debugDatas[index].angleCheck)
                                _extraDebugStyle.normal.textColor = _green;
                            else _extraDebugStyle.normal.textColor = _red;

                            GUILayout.Label("NO OBSTACLE CHECKS", _extraDebugStyle);
                            GUILayout.Label("", _extraDebugStyle);
                        }
                    }
                }
                else
                {
                    _extraDebugStyle.normal.textColor = _red;
                    GUILayout.Label("NO ANGLE CHECKS", _extraDebugStyle);
                    GUILayout.Label("NO OBSTACLE CHECKS", _extraDebugStyle);
                    GUILayout.Label("", _extraDebugStyle);
                }
            }
            else
            {
                GUILayout.Label("NO POSITON CHECKS", _extraDebugStyle);
                GUILayout.Label("NO ANGLE CHECKS", _extraDebugStyle);
                GUILayout.Label("NO OBSTACLE CHECKS", _extraDebugStyle);
                GUILayout.Label("", _extraDebugStyle);
            }
            GUILayout.Space(2f * _finalSize);

            if (_debugDatas[index].addToUseables)
            {
                _extraDebugStyle.normal.textColor = _green;
                GUILayout.Label("EFFECTOR CAN USE", _extraDebugStyle);
            }
            else
            {
                _extraDebugStyle.normal.textColor = _red;
                GUILayout.Label("EFFECTOR CAN NOT USE", _extraDebugStyle);
            }
            _extraDebugStyle.normal.textColor = _defaultFontColor;
            GUILayout.Label("USEABLE EFFECTORS: " + _debugDatas[index].useables.ToString(), _extraDebugStyle);
            GUILayout.Space(2f * _finalSize);

            if (_debugDatas[index].reposition)
            {
                _extraDebugStyle.normal.textColor = _green;
                GUILayout.Label("TARGET REPOSITIONING...", _extraDebugStyle);
            }
            else
            {
                if (_debugDatas[index].intType == InteractionTypes.TouchHorizontalUp || _debugDatas[index].intType == InteractionTypes.TouchVertical)
                {
                    _extraDebugStyle.normal.textColor = _red;
                    GUILayout.Label("TARGET RAYCAST FAILED", _extraDebugStyle);
                }
                else GUILayout.Label("", _extraDebugStyle);
            }
            GUILayout.EndVertical();
        }
        private void DrawRemovedData(int index)
        {
            _extraDebugStyle.normal.textColor = _red;
            _extraDebugStyle.fontSize = Mathf.RoundToInt(_fontSize * _finalSize);
            GUILayout.Label(_debugDatasRemoved[index].intObjName, _extraDebugStyle);
            GUILayout.Label("", _line);
            GUILayout.Space(4f * _finalSize);

            GUILayout.BeginVertical();
            _extraDebugStyle.fontSize = Mathf.RoundToInt(_fontSize * _finalSize * 0.75f);
            GUILayout.Label(IsTypeValid(_debugDatasRemoved[index].intType), _extraDebugStyle);
            GUILayout.Label(_debugDatasRemoved[index].targetName, _extraDebugStyle);
            GUILayout.Space(2f * _finalSize);

            GUILayout.Label(LastStatusText(_debugDatasRemoved[index].lastStatus, index), _extraDebugStyle);
            DrawProgressBar(_debugDatasRemoved[index].progress);
            GUILayout.Label("", _extraDebugStyle);
            GUILayout.Space(2f * _finalSize);

            if (_debugDatasRemoved[index].used)
            {
                GUILayout.Label("OBJECT IS BUSY", _extraDebugStyle);
                GUILayout.Space(2f * _finalSize);
            }
            else
            {
                GUILayout.Label("OBJECT IS AVAILABLE", _extraDebugStyle);
                GUILayout.Space(2f * _finalSize);
            }

            if (_debugDatasRemoved[index].minDist != 0)
            {
                bool distanceCheck = false;
                if (_debugDatasRemoved[index].distance > _debugDatasRemoved[index].minDist && _debugDatasRemoved[index].distance < _debugDatasRemoved[index].maxDist)
                {
                    distanceCheck = true;
                }

                GUILayout.Label(_debugDatasRemoved[index].minDist.ToString("F2") + "             " + _debugDatasRemoved[index].distance.ToString("F2") + "             " + _debugDatasRemoved[index].maxDist.ToString("F2"), _extraDebugStyle);

                if (distanceCheck)
                {
                    if (!_debugDatasRemoved[index].angleCheck)
                    {
                        GUILayout.Label("ANGLES FAILED", _extraDebugStyle);
                        GUILayout.Label("NO OBSTACLE CHECKS", _extraDebugStyle);
                        GUILayout.Label("", _extraDebugStyle);
                    }
                    else
                    {
                        GUILayout.Label("ANGLES PASSED", _extraDebugStyle);

                        if (_debugDatasRemoved[index].obstacleEnabled)
                        {
                            if (_debugDatasRemoved[index].obstacle)
                            {
                                GUILayout.Label("OBSTACLE HIT: ", _extraDebugStyle);
                                GUILayout.Label(_debugDatasRemoved[index].obstacleName, _extraDebugStyle);
                            }
                            else
                            {
                                GUILayout.Label("OBSTACLES CHECKED", _extraDebugStyle);
                                GUILayout.Label("NO OBSTACLE", _extraDebugStyle);
                            }
                        }
                        else
                        {
                            GUILayout.Label("NO OBSTACLE CHECKS", _extraDebugStyle);
                            GUILayout.Label("", _extraDebugStyle);
                        }
                    }
                }
                else
                {
                    GUILayout.Label("NO ANGLE CHECKS", _extraDebugStyle);
                    GUILayout.Label("NO OBSTACLE CHECKS", _extraDebugStyle);
                    GUILayout.Label("", _extraDebugStyle);
                }
            }
            else
            {
                GUILayout.Label("NO POSITON CHECKS", _extraDebugStyle);
                GUILayout.Label("NO ANGLE CHECKS", _extraDebugStyle);
                GUILayout.Label("NO OBSTACLE CHECKS", _extraDebugStyle);
                GUILayout.Label("", _extraDebugStyle);
            }
            GUILayout.Space(2f * _finalSize);

            if (_debugDatasRemoved[index].addToUseables)
            {
                GUILayout.Label("EFFECTOR CAN USE", _extraDebugStyle);
            }
            else
            {
                GUILayout.Label("EFFECTOR CAN NOT USE", _extraDebugStyle);
            }
            GUILayout.Label("USEABLE EFFECTORS: " + _debugDatasRemoved[index].useables.ToString(), _extraDebugStyle);
            GUILayout.Space(2f * _finalSize);

            if (_debugDatasRemoved[index].reposition)
            {
                _extraDebugStyle.normal.textColor = _green;
                GUILayout.Label("TARGET REPOSITIONING...", _extraDebugStyle);
            }
            else
            {
                if (_debugDatasRemoved[index].intType == InteractionTypes.TouchHorizontalUp || _debugDatasRemoved[index].intType == InteractionTypes.TouchVertical)
                {
                    _extraDebugStyle.normal.textColor = _red;
                    GUILayout.Label("TARGET RAYCAST FAILED", _extraDebugStyle);
                }
                else GUILayout.Label("", _extraDebugStyle);
            }
            GUILayout.EndVertical();
        }
        private void DrawUnupdatedData(DebugData debugData, Color color)
        {
            _extraDebugStyle.normal.textColor = color;
            _extraDebugStyle.fontSize = Mathf.RoundToInt(_fontSize * _finalSize);
            GUILayout.Label(debugData.intObjName, _extraDebugStyle);
            GUILayout.Label("", _line);
            GUILayout.Space(4f * _finalSize);

            GUILayout.BeginVertical();
            _extraDebugStyle.fontSize = Mathf.RoundToInt(_fontSize * _finalSize * 0.75f);
            GUILayout.Label(IsTypeValid(debugData.intType), _extraDebugStyle);
            GUILayout.Label(debugData.targetName, _extraDebugStyle);
            GUILayout.Space(20f * _finalSize);

            _extraDebugStyle.normal.textColor = _red;
            GUILayout.Label("DEBUG LIMITATION", _extraDebugStyle);
            GUILayout.Space(2f * _finalSize);

            _extraDebugStyle.normal.textColor = color;
            GUILayout.Label("Can't Debug More Than", _extraDebugStyle);
            GUILayout.Label("One Target For Same", _extraDebugStyle);
            GUILayout.Label("Effector Type and Object", _extraDebugStyle);
            GUILayout.Space(_sizedBoxHeight * 0.35f);
            GUILayout.EndVertical();
            
        }
        private void DrawProgressBar(float progress)
        {
            float yValue = 82f;
            float width = _sizedBoxWidth - _sizedPadding * 2;
            float widthPercent = width * progress;
            string path = "TargetPath - ";
            GUI.color = _barBGColor;
            if (progress > 1)
            {
                path = "BackPath - ";
                widthPercent = width * (1f - (progress - 1f));
                GUI.color = _barBGBackColor;
            }

            _barBG = new Rect(0, yValue * _finalSize, width, _extraDebugStyle.fontSize + 5f);
            _bar = new Rect(0, yValue * _finalSize, widthPercent, _extraDebugStyle.fontSize + 5f);
            
            GUI.Box(_barBG, GUIContent.none, _barStyle);
            GUI.color = _barColor;
            GUI.Box(_bar, GUIContent.none, _barStyle);
            GUI.color = Color.white;
            _extraDebugStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(_barBG, path + progress.ToString("F2"), _extraDebugStyle);
            _extraDebugStyle.alignment = _defaultAnchor;

            GUI.color = _defaultGUIColor;
        }

        public void SetSelectedTab(int newSelection)
        {
            if (_selectedTabPrev != newSelection)
            {
                _selectedTabPrev = newSelection;
                selectedTab = newSelection;
                ResetDebugData();
            }
        }

        public int GetSelectedTab()
        {
            if (selectedTab >= _interactor.effectorLinks.Count || selectedTab < 0) 
            {
                Debug.LogWarning("Selected tab does not exist on Interactor Effectors.", this);
                return -1;
            }

            return selectedTab;
        }

        public int GetSelectedEffectorType()
        {
            if (selectedTab >= _interactor.effectorLinks.Count) return -1;
            if (selectedTab < 0) return -1;

            return (int)_interactor.effectorLinks[selectedTab].effectorType;
        }

        public void AddNewDebugData(int targetID, InteractorTarget target, InteractorObject intObj, InteractionTypes intType)
        {
            if (!_targetIDs.Contains(targetID) && targetID != 0)
            {
                _debugDatas.Add(new DebugData(target, intObj, intType));
                _targetIDs.Add(targetID);
            }
        }

        public void RemoveDebugData(int targetID)
        {
            int index = _targetIDs.IndexOf(targetID);
            if (index >= 0)
            {
                if (removeTime > 0) DelayedRemoval(index);
                else
                {
                    _debugDatas.RemoveAt(index);
                    _targetIDs.RemoveAt(index);
                }
            }
        }
        private void DelayedRemoval(int index)
        {
            _debugDatasRemoved.Add(_debugDatas[index]);
            StartCoroutine(RemoveLater(_debugDatas[index]));

            _debugDatas.RemoveAt(index);
            _targetIDs.RemoveAt(index);
        }
        private IEnumerator RemoveLater(DebugData debugData)
        {
            yield return new WaitForSeconds(removeTime);
            _debugDatasRemoved.Remove(debugData);
        }

        public void UpdateDebugData(int targetID, DebugData debugData, bool updateEnded)
        {
            int index = _targetIDs.IndexOf(targetID);
            if (index >= 0)
            {
                _debugDatas[index].waiting = debugData.waiting;
                _debugDatas[index].reposition = debugData.reposition;
                _debugDatas[index].distance = debugData.distance;
                _debugDatas[index].minDist = debugData.minDist;
                _debugDatas[index].maxDist = debugData.maxDist;
                _debugDatas[index].angleCheck = debugData.angleCheck;
                _debugDatas[index].obstacleEnabled = debugData.obstacleEnabled;
                _debugDatas[index].obstacle = debugData.obstacle;
                if (!string.IsNullOrEmpty(debugData.obstacleName))
                    _debugDatas[index].obstacleName = debugData.obstacleName;
                _debugDatas[index].used = debugData.used;
                _debugDatas[index].addToUseables = debugData.addToUseables;
                _debugDatas[index].useables = debugData.useables;
                _debugDatas[index].progress = debugData.progress;
                _debugDatas[index].pause = debugData.pause;
                _debugDatas[index].lastStatus = debugData.lastStatus;

                _debugDatas[index].updateEnded = updateEnded;
            }
            else if (targetID > 0)
                AddNewDebugData(targetID, debugData.target, debugData.intObj, debugData.intType);
        }

        private void CheckTargets()
        {
            for (int i = 0; i < _debugDatas.Count; i++)
                if (!IsTargetValid(i)) RemoveDebugData(_targetIDs[i]);
        }

        private bool IsTargetValid(int index)
        {
            InteractorObject intObj = _debugDatas[index].intObj;
            InteractorTarget target = _debugDatas[index].target;
            if (intObj)
            {
                if (target)
                {
                    InteractorTarget[] temp = intObj.GetTargetsForEffectorType((int)target.effectorType);
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (temp[i] == target)
                            return true;
                    }
                }
            }
            return false;
        }

        private string IsTypeValid(InteractionTypes intType)
        {
            if ((int)intType < 0) return "Invalid Type";
            else return intType.ToString();
        }

        private string LastStatusText(int lastStatus, int index)
        {
            if (index >= _debugDatas.Count) return "TARGET REMOVED";

            switch (lastStatus)
            {
                case 0:
                    {
                        if (_debugDatas[index].waiting)
                        {
                            if (_debugDatas[index].intType == InteractionTypes.PickableTwo || _debugDatas[index].intType == InteractionTypes.Push)
                            {
                                if (_debugDatas[index].addToUseables)
                                    return "WAITING FOR OTHER...";
                                else return "CHECKING...";
                            }
                            else
                            {
                                return "WAITING FOR OTHER...";
                            }
                        }
                        return "CHECKING...";
                    }
                case 3: if(_debugDatas[index].pause) return "INTERACTION PAUSED"; 
                    else return "INTERACTION STARTED";
                case 4: return "INTERACTION RESUMED";
                case 5: return "INTERACTION REVERSED";
                case 6: return "INTERACTION STOPPED";
            }
            return "";
        }

        private void ResetDebugData()
        {
            _debugDatas = new List<DebugData>();
            _debugDatasRemoved = new List<DebugData>();
            _targetIDs = new List<int>();
        }
    }
}
