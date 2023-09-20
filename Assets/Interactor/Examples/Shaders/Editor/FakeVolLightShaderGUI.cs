using UnityEditor;
using UnityEngine;

namespace razz
{
    public class FakeVolLightShaderGUI : ShaderGUI
    {
        Material _material;
        MaterialProperty[] _props;
        MaterialEditor _materialEditor;

        // Properties
        private MaterialProperty Fresnel = null;
        private MaterialProperty AlphaOffset = null;
        private MaterialProperty NoiseSpeed = null;
        private MaterialProperty Ambient = null;
        private MaterialProperty Intensity = null;
        private MaterialProperty Fade = null;
        private MaterialProperty Wind = null;

        enum Category
        {
            General = 0
        }

        void AssignProperties()
        {
            Fresnel = FindProperty("_Fresnel", _props);
            AlphaOffset = FindProperty("_AlphaOffset", _props);
            NoiseSpeed = FindProperty("_NoiseSpeed", _props);
            Ambient = FindProperty("_Ambient", _props);
            Intensity = FindProperty("_Intensity", _props);
            Fade = FindProperty("_Fade", _props);
            Wind = FindProperty("_Wind", _props);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            _material = materialEditor.target as Material;
            _props = props;
            _materialEditor = materialEditor;

            AssignProperties();

            Layout.Initialize(_material);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(-7);
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            DrawGUI();
            EditorGUILayout.EndVertical();
            GUILayout.Space(1);
            EditorGUILayout.EndHorizontal();

            Undo.RecordObject(_material, "Material Edit");
        }

        static Texture2D bannerTex = null;
        static GUIStyle rateTxt = null;
        static GUIStyle title = null;
        static GUIStyle linkStyle = null;

        void DrawBanner()
        {
            if (bannerTex == null)
                bannerTex = Resources.Load<Texture2D>("Images/banner_fakeVolLight");

            if (rateTxt == null)
            {
                rateTxt = new GUIStyle();
                rateTxt.alignment = TextAnchor.LowerRight;
                rateTxt.normal.textColor = new Color(0, 0, 0);
                rateTxt.fontSize = 9;
                rateTxt.padding = new RectOffset(0, 1, 0, 1);
            }

            if (title == null)
            {
                title = new GUIStyle(rateTxt);
                title.normal.textColor = new Color(1f, 1f, 1f, 0.9f);
                title.alignment = TextAnchor.UpperCenter;
                title.fontSize = 20;
            }

            if (linkStyle == null) linkStyle = new GUIStyle();

            if (bannerTex != null)
            {
                GUILayout.Space(3);
                var rect = GUILayoutUtility.GetRect(0, int.MaxValue, 60, 60);
                EditorGUI.DrawPreviewTexture(rect, bannerTex, null, ScaleMode.ScaleAndCrop);
                rateTxt.alignment = TextAnchor.LowerRight;
                EditorGUI.LabelField(rect, Links.shaderLinkText, rateTxt);

                EditorGUI.LabelField(rect, "Fake Volumetric Light", title);

                if (GUI.Button(rect, "", linkStyle))
                {
                    Links.ShaderLinks();
                }
                GUILayout.Space(3);
            }
        }

        void DrawGUI()
        {
            DrawBanner();

            if (Layout.BeginFold((int)Category.General, "Effects"))
                DrawGeneralSettings();
            Layout.EndFold();
        }

        void DrawGeneralSettings()
        {
            GUILayout.Space(-3);
            EditorGUI.indentLevel++;
            var ofs = EditorGUIUtility.labelWidth;
            _materialEditor.SetDefaultGUIWidths();
            EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.labelWidth = ofs;
            _materialEditor.ShaderProperty(Fresnel, "Fresnel");
            _materialEditor.ShaderProperty(AlphaOffset, "Alpha Offset");
            _materialEditor.ShaderProperty(NoiseSpeed, "Noise Speed");
            _materialEditor.ShaderProperty(Ambient, "Ambient");
            _materialEditor.ShaderProperty(Intensity, "Intensity");
            _materialEditor.ShaderProperty(Fade, "Fade");
            _materialEditor.ShaderProperty(Wind, "Wind");
            EditorGUI.indentLevel--;
        }
    }
}
