using System;
using System.Collections;
using System.IO;
using System.Linq;
using MiniProject.Core.Editor.PackageWizard.EditorWindow;
using MiniProject.Core.Editor.Utilities;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace MiniProject.Core.Editor.PackageWizard
{
    public class PackageGenerator
    {
        private readonly PackageData _packageData;
        private string _rootPackagePath;
        private string _packageAssembly;
        
        public event EventHandler<ProgressEventArgs> OnProgressChanged;

        public PackageGenerator(PackageData packageData)
        {
            _packageData = packageData;
        }

        //Generate Functions
        //================================================================================================================//

        public void Generate()
        {
            if(!CheckForExisting())
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            EditorCoroutineUtility.StartCoroutine(CreateNewPackage(), this);
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
            _packageAssembly = packageInfo.name.Replace("core", packageName);
            
            var newPackagePath = corePackagePath.Replace(packageInfo.name, _packageAssembly);
            return newPackagePath;
        }

        private bool CheckForExisting()
        {
            OnProgressChanged?.Invoke(this, new ProgressEventArgs(R.Progress.CheckExisting, .1f));
            
            if (IsEmptyName(_packageData.Name))
                return false;
            
            var newPackageName = _packageData.Name.ToLower().Trim();
            _rootPackagePath = FormatPackagePath(newPackageName);

            if (!DirectoryOperations.CreateFolder(_rootPackagePath))
            {
                //todo Ask to load the existing package instead and setup UI with data
                if (!EditorUtility.DisplayDialog(R.UI.Title, "Package already exists, overwrite?", "Yes", "No"))
                    return false;
            }
            return true;
        }

        private IEnumerator CreateNewPackage()
        {
            yield return  new EditorWaitForSeconds(.3f);
            TryCreateDirectories();
            yield return  new EditorWaitForSeconds(.3f);
            TryCreateFiles();
            yield return  new EditorWaitForSeconds(.3f);
            TryCreateAssemblyDefinitions( );
            yield return  new EditorWaitForSeconds(.3f);
            UpdateManifests(_packageAssembly, _packageData.UnityVersions.Keys.ToArray(), _packageData.Platforms.Keys.ToArray());
            yield return  new EditorWaitForSeconds(.3f);
            PostGenerate();
        }

        private void TryCreateFiles()
        {
            OnProgressChanged?.Invoke(this, new ProgressEventArgs(R.Progress.Files, .3f));
            CreatePackageFile();
        }

        private void TryCreateDirectories()
        {
            OnProgressChanged?.Invoke(this, new ProgressEventArgs(R.Progress.Folder, .2f));
            DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, "Runtime"));
            DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, "Tests"));

            if(_packageData.HasEditorFolder)
                DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, "Editor"));
            if(_packageData.HasSamples)
                DirectoryOperations.CreateFolder(Path.Join(_rootPackagePath, "Samples"));
        }

        private void TryCreateAssemblyDefinitions( )
        {
            OnProgressChanged?.Invoke(this, new ProgressEventArgs(R.Progress.Assembly, .4f));
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
            OnProgressChanged?.Invoke(this, new ProgressEventArgs(R.Progress.Completed, 1f));
            EditorUtility.DisplayDialog(R.UI.Title, "Package created", "Ok");
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
            OnProgressChanged?.Invoke(this, new ProgressEventArgs(R.Progress.Manifest, .7f));
            var manifestWriter = new ManifestWriter();
            manifestWriter.UpdateManifestFiles(packageName, supportedUnityVersions, supportedPlatforms);
        }

        //================================================================================================================//
    }
}