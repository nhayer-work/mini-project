using System;

namespace MiniProject.Core.Editor.PackageWizard
{
    public class PackageGenerator
    {
        public PackageGenerator()
        {
            throw new NotImplementedException();
        }

        //Generate Functions
        //================================================================================================================//

        private void Generate()
        {
            throw new NotImplementedException();
        }

        private void CheckForExisting()
        {
            throw new NotImplementedException();
        }

        private void CreateNewPackage()
        {
            throw new NotImplementedException();
        }

        private void TryCreateFiles()
        {
            throw new NotImplementedException();
        }
        private void TryCreateDirectories()
        {
            throw new NotImplementedException();
        }
        private void TryCreateAssemblyDefinitions()
        {
            throw new NotImplementedException();
        }
        //Post-Generate Functions
        //================================================================================================================//

        private void PostGenerate()
        {
            throw new NotImplementedException();
        }

        //FIXME Need to use a list of enums instead for versions & platforms to avoid any string issues
        /// <summary>
        /// This function will include the package, with a generated path in the respective platform & unity versions,
        /// to manifest.json & packages-lock.json. If the entry already exists, then the it will only overwrite the
        /// directory information.
        /// </summary>
        /// <param name="packageName">com.miniproject.EXAMPLE</param>
        /// <param name="supportedUnityVersions">No need to include the subversions of Unity, just the major will suffice. Examples: "2021", "2022"</param>
        /// <param name="supportedPlatforms">All platforms will need to start with "miniproject-". Examples: "miniproject-ios","miniproject-webgl"</param>
        private void UpdateManifests(in string packageName, in string[] supportedUnityVersions, in string[] supportedPlatforms)
        {
            var manifestWriter = new ManifestWriter();
            manifestWriter.UpdateManifestFiles(packageName, supportedUnityVersions, supportedPlatforms);
        }
        
        //================================================================================================================//
    }
}