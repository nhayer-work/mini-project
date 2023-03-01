using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Serialization;

namespace MiniProject.PackageWizard.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Package Wizard Settings", menuName = "Mini Project/Package Wizard/Settings", order = 1)]
    public class PackageWizardSettingsScriptableObject : ScriptableObject
    {
        [Serializable]
        public struct ProjectInfo
        {
            public string name;
            
            public string directory;
            public string unityVersion;
            public BuildTargetGroup buildTargetGroup;

            public ProjectInfo(string directory, string unityVersion, BuildTargetGroup buildTargetGroup)
            {
                this.directory = directory;
                this.unityVersion = unityVersion;
                this.buildTargetGroup = buildTargetGroup;

                name = $"{unityVersion} - {buildTargetGroup}";
            }
        }

        //================================================================================================================//

        private void OnEnable()
        {
            Debug.Log("OnEnable");
        }

        private void OnValidate()
        {
            Debug.Log("OnValidate");
        }

        //Unity Hub Installs Directory
        //================================================================================================================//

        [Serializable]
        public class UnityEditorData
        {
            public string version;
            public string[] installedModules;
        }
        
        [SerializeField]
        private string installsDirectory = "C:/Program Files/Unity/Hub/Editor";
        [SerializeField]
        private List<UnityEditorData> installedEditors;

        [ContextMenu("Get Hub Installs Directory")]
        private void GetInstallsDirectory()
        {
            var path = EditorUtility.OpenFolderPanel("Select Unity Hub Installs Directory", installsDirectory, "");
                
            if(string.IsNullOrEmpty(path))
                return;

            var installsDirectoryInfo = new DirectoryInfo(path);
            foreach (var directory in installsDirectoryInfo.EnumerateDirectories())
            {
                /*var file = directory.EnumerateFiles("UnityEditor.dll", SearchOption.AllDirectories)
                    .First();*/
                
                
                installedEditors.Add(new UnityEditorData
                {
                    version = directory.Name,
                    installedModules = GetModules(directory)
                });
            }

            installedEditors = installedEditors.OrderByDescending(x => x.version).ToList();

            installsDirectory = installsDirectoryInfo.FullName;
        }
        
        //Directories
        //================================================================================================================//

        [SerializeField]
        private List<ProjectInfo> projectDirectories;

        [ContextMenu("Add New Directory")]
        private void AddNewDirectory()
        {
            var path = EditorUtility.OpenFolderPanel("Select Project Directory", Application.dataPath, "");
                
            if(string.IsNullOrEmpty(path))
                return;

            if (projectDirectories == null)
                projectDirectories = new List<ProjectInfo>();

            var parentDirectory = new DirectoryInfo(path);

            foreach (var directory in parentDirectory.EnumerateDirectories("ProjectSettings", SearchOption.AllDirectories))
            {
                var directoryName = directory.Parent.FullName;
                
                if (GetUnityVersion(directory, out var version) == false)
                {
                    Debug.LogError($"Unable to find project version in {directoryName}");
                    continue;
                }

                var installedModules = installedEditors.FirstOrDefault(x => x.version == version)?.installedModules;

                BuildTargetGroup buildTargetGroup = default;
                if (installedModules == null)
                {
                    Debug.LogError($"Unable to find Editor version {version}");
                }
                else if (GetBuildTarget(directory, installedModules, out buildTargetGroup) == false)
                {
                    Debug.LogError($"Unable to find Build Target in {directoryName}");
                }
                
                projectDirectories.Add(new ProjectInfo
                (
                    directoryName,
                    version,
                    buildTargetGroup
                ));
            }

            UpdateAsset();
        }

        private static bool GetUnityVersion(in DirectoryInfo directoryInfo, out string version)
        {
            const string EDITOR_VERSION = "m_EditorVersion";
            const string PROJECT_VERSION_FILE = "ProjectVersion.txt";
            //----------------------------------------------------------//

            version = default;
            
            var files = directoryInfo.GetFiles(PROJECT_VERSION_FILE, SearchOption.TopDirectoryOnly);

            if (files == null || files.Length == 0)
                //throw new FileNotFoundException($"Unable to find {PROJECT_VERSION_FILE} under:\n{directoryInfo.FullName}");
                return false;
            
            using (var stream = files[0].OpenText())
            {
                while (stream.EndOfStream == false)
                {
                    var line = stream.ReadLine();

                    if(string.IsNullOrWhiteSpace(line))
                        continue;
                    
                    if (line.Contains(EDITOR_VERSION) == false)
                        continue;

                    version = line.Replace($"{EDITOR_VERSION}: ", "");
                    return true;
                }
                //throw new MissingComponentException($"Unable to find {EDITOR_VERSION} within {files[0].FullName}");
                return false;
            }
        }
        
        private static bool GetBuildTarget(in DirectoryInfo directoryInfo, in string[] platforms, out BuildTargetGroup buildTargetGroup)
        {
            const string SELECTED_PLATFORM = "selectedPlatform";
            const string PROJECT_SETTINGS_FILE = "ProjectSettings.asset";
            //----------------------------------------------------------//
            buildTargetGroup = default;
            
            var files = directoryInfo.GetFiles(PROJECT_SETTINGS_FILE, SearchOption.TopDirectoryOnly);

            if (files == null || files.Length == 0)
                //throw new FileNotFoundException($"Unable to find {PROJECT_SETTINGS_FILE} under:\n{directoryInfo.FullName}");
                return false;
            
            using (var stream = files[0].OpenText())
            {
                while (stream.EndOfStream == false)
                {
                    var line = stream.ReadLine();

                    if(string.IsNullOrWhiteSpace(line))
                        continue;
                    
                    if (line.Contains(SELECTED_PLATFORM) == false)
                        continue;

                    if (int.TryParse(line.Replace($"{SELECTED_PLATFORM}: ", ""), out var buildTargetInt) == false)
                        //throw new Exception();
                        return false;
                    
                    //RuntimePlatform
                    buildTargetGroup = NamedGroupToTargetGroup(platforms[buildTargetInt]);
                    
                    return true;
                }
                //throw new MissingComponentException($"Unable to find {SELECTED_PLATFORM} within {files[0].FullName}");
                return false;
            }
        }

        //Test Target
        //================================================================================================================//
        /*public struct PlatformData
        {
            public BuildTargetGroup BuildTargetGroup;
            public BuildTarget BuildTarget;
            public NamedBuildTarget NamedBuildTarget;
            
        }
        private bool IsPlatformSupportLoaded(BuildTarget buildTarget)
        {
            //http://answers.unity.com/answers/1324228/view.html
            var moduleManager = System.Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
            var isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTarget", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
     
            return (bool)isPlatformSupportLoaded.Invoke(null,new object[] {(string)getTargetStringFromBuildTarget.Invoke(null, new object[] {buildTarget})});
        }


        [ContextMenu("Testing")]
        private PlatformData[] GetPlatformModules()
        {
            //Get List<BuildPlatform>
            //----------------------------------------------------------//

            //http://answers.unity.com/answers/1324228/view.html
            var buildPlatformsType = Type.GetType("UnityEditor.Build.BuildPlatforms,UnityEditor.dll");
            //var isPlatformSupportLoaded = moduleManager.GetMethod("GetValidPlatforms", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | BindingFlags.Default | BindingFlags.Public | BindingFlags.Instance);
            var getValidPlatformsMethod = buildPlatformsType.GetMethods()
                .Where(x => x.Name == "GetValidPlatforms")
                .FirstOrDefault(x => x.GetParameters().Length == 0);
            
            var instanceField = buildPlatformsType.GetField("s_Instance", BindingFlags.Default | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var instance = instanceField.GetValue(null);
            
            var test = getValidPlatformsMethod?.Invoke(instance, null);
            IList buildPlatforms = (IList)test;
            
            var count = buildPlatforms.Count;
            var outPlatformData = new PlatformData[count];
            
            //Get Data from BuildPlatform
            //----------------------------------------------------------//
            var buildPlatformType = Type.GetType("UnityEditor.Build.BuildPlatform,UnityEditor.dll");
            var defaultTargetField = buildPlatformType?.GetField("defaultTarget", BindingFlags.Default | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty);
            var namedBuildTargetField = buildPlatformType?.GetField("namedBuildTarget", BindingFlags.Default | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty);

            for (int i = 0; i < count; i++)
            {
                var buildTarget = (BuildTarget)defaultTargetField.GetValue(buildPlatforms[i]);
                var namedBuildTarget = (NamedBuildTarget)namedBuildTargetField.GetValue(buildPlatforms[i]);
                var buildTargetGroup = namedBuildTarget.ToBuildTargetGroup();

                outPlatformData[i] = new PlatformData
                {
                    BuildTargetGroup = buildTargetGroup,
                    BuildTarget = buildTarget,
                    NamedBuildTarget = namedBuildTarget,
                };
            }

            //----------------------------------------------------------//

            return outPlatformData;
        }
        
        private PlatformData[] GetPlatformModules(in FileInfo unityEditorAssembly)
        {
            var assembly = Assembly.LoadFrom(unityEditorAssembly.FullName);
            //Get List<BuildPlatform>
            //----------------------------------------------------------//

            //http://answers.unity.com/answers/1324228/view.html
            var buildPlatformsType = assembly.GetType("UnityEditor.Build.BuildPlatforms");
            //var isPlatformSupportLoaded = moduleManager.GetMethod("GetValidPlatforms", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | BindingFlags.Default | BindingFlags.Public | BindingFlags.Instance);
            var getValidPlatformsMethod = buildPlatformsType.GetMethods()
                .Where(x => x.Name == "GetValidPlatforms")
                .FirstOrDefault(x => x.GetParameters().Length == 0);
            
            var instanceField = buildPlatformsType.GetField("s_Instance", BindingFlags.Default | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var instance = instanceField.GetValue(null);
            
            var test = getValidPlatformsMethod?.Invoke(instance, null);
            IList buildPlatforms = (IList)test;
            
            var count = buildPlatforms.Count;
            var outPlatformData = new PlatformData[count];
            
            //Get Data from BuildPlatform
            //----------------------------------------------------------//
            var buildPlatformType = Type.GetType("UnityEditor.Build.BuildPlatform,UnityEditor.dll");
            var defaultTargetField = buildPlatformType?.GetField("defaultTarget", BindingFlags.Default | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty);
            var namedBuildTargetField = buildPlatformType?.GetField("namedBuildTarget", BindingFlags.Default | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty);

            for (int i = 0; i < count; i++)
            {
                var buildTarget = (BuildTarget)defaultTargetField.GetValue(buildPlatforms[i]);
                var namedBuildTarget = (NamedBuildTarget)namedBuildTargetField.GetValue(buildPlatforms[i]);
                var buildTargetGroup = namedBuildTarget.ToBuildTargetGroup();

                outPlatformData[i] = new PlatformData
                {
                    BuildTargetGroup = buildTargetGroup,
                    BuildTarget = buildTarget,
                    NamedBuildTarget = namedBuildTarget,
                };
            }

            //----------------------------------------------------------//

            return outPlatformData;
        }*/

        private static string[] GetModules(in DirectoryInfo directoryInfo)
        {
            string[] validNames = new string[]
            {
                "Standalone",
                "Server",
                "iOS",
                "Android",
                "WebGL",
                "Windows Store Apps",
                "PS4",
                "XboxOne",
                "tvOS",
                "Nintendo Switch",
                "Stadia",
                "CloudRendering",
                "LinuxHeadlessSimulation",
                "Lumin",
                "GameCoreScarlett",
                "GameCoreXboxOne",
                "PS5",
                "EmbeddedLinux",
            };
            var modulesPath = Path.Combine(directoryInfo.FullName, "Editor/Data/PlaybackEngines");
            var modulesDirectory = new DirectoryInfo(modulesPath);
            var directories = modulesDirectory.GetDirectories().Select(x => x.Name.ToLower()).ToList();

            var outList = new List<string>();
            foreach (var platformName in validNames)
            {
                var name = platformName.ToLower();
                if(directories.Any(x => x.Contains(name)) == false)
                    continue;
                
                outList.Add(platformName);
            }

            outList.Insert(1, "Server");
            return outList.ToArray();
        }

        private static BuildTargetGroup NamedGroupToTargetGroup(in string namedTargetGroup)
        {
            switch (namedTargetGroup)
            {
                case "Standalone":
                case "Windows Store Apps":
                case "Server":
                    return BuildTargetGroup.Standalone;
                case "iOS":
                    return BuildTargetGroup.iOS;
                case "Android":
                    return BuildTargetGroup.Android;
                case "WebGL":
                    return BuildTargetGroup.WebGL;
                case "PS4":
                    return BuildTargetGroup.PS4;
                case "XboxOne":
                    return BuildTargetGroup.XboxOne;
                case "tvOS":
                    return BuildTargetGroup.tvOS;
                case "Nintendo Switch":
                    return BuildTargetGroup.Switch;
                case "Stadia":
                    return BuildTargetGroup.Stadia;
                case "CloudRendering":
                    return BuildTargetGroup.LinuxHeadlessSimulation;
                case "LinuxHeadlessSimulation":
                    return BuildTargetGroup.LinuxHeadlessSimulation;
                case "Lumin":
                    return BuildTargetGroup.Lumin;
                case "GameCoreScarlett":
                    return BuildTargetGroup.GameCoreXboxSeries;
                case "GameCoreXboxOne":
                    return BuildTargetGroup.GameCoreXboxOne;
                case "PS5":
                    return BuildTargetGroup.PS5;
                case "EmbeddedLinux":
                    return BuildTargetGroup.EmbeddedLinux;
            }

            throw new Exception();
        }

        //Dependency Data
        //================================================================================================================//

        #region Dependency Data

        [Serializable]
        private class DepedencyDataGroup
        {
            public string name;
            [NonReorderable]
            public DependencyData[] dependencies;
        }
        
        [Serializable]
        private struct DependencyData
        {
            public string DisplayName;
            public string Domain;
            public string Version;
            //for com.unity packages, no need to include the source, the version should
            public string Source;

            public DependencyData(PackageData.DependencyData dependencyData)
            {
                DisplayName = dependencyData.DisplayName;
                Domain = dependencyData.Domain;
                Version = dependencyData.Version;
                Source = dependencyData.Source;
            }
            public PackageData.DependencyData GetPackageDependencyData()
            {
                return new PackageData.DependencyData
                {
                    DisplayName = DisplayName,
                    Domain = Domain,
                    Version = Version,
                    Source = Source
                };
            }
        }

        //================================================================================================================//

        public IReadOnlyDictionary<string, PackageData.DependencyData[]> Dependencies
        {
            get
            {
                if (_dependencies == null)
                    GenerateDependencyData();

                return _dependencies;
            }
        }

        private Dictionary<string, PackageData.DependencyData[]> _dependencies;

        [SerializeField, NonReorderable]
        private DepedencyDataGroup[] depedencyDataGroups;
        
        //================================================================================================================//

        private void GenerateDependencyData()
        {
            _dependencies = new Dictionary<string, PackageData.DependencyData[]>();
            foreach (var depedencyDataGroup in depedencyDataGroups)
            {
                _dependencies.Add(depedencyDataGroup.name,
                    depedencyDataGroup.dependencies.Select(x => x.GetPackageDependencyData()).ToArray());
            }
        }

        #endregion //Dependency Data
        
        //Asset Update
        //================================================================================================================//

        private void UpdateAsset()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        //================================================================================================================//

    }
}