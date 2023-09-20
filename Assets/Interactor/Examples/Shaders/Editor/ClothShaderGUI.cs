using UnityEditor;
using UnityEngine;

namespace razz
{
    public class ClothShaderGUI : ShaderGUI
    {
        Material _material;
        MaterialProperty[] _props;
        MaterialEditor _materialEditor;

        // Properties
        private MaterialProperty Albedo = null;
        private MaterialProperty AlbedoColor = null;
        private MaterialProperty Normal = null;
        private MaterialProperty Specular = null;
        private MaterialProperty Gloss = null;

        private MaterialProperty RimColor = null;
        private MaterialProperty RimPower = null;
        private MaterialProperty Transmittance = null;

        private static class Styles
        {
            public static GUIContent AlbedoTex = new GUIContent("Albedo");
            public static GUIContent NormalTex = new GUIContent("Normal");
        }

        enum Category
        {
            General = 0,
            Effects,
        }

        void AssignProperties()
        {
            Albedo = FindProperty("_MainTex", _props);
            AlbedoColor = FindProperty("_BodyColor", _props);
            Normal = FindProperty("_BumpTex", _props);

            Specular = FindProperty("_Specular", _props);
            Gloss = FindProperty("_Gloss", _props);

            RimColor = FindProperty("_RimColor", _props);
            RimPower = FindProperty("_RimPower", _props);
            Transmittance = FindProperty("_Transmittance", _props);
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
                bannerTex = Resources.Load<Texture2D>("Images/banner_cloth");

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

                EditorGUI.LabelField(rect, "Transmissive Cloth", title);

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

            if (Layout.BeginFold((int)Category.General, "Base"))
                DrawGeneralSettings();
            Layout.EndFold();

            if (Layout.BeginFold((int)Category.Effects, "Effects"))
            {
                DrawGeneralEffect();
                DrawRimSettings();
            }
            Layout.EndFold();
        }

        void DrawGeneralEffect()
        {
            GUILayout.Space(-3);
            GUILayout.Label("Transmittance", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            var ofs = EditorGUIUtility.labelWidth;
            _materialEditor.SetDefaultGUIWidths();
            _materialEditor.ShaderProperty(Transmittance, "Amount");
            EditorGUIUtility.labelWidth = ofs;
            EditorGUI.indentLevel--;
        }

        void DrawGeneralSettings()
        {
            GUILayout.Space(-3);
            EditorGUI.indentLevel++;
            var ofs = EditorGUIUtility.labelWidth;
            _materialEditor.SetDefaultGUIWidths();
            EditorGUIUtility.labelWidth = 0;
            _materialEditor.TexturePropertySingleLine(Styles.AlbedoTex, Albedo, AlbedoColor);
            EditorGUIUtility.labelWidth = ofs;
            _materialEditor.TexturePropertySingleLine(Styles.NormalTex, Normal);
            _materialEditor.ShaderProperty(Specular, "Specular");
            _materialEditor.ShaderProperty(Gloss, "Gloss");
            EditorGUI.indentLevel--;
        }

        void DrawRimSettings()
        {
            GUILayout.Space(-3);
            GUILayout.Label("Rim Light", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            var ofs = EditorGUIUtility.labelWidth;
            _materialEditor.SetDefaultGUIWidths();
            _materialEditor.ShaderProperty(RimColor, "Color");
            _materialEditor.ShaderProperty(RimPower, "Power");
            EditorGUIUtility.labelWidth = ofs;
            EditorGUI.indentLevel--;
        }
    }
}
