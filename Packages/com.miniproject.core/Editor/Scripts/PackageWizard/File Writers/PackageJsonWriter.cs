using System.IO;
using MiniProject.Core.Editor.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scripts.Core;
using UnityEngine;

namespace MiniProject.Core.Editor.PackageWizard
{
    public class PackageJsonWriter : FileWriterBase
    {
        protected override void TryCreateFile(in string filePath, in string fileContents)
        {
            Debug.Log(fileContents);
            FileOperations.Create(filePath, fileContents);
        }

        protected override void TryUpdateFile(in string filePath, in string fileContents)
        {
            throw new System.NotImplementedException();
        }

        public void Generate(PackageData packageData, string pathToRuntimeDirectory)
        {
            var emptyArray = new JArray();
            var author = new JObject
            {
                { "name", packageData.AuthorInfo.Name },
                { "email", packageData.AuthorInfo.Email },
                { "url", packageData.AuthorInfo.Url }
            };
            var packageInfo = new JObject
            {
                { "name", packageData.Name },
                { "version", packageData.Version },
                { "displayName", packageData.DisplayName },
                { "description", packageData.Description },
                { "unity", packageData.UnityVersionFormatted},
                { "unityRelease",packageData.UnityRelease },
                { "keywords", emptyArray },
                { "author", author },
                { "dependencies", new JObject()}
            };
            var path = Path.Combine(pathToRuntimeDirectory, $"package.json");
            TryCreateFile(path, JsonConvert.SerializeObject(packageInfo, Formatting.Indented));
        }
    }
}