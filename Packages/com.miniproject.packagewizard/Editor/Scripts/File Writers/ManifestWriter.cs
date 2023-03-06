//#define DEBUG
#undef DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniProject.Core.Editor.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiniProject.PackageWizard.FileWriters
{
    public class ManifestWriter : FileWriterBase
    {
        private const string DEPENDENCIES_KEY = "dependencies";
        private const string PROJECT_SETTINGS = "ProjectSettings";

        private const string MANIFEST_FILENAME = "manifest.json";

        //ManifestWriter Functions
        //================================================================================================================//

        /// <summary>
        /// This function will include the package, with a generated path in the respective platform & unity versions,
        /// to manifest.json & packages-lock.json. If the entry already exists, then the it will only overwrite the
        /// directory information.
        /// </summary>
        /// <param name="targetProjects"></param>
        /// <param name="packageDependencies"></param>
        /// <param name="customDependencies"></param>
        /// <param name="packageDirectory"></param>
        public void UpdateManifestFiles(
            in DirectoryInfo packageDirectory, 
            in DirectoryInfo[] targetProjects,
            in IEnumerable<PackageData.DependencyData> packageDependencies,
            in IEnumerable<PackageData.DependencyData> customDependencies)
        {
            //----------------------------------------------------------//

            var packageManifestDependencies = GetAsManifestDependencies(packageDependencies, customDependencies);
            for (var i = 0; i < targetProjects.Length; i++)
            {
                //We only need to find the Manifest file, as the package-lock appears to update itself
                var files = targetProjects[i].GetFiles(MANIFEST_FILENAME, SearchOption.AllDirectories);
                var projectSettingsDirectory =
                    new DirectoryInfo(Path.Combine(targetProjects[i].FullName, PROJECT_SETTINGS));

                for (var ii = 0; ii < files.Length; ii++)
                {
                    if(files[ii].Name.Equals(MANIFEST_FILENAME) == false)
                        continue;

                    //TODO Determine if this should be a toggleable setting
                    //Gets relative path from targetProjects[i]/ProjectSettings/ to package save location
                    var relativePath = projectSettingsDirectory.GetRelativePathTo(packageDirectory);
                    //Convert the path into a Package Manifest friendly format
                    var relativePackagePath = $"file:{relativePath.Replace("\\","/")}";
                    
                    //Need to create new list here, since the relative path needs to be different per project 
                    var tempList = new List<KeyValuePair<string, string>>(packageManifestDependencies)
                    {
                        new KeyValuePair<string, string>(packageDirectory.Name, relativePackagePath)
                    };

                    //Add all of the dependencies that the package needs to the project, including itself
                    TryAddKeysToManifest(files[ii], tempList);
                    
                }
            }
        }

        //FileWriterBase Functions
        //================================================================================================================//

        protected override void TryCreateFile(in string filePath, in string fileContents) =>
            throw new NotImplementedException("This file should already exist, you should not try creating it");

        protected override void TryUpdateFile(in string filePath, in string fileContents)
        {
            if (File.Exists(filePath) == false)
                throw new ArgumentException($"No File found matching path: {filePath}");
            
            File.WriteAllText(filePath, fileContents);
        }
        
        //Update Manifest File Functions
        //================================================================================================================//
        private void TryAddKeysToManifest(in FileInfo fileInfo, in List<KeyValuePair<string,string>> packageDependencies)
        {
            if (packageDependencies == null || packageDependencies.Count == 0)
                return;
            
            var fileText = File.ReadAllText(fileInfo.FullName);
            var data = JObject.Parse(fileText);

            if (data.ContainsKey(DEPENDENCIES_KEY) == false)
                throw new Exception();

            var dependencies = (JObject)data[DEPENDENCIES_KEY];

            if (dependencies.HasValues == false)
                throw new Exception();

            var propertiesListCopy = dependencies.Values<JProperty>().ToList();
            
            foreach (var (key, value) in packageDependencies)
            {
                if (dependencies.ContainsKey(key))
                {
                    var foundIndex = propertiesListCopy.FindIndex(x => x.Name == key);
                    propertiesListCopy[foundIndex] = new JProperty(key, value);
                }
                //If we need to add a new key, its a little more involved.
                else
                {
                    //If we want to add a new value, but have it sit at the [0] position, we need to create a new list
                    // with the order that we are expecting.
                    propertiesListCopy.Insert(0, new JProperty(key, value));
                }
            }
            var newDependencies = new JObject();
            foreach (var dependency in propertiesListCopy)
            {
                newDependencies.Add(dependency);
            }

            var outObject = new JObject
            {
                { DEPENDENCIES_KEY, newDependencies }
            };
            fileText = JsonConvert.SerializeObject(outObject, Formatting.Indented);

            
#if DEBUG
                Debug.Log(fileText);
#endif
            TryUpdateFile(fileInfo.FullName, fileText);
        }

        //Get Manifest Dependencies
        //================================================================================================================//

        /// <summary>
        /// Get a list of manifest dependencies if a custom source was used (Git/Local).
        /// </summary>
        /// <param name="dependencies"></param>
        /// <param name="customDependencies"></param>
        /// <returns></returns>
        private static List<KeyValuePair<string, string>> GetAsManifestDependencies(
            in IEnumerable<PackageData.DependencyData> dependencies, 
            in IEnumerable<PackageData.DependencyData> customDependencies)
        {
            var packageDependencies = new List<KeyValuePair<string, string>>();

            foreach (var dependencyData in dependencies)
            {
                if(string.IsNullOrWhiteSpace(dependencyData.Source))
                    continue;
                    
                packageDependencies.Add(new KeyValuePair<string, string>(dependencyData.Domain, dependencyData.Source));
            }

            foreach (var dependencyData in customDependencies)
            {
                if(string.IsNullOrWhiteSpace(dependencyData.Source))
                    continue;
                    
                packageDependencies.Add(new KeyValuePair<string, string>(dependencyData.Domain, dependencyData.Source));
            }

            return packageDependencies;
        }

        //================================================================================================================//
    }
}