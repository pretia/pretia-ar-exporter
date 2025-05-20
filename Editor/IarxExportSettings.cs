using UnityEditor;

namespace PretiaEditor
{
    /// <summary>
    /// Holds export-related settings
    /// </summary>
    public class IarxExportSettings
    {
        public bool EnableIOS = true;
        public bool EnableAndroid = true;
        public bool EnableWebGL = true;
        public bool UploadAssets = true;
        public bool CreatePrefabs = true;
        public bool SavePrefabOverrides = true;
        public string PrefabDirectory = "Prefabs";
        public string TempAssetBundlePath;

        public void LoadEditorPrefs()
        {
            EnableIOS = EditorPrefs.GetBool("EnableIOS", EnableIOS);
            EnableAndroid = EditorPrefs.GetBool("EnableAndroid", EnableAndroid);
            EnableWebGL = EditorPrefs.GetBool("EnableWebGL", EnableWebGL);
            UploadAssets = EditorPrefs.GetBool("UploadAssets", UploadAssets);
            CreatePrefabs = EditorPrefs.GetBool("AutoCreatePrefabs", CreatePrefabs);
            SavePrefabOverrides = EditorPrefs.GetBool("SavePrefabOverrides", SavePrefabOverrides);
            PrefabDirectory = EditorPrefs.GetString("PrefabDirectory", PrefabDirectory);
            
            TempAssetBundlePath = InitPath("IarxTempAssetBundlePath");
        }

        private string InitPath(string key)
        {
            string path = EditorPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(path))
            {
                path = FileUtil.GetUniqueTempPathInProject();
                EditorPrefs.SetString(key, path);
            }
            return path;
        }

        public void SaveEditorPrefs()
        {
            EditorPrefs.SetBool("EnableIOS", EnableIOS);
            EditorPrefs.SetBool("EnableAndroid", EnableAndroid);
            EditorPrefs.SetBool("EnableWebGL", EnableWebGL);
            EditorPrefs.SetBool("UploadAssets", UploadAssets);
            EditorPrefs.SetBool("AutoCreatePrefabs", CreatePrefabs);
            EditorPrefs.SetBool("SavePrefabOverrides", SavePrefabOverrides);
            EditorPrefs.SetString("PrefabDirectory", PrefabDirectory);
        }
    }
}