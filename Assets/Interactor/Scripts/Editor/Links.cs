using UnityEngine;
using UnityEditor;

namespace razz
{
    public class Links : Editor
    {
        public static string onlineDocName = "Docs";
        public static string onlineDocDesc = "Go to online documentation and videos";
        [MenuItem("Window/Interactor/Online Documentation")]
        public static void OnlineDocumentataion()
        {
            Application.OpenURL("https://negengames.com/interactor");
        }

        public static string forumName = "Forum";
        public static string forumDesc = "Go to official forum thread to ask questions or write suggestions. I'm answering daily (well, mostly). If you're having a problem, found a bug or want to share your great ideas, prefer forum thread please, so others can search and read same informations. Discord is isolating and killing data.";
        public static void Forum()
        {
            Application.OpenURL("https://negengames.com/interactor/forum.html");
        }

        public static string messageName = "Message";
        public static string messageDesc = "Start a private conversation with me for anything, when you have great suggestions or when you need help. You need to be signed into Unity Forums. Or you can send an email to me: support@negengames.com";
        public static void Message()
        {
            Application.OpenURL("https://forum.unity.com/conversations/add?to=razzraziel");
        }

        public static string storeName = "Store Page";
        public static string storeDesc = "Open Asset Page to check updates or rate & write a honest review. If you like my work, it's a good place to show some love and support development/future updates. If you don't like, first send a message about why and I'll try my best to help.";
        public static void Store(bool reviewPage)
        {
            if (reviewPage)
            {
#if UNITY_2020_OR_NEWER
            Application.OpenURL("https://assetstore.unity.com/packages/slug/178062#reviews");
#else
                UnityEditorInternal.AssetStore.Open("content/178062/reviews");
#endif
            }
            else
            {
#if UNITY_2020_OR_NEWER
            Application.OpenURL("https://assetstore.unity.com/packages/slug/178062");
#else
                UnityEditorInternal.AssetStore.Open("content/178062");
#endif
            }
        }
        public static void Support()
        {
            Application.OpenURL("https://negengames.com/interactor/documentation.html#support");
        }

        public static string interactorScriptName = "Interactor Main Loop";
        public static string interactorScriptDesc = "If you like to edit main Interactor script, this goes into codes.";
        public static void InteractorScript()
        {
            MonoScript ms = (MonoScript)AssetDatabase.LoadAssetAtPath("Assets/Interactor/Scripts/Interactor.cs", typeof(MonoScript));
            AssetDatabase.OpenAsset(ms, 1664);
        }

        public static string interactorEditorScriptName = "Expose Properties";
        public static string interactorEditorScriptDesc = "If you like to add more properties here, this goes into editor script codes.";
        public static void InteractorEditorScript()
        {
            MonoScript ms = (MonoScript)AssetDatabase.LoadAssetAtPath("Assets/Interactor/Scripts/Editor/InteractorEditor.cs", typeof(MonoScript));
            AssetDatabase.OpenAsset(ms, 1339);
        }

        public static string shaderLinkText = "Online Documentation      ";
        public static void ShaderLinks()
        {
            Application.OpenURL("https://negengames.com/interactor/documentation.html#shaders");
        }
    }
}
