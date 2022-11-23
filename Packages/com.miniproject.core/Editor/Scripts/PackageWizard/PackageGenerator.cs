using System;
using System.IO;
using MiniProject.Core.Editor.PackageWizard.EditorWindow;
using MiniProject.Core.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace MiniProject.Core.Editor.PackageWizard
{
    public class PackageGenerator
    {
        private readonly PackageData _packageData;
        private string _rootPackagePath;

        public PackageGenerator(PackageData packageData)
        {
            _packageData = packageData;
        }

        //Generate Functions
        //================================================================================================================//

        public void Generate()
        {
            var newPackageName = _packageData.name.ToLower().Trim();
            _rootPackagePath = FormatPackagePath(newPackageName);
            
            if (!DirectoryOperations.CreateFolder(_rootPackagePath))
            {
                if (EditorUtility.DisplayDialog(R.Title, "Package already exists, overwrite?", "Yes", "No"))
                {
                    DirectoryOperations.DeleteFolder(_rootPackagePath);
                    DirectoryOperations.CreateFolder(_rootPackagePath);
                }
            }
            CreateNewPackage();
        }

        private string FormatPackagePath(string packageName)
        {
            var packageInfo = PackageInfo.FindForAssembly(GetType().Assembly);
            if (packageInfo == null) return null;

            var corePackagePath = packageInfo.resolvedPath;
            var corePackageName = packageInfo.name;
            
            corePackageName = corePackageName.Replace("core", packageName);
            Debug.Log(corePackagePath);
            
            var newPackagePath = corePackagePath.Replace(packageInfo.name, corePackageName);
            return newPackagePath;
        }

        private void CheckForExisting()
        {
            throw new NotImplementedException();
        }

        private void CreateNewPackage()
        {
            TryCreateDirectories();
        }

        private void TryCreateFiles()
        {
            throw new NotImplementedException();
        }

        private void TryCreateDirectories()
        {
            DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, "Runtime"));
            DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, "Editor"));
            DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, "Tests"));
        }

        private void TryCreateAssemblyDefinitions(in string packageName, in string packageDirectory,
            in bool usesEditorDirectory)
        {
            var assemblyWriter = new AssemblyWriter();
            assemblyWriter.GenerateAssemblyFiles(packageName, packageDirectory, usesEditorDirectory);
        }
        //Post-Generate Functions
        //================================================================================================================//

        private void PostGenerate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This function will include the package, with a generated path in the respective platform & unity versions,
        /// to manifest.json & packages-lock.json. If the entry already exists, then the it will only overwrite the
        /// directory information.
        /// </summary>
        /// <param name="packageName">com.miniproject.EXAMPLE</param>
        /// <param name="supportedUnityVersions">No need to include the subversions of Unity, just the major will suffice. Examples: "2021", "2022"</param>
        /// <param name="supportedPlatforms">All platforms will need to start with "miniproject-". Examples: "miniproject-ios","miniproject-webgl"</param>
        private void UpdateManifests(in string packageName, in PackageData.UnityVersion[] supportedUnityVersions,
            in PackageData.Platform[] supportedPlatforms)
        {
            var manifestWriter = new ManifestWriter();
            manifestWriter.UpdateManifestFiles(packageName, supportedUnityVersions, supportedPlatforms);
        }

        //================================================================================================================//
    }
}