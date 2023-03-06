using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using MiniProject.Core.Editor.PackageWizard;
using MiniProject.Core.Editor.Utilities;
using MiniProject.PackageWizard.EditorWindow;
using MiniProject.PackageWizard.FileWriters;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace MiniProject.PackageWizard
{
    public class PackageGenerator
    {
        private const string RUNTIME_DIR_NAME = "Runtime";
        private const string TESTS_DIR_NAME = "Tests";
        private const string EDITOR_DIR_NAME = "Editor";
        private const string SAMPLES_DIR_NAME = "Samples";
        
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

        private static bool IsEmptyName(string packageDataName)
        {
            if (!string.IsNullOrEmpty(packageDataName)) return false;
            EditorUtility.DisplayDialog(R.UI.Title, "Package name is empty", "Ok");
            return true;
        }

        private bool CheckForExisting()
        {
            OnProgressChanged?.Invoke(this, new ProgressEventArgs(R.Progress.CheckExisting, .1f));

            if (IsEmptyName(_packageData.DisplayName))
                return false;

            //Find and remove any illegal characters
            //----------------------------------------------------------//
            var regexItem = new Regex("[^a-zA-Z0-9_.]+");
            _packageData.Name = $"com.miniproject.{regexItem.Replace(_packageData.DisplayName.ToLower(), "")}";

            //----------------------------------------------------------//

            _rootPackagePath = Path.Combine(_packageData.Path, _packageData.Name).Replace("\\","/");

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
            UpdateManifests(
                _packageData.Directory, 
                _packageData.SelectedProjects.ToArray(),
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
            DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, RUNTIME_DIR_NAME));
            DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, TESTS_DIR_NAME));

            if (_packageData.HasEditorFolder)
                DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, EDITOR_DIR_NAME));
            if (_packageData.HasSamples)
                DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, SAMPLES_DIR_NAME));

            _packageData.Directory = new DirectoryInfo(_rootPackagePath);
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
        /// <param name="targetProjects"></param>
        /// <param name="dependencies"></param>
        /// <param name="customDependencies"></param>
        /// <param name="packageDirectory"></param>
        private void UpdateManifests(
            in DirectoryInfo packageDirectory, 
            in DirectoryInfo[] targetProjects,
            in PackageData.DependencyData[] dependencies,
            in PackageData.DependencyData[] customDependencies)
        {
            
            OnProgressChanged?.Invoke(this, new ProgressEventArgs(R.Progress.Manifest, .7f));
            var manifestWriter = new ManifestWriter();
            manifestWriter.UpdateManifestFiles(packageDirectory, targetProjects, dependencies, customDependencies);
        }

        private void PostGenerate()
        {
            OnProgressChanged?.Invoke(this, new ProgressEventArgs(R.Progress.Completed, 1f));
        }
    }
}