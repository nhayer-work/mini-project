using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MiniProject.PackageWizard.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Package Wizard Settings", menuName = "Mini Project/Package Wizard/Settings", order = 1)]
    public class PackageWizardSettingsScriptableObject : ScriptableObject
    {
        [Serializable]
        public struct ProjectInfo
        {
            [SerializeField]
            private string name;

            public string projectName;
            public string directory;
            public string unityVersion;

            public ProjectInfo(DirectoryInfo directory, string unityVersion)
            {
                this.directory = directory.FullName;
                this.unityVersion = unityVersion;

                projectName = directory.Name;
                name = $"{unityVersion} - {directory.Name}";
            }
        }

        //================================================================================================================//

        private void OnValidate()
        {
            Debug.Log("OnValidate");
            EditorUtility.SetDirty(this);
        }

        //Project Directories
        //================================================================================================================//
        public IReadOnlyList<ProjectInfo> ProjectDirectories => projectDirectories;
        [SerializeField]
        private List<ProjectInfo> projectDirectories;

        [ContextMenu("Add New Project Directory")]
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
                var directoryParent = directory.Parent;
                var directoryName = directoryParent.FullName;
                
                if (GetUnityVersion(directory, out var version) == false)
                {
                    Debug.LogError($"Unable to find project version in {directoryName}");
                    continue;
                }

                projectDirectories.Add(new ProjectInfo
                (
                    directoryParent,
                    version
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

        //Unity Major Versions
        //================================================================================================================//

        [SerializeField]
        public List<string> unityVersions;

        //Dependency Data
        //================================================================================================================//

        #region Dependency Data

        [Serializable]
        private class DependencyDataGroup
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

        public IReadOnlyDictionary<string, PackageData.DependencyData[]> Dependencies => GenerateDependencyData();

        [SerializeField, NonReorderable]
        private DependencyDataGroup[] depedencyDataGroups;
        
        //================================================================================================================//

        private IReadOnlyDictionary<string, PackageData.DependencyData[]> GenerateDependencyData()
        {
            var dependencies = new Dictionary<string, PackageData.DependencyData[]>();
            foreach (var dependencyDataGroup in depedencyDataGroups)
            {
                dependencies.Add(dependencyDataGroup.name,
                    dependencyDataGroup.dependencies.Select(x => x.GetPackageDependencyData()).ToArray());
            }

            return dependencies;
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