using UnityEditor;
using UnityEngine;

namespace razz
{
    public class OutlineShaderGUI : ShaderGUI
    {
        Material _material;
        MaterialProperty[] _props;
        MaterialEditor _materialEditor;

        // Properties
        private MaterialProperty Albedo = null;
        private MaterialProperty AlbedoColor = null;

        private MaterialProperty MetallicGloss = null;
        private MaterialProperty Smoothness = null;
        private MaterialProperty Metallic = null;

        private MaterialProperty Normal = null;
        private MaterialProperty NormalAmount = null;

        private MaterialProperty Occlusion = null;
        private MaterialProperty OcclusionAmount = null;

        private MaterialProperty FirstOutlineColor = null;
        private MaterialProperty FirstOutlinesWidth = null;

        private MaterialProperty SecondOutlineColor = null;
        private MaterialProperty SecondOutlinesWidth = null;

        private MaterialProperty SwitchAngle = null;

        private static class Styles
        {
            public static GUIContent AlbedoTex = new GUIContent("Albedo");
            public static GUIContent MetalTex = new GUIContent("Metallic");
            public static GUIContent NormalTex = new GUIContent("Normal Map");
            public static GUIContent AOTex = new GUIContent("Occlusion");
        }

        enum Category
        {
            General = 0,
            Effects,
        }

        void AssignProperties()
        {
            Albedo = FindProperty("_MainTex", _props);
            AlbedoColor = FindProperty("_Color", _props);

            MetallicGloss = FindProperty("_MetallicGlossMap", _props);
            Smoothness = FindProperty("_Glossiness", _props);
            Metallic = FindProperty("_Metallic", _props);

            Normal = FindProperty("_Normal", _props);
            NormalAmount = FindProperty("_NormalAmount", _props);

            Occlusion = FindProperty("_AO", _props);
            OcclusionAmount = FindProperty("_AOAmount", _props);

            FirstOutlineColor = FindProperty("_FirstOutlineColor", _props);
            FirstOutlinesWidth = FindProperty("_FirstOutlineWidth", _props);

            SecondOutlineColor = FindProperty("_SecondOutlineColor", _props);
            SecondOutlinesWidth = FindProperty("_SecondOutlineWidth", _props);

            SwitchAngle = FindProperty("_Angle", _props);
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

            Undo.RecordObject(_material, "Material Edition");
        }

        static Texture2D bannerTex = null;
        static GUIStyle rateTxt = null;
        static GUIStyle title = null;
        static GUIStyle linkStyle = null;

        void DrawBanner()
        {
            if (bannerTex == null)
                bannerTex = Resources.Load<Texture2D>("Images/banner_outlines");

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

                EditorGUI.LabelField(rect, "Standart Metallic Outlines", title);

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

            if (Layout.BeginFold((int)Category.General, "Unity Standart Shader (Metallic)"))
                DrawGeneralSettings();
            Layout.EndFold();

            if (Layout.BeginFold((int)Category.Effects, "Outline Effects"))
            {
                DrawFirstOutline();
                DrawSecondOutline();
                DrawAngleSetting();
            }
            Layout.EndFold();
        }

        void DrawGeneralSettings()
        {
            GUILayout.Space(-3);
            GUILayout.Label("Main Maps", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            var ofs = EditorGUIUtility.labelWidth;
            _materialEditor.SetDefaultGUIWidths();
            EditorGUIUtility.labelWidth = 0;
            _materialEditor.TexturePropertySingleLine(Styles.AlbedoTex, Albedo, AlbedoColor);
            _materialEditor.TexturePropertySingleLine(Styles.MetalTex, MetallicGloss, Metallic);
            _materialEditor.ShaderProperty(Smoothness, "Smoothness");
            _materialEditor.TexturePropertySingleLine(Styles.NormalTex, Normal, NormalAmount);
            _materialEditor.TexturePropertySingleLine(Styles.AOTex, Occlusion, OcclusionAmount);
            EditorGUIUtility.labelWidth = ofs;
            EditorGUI.indentLevel--;
        }

        void DrawFirstOutline()
        {
            GUILayout.Space(-3);
            GUILayout.Label("First Outline Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            var ofs = EditorGUIUtility.labelWidth;
            _materialEditor.SetDefaultGUIWidths();
            _materialEditor.ShaderProperty(FirstOutlineColor, "First Outline Color");
            _materialEditor.ShaderProperty(FirstOutlinesWidth, "First Outline Width");
            EditorGUIUtility.labelWidth = ofs;
            EditorGUI.indentLevel--;
        }

        void DrawSecondOutline()
        {
            GUILayout.Space(-3);
            GUILayout.Label("Second Outline Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            var ofs = EditorGUIUtility.labelWidth;
            _materialEditor.SetDefaultGUIWidths();
            _materialEditor.ShaderProperty(SecondOutlineColor, "Second Outline Color");
            _materialEditor.ShaderProperty(SecondOutlinesWidth, "Second Outline Width");
            EditorGUIUtility.labelWidth = ofs;
            EditorGUI.indentLevel--;
        }

        void DrawAngleSetting()
        {
            GUILayout.Space(-3);
            GUILayout.Label("Angle to Switch Outline", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            var ofs = EditorGUIUtility.labelWidth;
            _materialEditor.SetDefaultGUIWidths();
            _materialEditor.ShaderProperty(SwitchAngle, "Switch Angle");
            EditorGUIUtility.labelWidth = ofs;
            EditorGUI.indentLevel--;
        }
    }
}
