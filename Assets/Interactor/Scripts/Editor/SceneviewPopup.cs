using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace razz
{
    internal sealed class SceneviewPopup : PopupWindowContent
    {
        public SceneviewPopup(List<GameObject> prefabs, Vector2 mousePos)
        {
            this._prefabs = prefabs;
            _mousePos = mousePos;
        }

        private List<GameObject> _prefabs;
        private float _buttonAndIconsWidth;
        private float _buttonWidth;
        private float _iconWidth;
        private Vector2 _mousePos;

        private Styles _styles;
        private List<Component> _components = new List<Component>(8);
        private Vector2 _scroll;
        private Rect _contentRect;

        private GUISkin _skin;
        private Texture2D _interactorLogo;

        private Dictionary<GameObject, Texture2D[]> _iconLookup = new Dictionary<GameObject, Texture2D[]>(64);

        //A lookup to avoid showing the same icon multiple times.
        private HashSet<Texture2D> _displayedIcons = new HashSet<Texture2D>();

        //These types are displayed as component icons in the popup window.
        private static HashSet<Type> _iconTypes = new HashSet<Type>()
        {
            typeof(Transform)
        };

        private class Styles
        {
            public Styles()
            {
                prefabLabel = new GUIStyle("PR PrefabLabel");
                prefabLabel.alignment = TextAnchor.MiddleLeft;

                label = new GUIStyle(EditorStyles.label);
                label.alignment = TextAnchor.MiddleLeft;
                var p = label.padding;
                p.top -= 1;
                p.left -= 1;
                label.padding = p;
            }

            public GUIStyle LabelStyle(GameObject target)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(target))
                    return prefabLabel;
                else
                    return label;
            }

            public GUIStyle label;

            private static GUIStyle prefabLabel;

            public Vector2 iconSize = new Vector2(16, 16);

            private static readonly Color splitterDark = new Color(0.12f, 0.12f, 0.12f, 1.333f);
            private static readonly Color splitterLight = new Color(0.6f, 0.6f, 0.6f, 1.333f);

            public Color splitterColor { get { return EditorGUIUtility.isProSkin ? splitterDark : splitterLight; } }

            private static readonly Color hoverDark = new Color(0.1f, 0.2f, 0.3f, 0.4f);
            private static readonly Color hoverLight = new Color(0.5f, 0.6f, 0.7f, 0.6f);

            public Color rowHoverColor { get { return EditorGUIUtility.isProSkin ? hoverDark : hoverLight; } }

            private GUIContent tempContent = new GUIContent();

            public GUIContent TempContent(string text, Texture2D image)
            {
                tempContent.text = text;
                tempContent.image = image;
                return tempContent;
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            _skin = Resources.Load<GUISkin>("InteractorGUISkin");
            _interactorLogo = Resources.Load<Texture2D>("Images/LogoIconTarget16px");
            _styles = new Styles();
            editorWindow.wantsMouseMove = true;
            PrecalculateRequiredSizes();
        }

        private void PrecalculateRequiredSizes()
        {
            _buttonWidth = 0;

            for (int i = 0; i < _prefabs.Count; i++)
            {
                float width = _prefabs[i] != null ? _styles.label.CalcSize(new GUIContent(_prefabs[i].name)).x : 0f;

                int maxWidth = 300;
                if (width > maxWidth)
                    width = maxWidth;

                //Width of prefab name to be cropped
                if (width > this._buttonWidth)
                    this._buttonWidth = width + 80f;
            }

            this._buttonWidth += EditorGUIUtility.standardVerticalSpacing;

            _iconWidth = 0;

            for (int i = 0; i < _prefabs.Count; i++)
            {
                if (_prefabs[i] == null)
                    continue;

                _prefabs[i].GetComponents<Component>(_components);

                _displayedIcons.Clear();

                for (int j = 0; j < _components.Count; j++)
                {
                    if (_components[j] == null)
                    {
                        Debug.Log("There is a problem with one of prefabs(or more) that assigned to InteractorTargetSpawner, please check prefabs (Missing script etc).");
                        continue;
                    }

                    var type = _components[j].GetType();

                    if (!_iconTypes.Contains(type))
                        continue;

                    Texture2D componentIcon = AssetPreview.GetMiniThumbnail(_components[j]);

                    if (_displayedIcons.Contains(componentIcon))
                        continue;

                    _displayedIcons.Add(componentIcon);
                }

                _displayedIcons.Add(_interactorLogo);
                _iconLookup.Add(_prefabs[i], _displayedIcons.ToArray());
                float iconWidth = (18 * _displayedIcons.Count);

                if (iconWidth > this._iconWidth)
                    this._iconWidth = iconWidth;
            }
            this._buttonAndIconsWidth = this._buttonWidth + this._iconWidth + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect rect)
        {
            Event current = Event.current;
            GUI.skin = _skin;
            Rect windowRect = new Rect(0, 0, EditorGUIUtility.currentViewWidth, Mathf.Max(GetWindowSize().y, 450));
            GUI.Box(windowRect, "", GUI.skin.GetStyle("BackgroundStyle"));

            _scroll = GUI.BeginScrollView(rect, _scroll, _contentRect, _skin.GetStyle("horizontalscrollbar"), _skin.GetStyle("verticalscrollbar"));
            rect.height = EditorGUIUtility.singleLineHeight + 2;
            rect.y += EditorGUIUtility.standardVerticalSpacing;
            rect.y -= 1;
            rect.xMin += 2;
            rect.xMax -= 2;
            using (new EditorGUIUtility.IconSizeScope(_styles.iconSize))
            {
                for (int i = 0; i < _prefabs.Count; i++)
                {
                    DrawRow(rect, current, _prefabs[i]);
                    rect.y += RowHeight();

                    if (i < _prefabs.Count - 1)
                        DrawSplitter(rect);
                }
            }
            GUI.EndScrollView();

            if (current.type == EventType.MouseMove)
                editorWindow.Repaint();
        }

        private void DrawSplitter(Rect rect)
        {
            rect.height = 1;
            rect.y -= 1;
            rect.xMin = 0f;
            rect.width += 4f;
            EditorGUI.DrawRect(rect, _styles.splitterColor);
        }

        private void DrawRow(Rect rect, Event current, GameObject target)
        {
            if (rect.Contains(current.mousePosition) && current.type != EventType.MouseDrag)
            {
                Rect background = rect;
                background.xMin -= 1;
                background.xMax += 1;
                background.yMax += 1;
                EditorGUI.DrawRect(background, _styles.rowHoverColor);
            }

            Rect originalRect = rect;
            var icon = AssetPreview.GetMiniThumbnail(target);
            Rect iconRect = rect;
            iconRect.width = 20;

            EditorGUI.LabelField(iconRect, _styles.TempContent(null, icon));

            rect.x = iconRect.xMax;
            rect.width = _buttonWidth;

            var nameContent = _styles.TempContent(target != null ? target.name : "Null", null);
            EditorGUI.LabelField(rect, nameContent, GUI.skin.GetStyle("grey_border"));

            if (current.type == EventType.MouseDown &&
                originalRect.Contains(current.mousePosition))
            {
                if (current.shift || current.control)
                {
                    //SpawnSelectedObjectAlternative();
                }
                else
                {
                    SpawnSelectedObject(target);
                }

                if (base.editorWindow)
                    base.editorWindow.Close();
                GUIUtility.ExitGUI();
            }

            if (target == null)
                return;

            Rect componentIconRect = rect;
            componentIconRect.x = rect.xMax;
            componentIconRect.width = rect.height;

            var icons = _iconLookup[target];
            for (int i = 0; i < icons.Length; i++)
            {
                EditorGUI.LabelField(componentIconRect, _styles.TempContent(null, icons[i]));
                componentIconRect.x = componentIconRect.xMax;
            }
        }

        private void SpawnSelectedObject(GameObject selectedObject)
        {
            InteractorTargetSpawner.SpawnPrefab(selectedObject, _mousePos);
        }

        private float RowHeight()
        {
            return EditorGUIUtility.singleLineHeight + 2f + EditorGUIUtility.standardVerticalSpacing;
        }

        public override Vector2 GetWindowSize()
        {
            float height = RowHeight() * _prefabs.Count;
            height += EditorGUIUtility.standardVerticalSpacing;

            float preIconWidth = 22f;
            var size = new Vector2(preIconWidth + _buttonAndIconsWidth, height - 1);

            _contentRect = new Rect(Vector2.zero, size);
            int maxHeight = Mathf.Min(Screen.currentResolution.height, 800);
            if (height > maxHeight)
            {
                size.y = maxHeight;
                size.x += 14;
            }
            return size;
        }
    }
}
