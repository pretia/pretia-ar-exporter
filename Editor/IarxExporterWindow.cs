using System.Linq;
using Pretia;
using UnityEditor;
using UnityEngine;

namespace PretiaEditor
{
    /// <summary>
    /// Pretia Asset Exporter Window UI and Pretia Menu options
    /// </summary>
    public class IarxExporterWindow : EditorWindow
    {
        private static IarxExportSettings Settings = new IarxExportSettings();

        private static int _selectedTeamIndex = 0;
        private static string[] _teamNames = new string[] { };
        private static Team[] _teamList = new Team[] { };

        private Color _outputMessageColor = Color.white;
        private string _outputMessage = "";
        private FontStyle _defaultStyle;
        private Color _defaultColor;
        private bool _settingChanged = false;

        private Texture _icon = null;
        private Texture _bgImg = null;
        private Texture _docIcon = null;
        
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Settings.LoadEditorPrefs();
        }

        [MenuItem("Pretia/Settings")]
        private static void ShowSettingsWindow()
        {
            var window = EditorWindow.GetWindow(
                typeof(IarxExporterWindow),
                utility: false,
                "Pretia Asset Exporter"
            );
            window.Show();
        }

        [MenuItem("Pretia/Export", true)]
        public static bool ValidateExportIarx()
        {
            return !string.IsNullOrEmpty(Authentication.Token);
        }

        [MenuItem("Pretia/Export")]
        public static void ExportIarx()
        {
            var window = EditorWindow.GetWindow(
                typeof(IarxExporterWindow),
                utility: false,
                "Pretia Asset Exporter") as IarxExporterWindow;
            
            IarxExporter.ExportIarx(Settings, window == null ? null : window.SetOutputMessage);
        }

