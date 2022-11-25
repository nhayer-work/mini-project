using System;
using System.IO;
using System.Linq;
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
        private string _packageAssembly;

        public PackageGenerator(PackageData packageData)
        {
            _packageData = packageData;
        }

        //Generate Functions
        //================================================================================================================//

        public void Generate()
        {
            if (IsEmptyName(_packageData.Name))
                return;
            
            var newPackageName = _packageData.Name.ToLower().Trim();
            _rootPackagePath = FormatPackagePath(newPackageName);

            if (!DirectoryOperations.CreateFolder(_rootPackagePath))
            {
                //todo Ask to load the existing package instead and setup UI with data
                if (!EditorUtility.DisplayDialog(R.Title, "Package already exists, overwrite?", "Yes", "No"))
                    return;
            }

            CreateNewPackage();

            EditorUtility.DisplayDialog(R.Title, "Package created", "Ok");
        }

        private bool IsEmptyName(string packageDataName)
        {
            if (!string.IsNullOrEmpty(packageDataName)) return false;
            EditorUtility.DisplayDialog(R.Title, "Package name is empty", "Ok");
            return true;
        }

        private string FormatPackagePath(string packageName)
        {
            var packageInfo = PackageInfo.FindForAssembly(GetType().Assembly);
            if (packageInfo == null) return null;

            var corePackagePath = packageInfo.resolvedPath;
            _packageAssembly = packageInfo.name.Replace("core", packageName);
            
            var newPackagePath = corePackagePath.Replace(packageInfo.name, _packageAssembly);
            return newPackagePath;
        }

        private void CheckForExisting()
        {
            throw new NotImplementedException();
        }

        private void CreateNewPackage()
        {
            TryCreateDirectories();
            TryCreateFiles();
            
            TryCreateAssemblyDefinitions( );
            
            UpdateManifests(_packageAssembly, _packageData.UnityVersions.Keys.ToArray(), _packageData.Platforms.Keys.ToArray());
            
            PostGenerate();
        }

        private void TryCreateFiles()
        {
            EditorUtility.DisplayProgressBar(R.Title, R.Progress.Files, .3f);
            CreatePackageFile();
        }

        private void TryCreateDirectories()
        {
            EditorUtility.DisplayProgressBar(R.Title, R.Progress.Folder, .1f);
            DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, "Runtime"));
            DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, "Tests"));

            if(_packageData.HasEditorFolder)
                DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, "Editor"));
            if(_packageData.HasSamples)
                DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, "Samples"));
        }

        private void TryCreateAssemblyDefinitions( )
        {
            EditorUtility.DisplayProgressBar(R.Title, R.Progress.Assembly, .4f);
            var assemblyWriter = new AssemblyWriter();
            assemblyWriter.GenerateAssemblyFiles(_packageAssembly, _rootPackagePath, _packageData.HasEditorFolder);
        }

        private void CreatePackageFile()
        {
            var packageJsonWriter = new PackageJsonWriter();
            packageJsonWriter.Generate();
        }
        //Post-Generate Functions
        //================================================================================================================//

        private void PostGenerate()
        {
            EditorUtility.ClearProgressBar();
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
            EditorUtility.DisplayProgressBar(R.Title, R.Progress.Manifest, .6f);
            var manifestWriter = new ManifestWriter();
            manifestWriter.UpdateManifestFiles(packageName, supportedUnityVersions, supportedPlatforms);
        }

        //================================================================================================================//
    }
}