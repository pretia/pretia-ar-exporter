using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

namespace PretiaEditor
{
    /// <summary>
    /// Handles prefab creation, exporting and uploading of assets via the CMC
    /// </summary>
    public class IarxExporter : EditorWindow
    {
        public static string CurrentTeamID = "";
        
        private static int _assetUploadTotal = 0;
        private static int _assetUploadProgress = 0;

        /// <summary>
        /// Export asset bundles for all assets in the scene, using given settings
        /// </summary>
        /// <param name="settings">IarxExportSettings to use when exporting</param>
        /// <param name="messageCallback">Optional callback for progress and result messages</param>
        public static void ExportIarx(
            IarxExportSettings settings, 
            System.Action<string, Color> messageCallback = null)
        {
            messageCallback?.Invoke("Exporting scene...", Color.white);
            
            var assetBundleNames = new HashSet<string>();
            var currentScene = SceneManager.GetActiveScene();
            var rootGameObjects = currentScene.GetRootGameObjects();

            // Iterate over all game objects in the active scene
            foreach (var gameObject in rootGameObjects)
            {
                if (!gameObject.activeSelf || 
                    gameObject.name == "Main Camera" ||
                    gameObject.name == "Event System" ||
                    gameObject.name.StartsWith("__Simulation")
                    ) continue;
                
                // Check if the game object is a prefab instance
                PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(gameObject);

                if (settings.CreatePrefabs && prefabType == PrefabAssetType.NotAPrefab)
                {
                    if (!Directory.Exists($"Assets/{settings.PrefabDirectory}"))
                        AssetDatabase.CreateFolder("Assets", settings.PrefabDirectory);
                    
                    PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, 
                        $"Assets/{settings.PrefabDirectory}/{gameObject.name}.prefab", 
                        InteractionMode.UserAction, 
                        out bool saved);
                    
                    if (saved)
                        prefabType = PrefabAssetType.Regular;
                    else
                        Debug.LogError("Error saving prefab");
                }
                
                if (prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant)
                {
                    PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(gameObject);

                    foreach (PropertyModification mod in modifications)
                    {
                        if (!PrefabUtility.IsDefaultOverride(mod))
                        {
                            if (settings.SavePrefabOverrides)
                            {
                                Debug.Log($"Saving prefab overrides for {gameObject.name}");
                                PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.UserAction);
                                //if (!prefabUpdated)
                                //    Debug.LogWarning($"WARNING: {gameObject.name} prefab has overrides which were not saved");
                            }
                            else
                                Debug.LogWarning($"WARNING: {gameObject.name} has prefab overrides");
                            break;
                        }
                    }
                    
                    // Get the path of the prefab asset
                    GameObject prefabObj = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
                    string prefabPath = AssetDatabase.GetAssetPath(prefabObj);
                    AssetImporter assetImporter = AssetImporter.GetAtPath(prefabPath);
                    
                    if (assetImporter.assetBundleName != prefabObj.name)
                        assetImporter.SetAssetBundleNameAndVariant(prefabObj.name, "");
                    
                    if (prefabObj.name != gameObject.name)
                        Debug.LogWarning($"WARNING: GameObject name ({gameObject.name}) does not match prefab name ({prefabObj.name}).  \nAsset will be exported using the prefab name.");
                    
                    if (!string.IsNullOrEmpty(assetImporter.assetBundleName))
                    {
                        assetBundleNames.Add(assetImporter.assetBundleName);
                    }
                }
            }

            if (settings.UploadAssets)
            {
                messageCallback?.Invoke($"Uploading Assets...", Color.white);
                _assetUploadTotal = 0;
                _assetUploadProgress = 0;
            }

