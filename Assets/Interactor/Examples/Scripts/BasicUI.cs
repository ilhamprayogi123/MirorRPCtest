using UnityEngine;

namespace razz
{
    [HelpURL("https://negengames.com/interactor/components.html#basicuics")]
    [DisallowMultipleComponent]
    public class BasicUI : MonoBehaviour
    {
        private Interactor _interactor;
        private Texture2D _crosshair;
        private GUISkin _skin;
        private GUIStyle _line, _label, _scenelabel;
        private bool _basicInputExist;
        private bool _basicInputInteractExist;
        private bool _init;

        public bool crosshairEnable = true;
        
        private void Start()
        {
            _interactor = GetComponent<Interactor>();
            if (FindObjectOfType<BasicInput>())
                _basicInputExist = true;
            if (FindObjectOfType<BasicInputInteract>())
                _basicInputInteractExist = true;
        }

        private void GetStyle()
        {
            if (_init) return;
#if UNITY_EDITOR
            _skin = Resources.Load<GUISkin>("InteractorGUISkin");
            _line = _skin.GetStyle("HorizontalLine");
            _label = _skin.GetStyle("label");
            _scenelabel = _skin.GetStyle("LabelSceneview");
#else
            _skin = GUI.skin;
            _line = _skin.GetStyle("Label");
            _label = _skin.GetStyle("Label");
            _scenelabel = _skin.GetStyle("Label");
#endif
            _crosshair = new Texture2D(2, 2);
            _init = true;
        }

        private void Update()
        {
            if (_interactor == null) return;

            int i = 0;
            if (_interactor.selfInteractionEnabled)
            {
                i = 1;
            }

            float mouseWheel = 0;
            if (_basicInputExist)
                mouseWheel = BasicInput.GetMouseWheel();
            else if(_basicInputInteractExist)
                mouseWheel = BasicInputInteract.GetMouseWheel();

            if (mouseWheel < 0)
            {
                if (_interactor.selectedByUI < _interactor.intObjComponents.Count)
                {
                    _interactor.selectedByUI++;

                    if (_interactor.selectedByUI < _interactor.intObjComponents.Count)
                    {
                        _interactor.NewLookOrder(_interactor.intObjComponents[_interactor.selectedByUI].interactorObject, Look.OnSelection);
                    }
                    else
                        _interactor.NewLookOrder(null, Look.OnSelection);
                }   
            }
            else if (mouseWheel > 0)
            {
                if (_interactor.selectedByUI > i)
                {
                    _interactor.selectedByUI--;

                    if (_interactor.selectedByUI < _interactor.intObjComponents.Count)
                    {
                        _interactor.NewLookOrder(_interactor.intObjComponents[_interactor.selectedByUI].interactorObject, Look.OnSelection);
                    }
                    else
                        _interactor.NewLookOrder(null, Look.OnSelection);
                }
            }
        }

        private void OnGUI()
        {
            GetStyle();
            if (crosshairEnable)
            {
                Crosshair();
            }

            if (_interactor == null) return;
            if (_interactor.intObjComponents.Count <= 0) return;
            if (_interactor.selfInteractionEnabled && _interactor.intObjComponents.Count <= 1) return;
            if (!_interactor.selfInteractionEnabled && _interactor.intObjComponents.Count <= 0) return;

            ShowIntObjList();
        }

        private void Crosshair()
        {
            GUILayout.BeginArea(new Rect((Screen.width * 0.5f) - 1f, (Screen.height * 0.5f) - 1f, (Screen.width * 0.5f) + 1f, (Screen.height * 0.5f) + 1f));
            GUILayout.Label(_crosshair);
            GUILayout.EndArea();
        }

        private void ShowIntObjList()
        {
            GUILayout.Space(10f);
            GUILayout.Label("  Interaction Objects in Range (Closest): ", _scenelabel);
            GUILayout.Space(3f);
            GUILayout.Label("", _line);

            int i = 0;
            if (_interactor.selfInteractionEnabled)
                i = 1;
            
            while (i <= _interactor.intObjComponents.Count)
            {
                GUI.color = Color.white;

                if (i == _interactor.selectedByUI)
                {
                    GUI.color = Color.red;
                }

                if (i == _interactor.intObjComponents.Count)
                {
                    if (i > 1)
                        GUILayout.Label("\n  ALL Objects", _scenelabel);
                }
                else
                {
                    if (!_interactor.intObjComponents[i].interactorObject)
                    {
                        i++;
                        continue;
                    }

                    GUILayout.Label("\n  " + _interactor.intObjComponents[i].interactorObject.name, _scenelabel);

                    if (_interactor.intObjComponents[i].interactorObject.ready && i == _interactor.selectedByUI)
                    {
                        GUI.color = Color.white;

                        if (_interactor.intObjComponents[i].interactorObject.interactionType == InteractionTypes.DistanceCrosshair)
                        {
                            GUI.Label(new Rect(Screen.width / 2 - 40, Screen.height / 3 * 2, 150, 50), "Left Click to Use", _label);
                        }
                        else
                        {
                            GUI.Label(new Rect(Screen.width / 2 - 40, Screen.height / 3 * 2, 150, 50), "Press E to Use", _label);
                        }
                    }
                }
                i++;
            }
        }
    }
}
