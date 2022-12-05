using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using MiniProject.Core.Editor.PackageWizard.EditorWindow;
using MiniProject.Core.Editor.Utilities;
using Scripts.Core;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace MiniProject.Core.Editor.PackageWizard
{
    public class PackageGenerator
    {
        private readonly PackageData _packageData;
        private string _rootPackagePath;

        public event EventHandler<ProgressEventArgs> OnProgressChanged;

        public PackageGenerator(PackageData packageData)
        {
            _packageData = packageData;
        }

        public void Generate()
        {
            if (!CheckForExisting())
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            EditorCoroutineUtility.StartCoroutine(CreateNewPackageCoroutine(), this);
        }

        private bool IsEmptyName(string packageDataName)
        {
            if (!string.IsNullOrEmpty(packageDataName)) return false;
            EditorUtility.DisplayDialog(R.UI.Title, "Package name is empty", "Ok");
            return true;
        }

        private string FormatPackagePath(string packageName)
        {
            var packageInfo = PackageInfo.FindForAssembly(GetType().Assembly);
            if (packageInfo == null) return null;

            var corePackagePath = packageInfo.resolvedPath;
            _packageData.Name = packageInfo.name.Replace("core", packageName);

            var newPackagePath = corePackagePath.Replace(packageInfo.name, _packageData.Name);
            return newPackagePath;
        }

        private bool CheckForExisting()
        {
            OnProgressChanged?.Invoke(this, new ProgressEventArgs(R.Progress.CheckExisting, .1f));

            if (IsEmptyName(_packageData.DisplayName))
                return false;

            var regexItem = new Regex("[^a-zA-Z0-9_.]+");
            _packageData.Name = regexItem.Replace(_packageData.DisplayName.ToLower(), "");
            _rootPackagePath = FormatPackagePath(_packageData.Name);

            if (!DirectoryOperations.CreateFolder(_rootPackagePath))
            {
                //todo Ask to load the existing package instead and setup UI with data
                if (!EditorUtility.DisplayDialog(R.UI.Title, "Package already exists, overwrite?", "Yes", "No"))
                    return false;
            }

            return true;
        }

        private IEnumerator CreateNewPackageCoroutine()
        {
            var wait = new EditorWaitForSeconds(.1f);
            yield return wait;
            TryCreateDirectories();
            yield return wait;
            TryCreateFiles();
            yield return wait;
            TryCreateAssemblyDefinitions();
            yield return wait;
            UpdateManifests(_packageData.Name, 
                _packageData.UnityVersions.ToArray(), 
                _packageData.Platforms.ToArray(),
                _packageData.Dependencies.ToArray(),
                _packageData.CustomDependencies.ToArray());
            yield return wait;
            PostGenerate();
            yield return wait;

            yield return new WaitUntil(() => EditorUtility.DisplayDialog(R.UI.Title, "Package created", "Ok"));
            //FIXME This is not refresh as expected, requires manual refresh by user
            AssetDatabase.Refresh();
        }

        private void TryCreateFiles()
        {
            OnProgressChanged?.Invoke(this, new ProgressEventArgs(R.Progress.Files, .3f));
            CreatePackageFile();
            CreateReadMeFile();
            CreateConfigFile();
        }


        private void TryCreateDirectories()
        {
            OnProgressChanged?.Invoke(this, new ProgressEventArgs(R.Progress.Folder, .2f));
            DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, "Runtime"));
            DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, "Tests"));

            if (_packageData.HasEditorFolder)
                DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, "Editor"));
            if (_packageData.HasSamples)
                DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, "Samples"));
        }

        private void TryCreateAssemblyDefinitions()
        {
            OnProgressChanged?.Invoke(this, new ProgressEventArgs(R.Progress.Assembly, .4f));
            var assemblyWriter = new AssemblyWriter();
            assemblyWriter.GenerateAssemblyFiles(_packageData.Name, _rootPackagePath, _packageData.HasEditorFolder);
        }

        private void CreatePackageFile()
        {
            var packageJsonWriter = new PackageJsonWriter();
            packageJsonWriter.Generate(_packageData, _rootPackagePath);
        }

        private void CreateReadMeFile()
        {
            var readmeWriter = new ReadmeWriter();
            readmeWriter.Generate(_packageData, _rootPackagePath);
        }
        
        private void CreateConfigFile()
        {
            var configDataWriter = new ConfigDataWriter();
            configDataWriter.Generate(_packageData, _rootPackagePath);
        }


        /// <summary>
        /// This function will include the package, with a generated path in the respective platform & unity versions,
        /// to manifest.json & packages-lock.json. If the entry already exists, then the it will only overwrite the
        /// directory information.
        /// </summary>
        /// <param name="packageName">com.miniproject.EXAMPLE</param>
        /// <param name="supportedUnityVersions">No need to include the subversions of Unity, just the major will suffice. Examples: "2021", "2022"</param>
        /// <param name="supportedPlatforms">All platforms will need to start with "miniproject-". Examples: "miniproject-ios","miniproject-webgl"</param>
        /// <param name="dependencies"></param>
        /// <param name="customDependencies"></param>
        private void UpdateManifests(in string packageName, 
            in PackageData.UnityVersion[] supportedUnityVersions,
            in PackageData.Platform[] supportedPlatforms,
            in PackageData.Dependency[] dependencies,
            in PackageData.DependencyData[] customDependencies)
        {
            OnProgressChanged?.Invoke(this, new ProgressEventArgs(R.Progress.Manifest, .7f));
            var manifestWriter = new ManifestWriter();
            manifestWriter.UpdateManifestFiles(packageName, supportedUnityVersions, supportedPlatforms, dependencies, customDependencies);
        }

        private void PostGenerate()
        {
            OnProgressChanged?.Invoke(this, new ProgressEventArgs(R.Progress.Completed, 1f));
        }
    }
}