        private void LoadResources()
        {
            if (_bgImg == null)
                _bgImg = EditorGUIUtility.Load("Packages/com.pretia.exporter/Img/banner_bg.png") as Texture;
            if (_icon == null)
                _icon = EditorGUIUtility.Load("Packages/com.pretia.exporter/Img/banner.png") as Texture;
            if (_docIcon == null)
                _docIcon = EditorGUIUtility.Load("Packages/com.pretia.exporter/Img/btn_guidelines.png") as Texture;
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(100.0f);
            // BG
            GUI.DrawTextureWithTexCoords(new Rect(0, 0, position.width, 100), _bgImg, 
                new Rect(0, 0, (float)position.width / 30.8f, 1.0f));
            // Icon
            GUI.DrawTexture(new Rect((position.width / 2.0f) - 128.0f, 18, 256, 64), _icon, ScaleMode.StretchToFill, true);
            
            // Docs
            Color oldBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.3f, 0.7f, 1.0f);
            if (GUILayout.Button("Documentation", GUILayout.Height(38)))
            //if (GUI.Button(new Rect(position.width - 60, 105, 40, 40), _docIcon))
            {
                Application.OpenURL(PretiaDocs.UnityExporterUrl());
            }
            EditorGUILayout.Space();
            GUI.backgroundColor = oldBackgroundColor;
        }

        private void OnGUI()
        {
            _settingChanged = false;

            _defaultStyle = EditorStyles.label.fontStyle;
            _defaultColor = GUI.color;
            Color oldBackgroundColor = GUI.backgroundColor;

            LoadResources();
            EditorGUILayout.BeginVertical("box");
            
            // HEADER
            DrawHeader();
            
            // PLATFORMS
            EditorGUILayout.LabelField(new GUIContent("Platforms", "Which platforms to build asset bundles for"), EditorStyles.boldLabel);
            EditorGUILayout.Space();

            Settings.EnableIOS = Toggle(new GUIContent(" iOS", "Export bundles for iOS platform"), Settings.EnableIOS);
            Settings.EnableAndroid = Toggle(new GUIContent(" Android", "Export bundles for Android platform"), Settings.EnableAndroid);
            Settings.EnableWebGL = Toggle(new GUIContent(" WebGL", "Export bundles for WebGL platform (Used by the AR Creator)"), Settings.EnableWebGL);

            // SETTINGS
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Export Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            Settings.CreatePrefabs = Toggle(new GUIContent(
                    "Create Prefabs", 
                    "If enabled, then Prefabs will be automatically created for each root object in the scene"), 
                Settings.CreatePrefabs);

            if (Settings.CreatePrefabs)
            {
                string prefabDirectory = EditorGUILayout.TextField(new GUIContent(
                        "  Prefab Directory", 
                        "Directory within Assets to which prefabs should be saved when they are automatically created"), 
                    Settings.PrefabDirectory);
                
                if (prefabDirectory != Settings.PrefabDirectory)
                {
                    Settings.PrefabDirectory = prefabDirectory;
                    _settingChanged = true;
                }

                EditorGUILayout.Space();
            }

            Settings.SavePrefabOverrides = Toggle(new GUIContent(
                    "Save Prefab Overrides", 
                    "If enabled then any changes to prefabs will be automatically applied when exporting"), 
                Settings.SavePrefabOverrides);

            EditorGUILayout.Space();

            if (string.IsNullOrEmpty(Authentication.Token))
            {
                GUI.backgroundColor = new Color(0.3f, 0.7f, 1.0f);
                if (GUILayout.Button("Log in", GUILayout.Height(38)))
                {
                    Authentication.Login();
                }

                GUI.backgroundColor = oldBackgroundColor;

                if (!string.IsNullOrEmpty(Authentication.Status))
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(Authentication.Status);
                    GUI.color = _defaultColor;
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Upload Settings", EditorStyles.boldLabel);

                Settings.UploadAssets = true;
                
                // For now we assume users always want to automatically upload assets
                // Toggle(new GUIContent("Upload Assets", "If enabled, assets will be automatically uploaded, otherwise they will just be exported locally to allow for manual upload"), Settings.UploadAssets);
                
                if (Settings.UploadAssets)
                {
                    InitializeTeams();
                    int index = EditorGUILayout.Popup(new GUIContent("Team", "Team's asset library for which assets should be uploaded to"), 
                        _selectedTeamIndex, _teamNames);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if ((string.IsNullOrEmpty(IarxExporter.CurrentTeamID) || index != _selectedTeamIndex) &&
                            index >= 0 && index < _teamList.Length)
                        {
                            _selectedTeamIndex = index;
                            IarxExporter.CurrentTeamID = _teamList[_selectedTeamIndex].id;
                            EditorPrefs.SetInt("TEAM_INDEX", _selectedTeamIndex);
                        }
                    }
                }

                EditorGUILayout.Space();

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Log out", GUILayout.Height(38)))
                {
                    Authentication.Logout();
                }
                GUI.backgroundColor = oldBackgroundColor;
                
                EditorGUILayout.Space();
                if (GUILayout.Button("Export", GUILayout.Height(38)))
                {
                    IarxExporter.ExportIarx(Settings, SetOutputMessage);
                    _settingChanged = true;
                }

            }

            EditorGUILayout.Space();

            if (!string.IsNullOrEmpty(_outputMessage))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.color = _outputMessageColor;
                GUILayout.Label(_outputMessage);
                GUI.color = _defaultColor;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            // Note: Files should be zipped if manually importing
            /*if (GUILayout.Button("Open Output Folder"))
            {
                DirectoryOpener.OpenInFileExplorer(Settings.TempSubmissionPath);
            }
            EditorGUILayout.Space();*/

            EditorGUILayout.EndVertical();

            if (_settingChanged)
            {
                Settings.SaveEditorPrefs();
            }
        }

        private bool Toggle(GUIContent content, bool value)
        {
            bool outValue = EditorGUILayout.Toggle(content, value);

            if (outValue != value)
                _settingChanged = true;

            return outValue;
        }

        private void SetOutputMessage(string message, Color color)
        {
            _outputMessageColor = color;
            _outputMessage = message;
        }

        private void InitializeTeams()
        {
            if (_teamList.Length == 0 && !string.IsNullOrEmpty(Authentication.Token))
            {
                CMC.Get("teams", ((bool noError, string response) result) =>
                {
                    try
                    {
                        if (result.noError)
                        {
                            TeamList teamList = JsonUtility.FromJson<TeamList>($"{{\"teams\": {result.response}}}");
                            _teamList = teamList.teams;
                            _teamNames = _teamList.Select(team => team.name).ToArray();
                            _selectedTeamIndex = EditorPrefs.GetInt("TEAM_INDEX", 0);
                        }
                        else
                        {
                            Debug.LogError("Error when requesting teams: " + result.response);
                            Authentication.Logout(false);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Exception while requesting teams: " + e.ToString());
                    }
                });
            }
        }
    }
}