            if (assetBundleNames.Count > 0)
            {
                AssetBundleBuild[] buildMap = assetBundleNames.Select(name => new AssetBundleBuild
                {
                    assetBundleName = name,
                    assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(name)
                }).ToArray();

                //BuildForPlatform(BuildTarget.StandaloneWindows, settings.TempAssetBundlePath, buildMap, assetBundleNames, settings, messageCallback);
                
                // Build and copy asset bundles for each platform
                if (settings.EnableWebGL)
                {
                    BuildForPlatform(BuildTarget.WebGL, settings.TempAssetBundlePath, buildMap, assetBundleNames, settings, messageCallback);
                }
                if (settings.EnableIOS)
                {
                    BuildForPlatform(BuildTarget.iOS, settings.TempAssetBundlePath, buildMap, assetBundleNames, settings, messageCallback);
                }
                if (settings.EnableAndroid)
                {
                    BuildForPlatform(BuildTarget.Android, settings.TempAssetBundlePath, buildMap, assetBundleNames, settings, messageCallback);
                }
            }
            
            if (_assetUploadTotal == 0)
                messageCallback?.Invoke("No objects to export.", Color.red);
            else if (!settings.UploadAssets)
                messageCallback?.Invoke("Export complete.", Color.green);
        }

        private static void BuildForPlatform(BuildTarget buildTarget, 
            string directory, 
            AssetBundleBuild[] buildMap,
            HashSet<string> assetBundleNames,
            IarxExportSettings settings, 
            System.Action<string, Color> messageCallback = null)
        {
            string buildPath = Path.Combine(directory, buildTarget.ToString());
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }

            // Switch the active build target
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(buildTarget), buildTarget);

            // Build the Asset Bundles for the specified platform
            BuildPipeline.BuildAssetBundles(buildPath, buildMap, BuildAssetBundleOptions.None, buildTarget);

            if (settings.UploadAssets)
            {
                string platform = "universal";
                switch (buildTarget)
                {
                    case BuildTarget.Android:
                        platform = "android";
                        break;
                    case BuildTarget.WebGL:
                        platform = "web_gl";
                        break;
                    case BuildTarget.iOS:
                        platform = "ios";
                        break;
                    case BuildTarget.StandaloneWindows:
                    case BuildTarget.StandaloneWindows64:
                        platform = "windows";
                        break;
                }
                
                string[] files = Directory.GetFiles(buildPath);
                foreach (var file in files)
                {
                    string filename = Path.GetFileName(file);
                    if (assetBundleNames.Contains(filename))
                    {
                        string sourcePath = Path.Combine(buildPath, filename);
                        UploadAsset($"{filename}.bundle", sourcePath, platform, messageCallback);
                    }
                }
            }
        }

        /// <summary>
        /// Upload given asset to the CMC
        /// </summary>
        /// <param name="assetName">File name / local CMC path</param>
        /// <param name="path">File path of the asset to upload</param>
        /// <param name="platform">Platform which this asset bundle was created for</param>
        /// <param name="messageCallback">Callback to report progress</param>
        private static void UploadAsset(string assetName, 
            string path, 
            string platform, 
            System.Action<string, Color> messageCallback = null)
        {
            byte[] assetData = File.ReadAllBytes(path);
            
            if (assetData == null)
                throw new System.ArgumentNullException(nameof(assetData));

            if (assetData.Length == 0)
                throw new System.ArgumentException("Can't save an empty asset");

            string teamId = CurrentTeamID;
            string url = $"teams/{teamId}/assets?platform={platform}";
            
            Debug.Log($"Uploading asset: <{assetName}> for {platform} platform");
            _assetUploadTotal++;
            
            CMC.PostFileData(url, assetName, assetData,
                ((bool success, string response) result) =>
                {
                    _assetUploadProgress++;
                    if (!result.success)
                        messageCallback?.Invoke("Error uploading asset", Color.red);
                    else
                    {
                        if (_assetUploadProgress == _assetUploadTotal)
                            messageCallback?.Invoke($"Uploaded Assets ( {_assetUploadProgress} / {_assetUploadTotal} )", Color.green);
                        else
                            messageCallback?.Invoke($"Uploading Assets ( {_assetUploadProgress} / {_assetUploadTotal} )", Color.white);
                    }
                });
        }
    }
}