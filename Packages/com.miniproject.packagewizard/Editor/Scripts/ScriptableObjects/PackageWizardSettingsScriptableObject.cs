using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace MiniProject.PackageWizard.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Package Wizard Settings", menuName = "Package Wizard/Settings", order = 1)]
    public class PackageWizardSettingsScriptableObject : ScriptableObject
    {
        //Additional Types
        //================================================================================================================//

        #region Additional Types

        [Serializable]
        public struct ProjectInfo
        {
            //Display name for Unity Lists
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
            /// <summary>
            /// Convert DependencyData to PackageData.DependencyData. This is to avoid the Custom Unity Editor for the
            /// PackageData.DependencyData object.
            /// </summary>
            /// <returns></returns>
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

        #endregion //Additional Types

        //Properties
        //================================================================================================================//

        #region Properties

        //Project Directories Properties
        //----------------------------------------------------------//
        public IReadOnlyList<ProjectInfo> ProjectDirectories => projectDirectories;
        [SerializeField]
        private List<ProjectInfo> projectDirectories;
        
        //Unity Versions List
        //----------------------------------------------------------//
        [SerializeField]
        public List<string> unityVersions;

        //Dependency Data
        //----------------------------------------------------------//
        public IReadOnlyDictionary<string, PackageData.DependencyData[]> Dependencies => GenerateDependencyDataGroup();

        [FormerlySerializedAs("depedencyDataGroups")] 
        [SerializeField, NonReorderable]
        private DependencyDataGroup[] dependencyDataGroups;

        #endregion //Properties

        //Unity Functions
        //================================================================================================================//

        private void OnValidate()
        {
            //Marks the object dirty to ensure that the data is able to be saved
            EditorUtility.SetDirty(this);
        }

        //Project Directories
        //================================================================================================================//

        //FIXME This will likely need to exist somewhere else, since this will now be pushed. Might need to store in Editor Preferences.
        #region Project Directories

        /// <summary>
        /// Groups projects by the Unity version used.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, List<ProjectInfo>> GetGroupedProjectDirectories()
        {
            var projects = projectDirectories;

            var outDictionary = new Dictionary<string, List<ProjectInfo>>();

            foreach (var project in projects)
            {
                if (outDictionary.TryGetValue(project.unityVersion, out var list) == false)
                {
                    list = new List<ProjectInfo>();
                    outDictionary.Add(project.unityVersion, list);
                }
                    
                list.Add(project);
            }

            return outDictionary;
        }
        
        //TODO This will need to be a button
        /// <summary>
        /// Gets all projects within a directory (Recursive) which will be used to display as a selection option
        /// when creating packages
        /// </summary>
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

        /// <summary>
        /// Finds the unity version for a particular project.
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <param name="version"></param>
        /// <returns></returns>
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
        


        #endregion //Project Directories

        //Dependency Data
        //================================================================================================================//

        #region Dependency Data
        
        /// <summary>
        /// Gets a grouped list of the dependencies stored on this object
        /// </summary>
        /// <returns></returns>
        private IReadOnlyDictionary<string, PackageData.DependencyData[]> GenerateDependencyDataGroup()
        {
            var dependencies = new Dictionary<string, PackageData.DependencyData[]>();
            foreach (var dependencyDataGroup in dependencyDataGroups)
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