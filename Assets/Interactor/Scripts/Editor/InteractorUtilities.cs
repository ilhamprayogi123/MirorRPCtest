using UnityEngine;
using UnityEditor;

namespace razz
{
    //Save, check or replace SaveData ScriptableObject assets
    public static class InteractorUtilities
    {
        public struct SaveBundle
        {
            public SaveData saveData;
            public string savePath;
        }

        public static SaveBundle LoadAsset(string savePath)
        {
            SaveBundle send = new SaveBundle();

            if (savePath == null || savePath == "" || !System.IO.File.Exists(savePath))
            {
                send = SaveSelectOrCreate(send);
            }
            else
            {
                send.savePath = savePath;
            }

            send.saveData = AssetDatabase.LoadAssetAtPath(send.savePath, typeof(SaveData)) as SaveData;
            return send;
        }

        private static SaveBundle SaveSelectOrCreate(SaveBundle send)
        {
            int answer = EditorUtility.DisplayDialogComplex("Save or Open new file", "Interactor could not find a saved file. If you have, you can select in your project or create a new save file.", "Open Save File", "Cancel", "Create New Save");

            string savePath;

            if (answer == 0)
            {
                savePath = EditorUtility.OpenFilePanel("Select a Save File", Application.dataPath, "asset");
                if (savePath == null || savePath == "")
                {
                    Debug.Log("Wrong path.");
                    return send;
                }
                savePath = savePath.Substring(savePath.IndexOf("Assets/"));
            }
            else if (answer == 1)
            {
                return send;
            }
            else
            {
                savePath = EditorUtility.SaveFilePanelInProject("Save prefab list for this Player", "InteractorPrefabList", "asset", "Save prefab list for this Player");
                if (savePath == null || savePath == "")
                {
                    Debug.Log("Wrong path.");
                    return send;
                }
            }
            send.savePath = savePath;
            return send;
        }
    }
}
