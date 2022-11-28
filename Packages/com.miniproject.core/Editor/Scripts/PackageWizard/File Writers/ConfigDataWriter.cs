using System.IO;
using MiniProject.Core.Editor.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scripts.Core;

namespace MiniProject.Core.Editor.PackageWizard
{
    public class ConfigDataWriter : FileWriterBase
    {
        protected override void TryCreateFile(in string filePath, in string fileContents)
        {
            FileOperations.Create(filePath, fileContents);
        }

        protected override void TryUpdateFile(in string filePath, in string fileContents)
        {
            throw new System.NotImplementedException();
        }


        public void Generate(PackageData packageData, string pathToRuntimeDirectory)
        {
            var emptyArray = new JArray();
            var configData = new JObject
            {
                { "Name", packageData.Name },
                { "DisplayName", packageData.DisplayName },
                { "Platforms", JsonConvert.SerializeObject(packageData.Platforms) },
                { "Dependencies", emptyArray },
                { "EditorVersion", packageData.UnityVersionFormatted }
            };
            var path = Path.Combine(pathToRuntimeDirectory, $"config.json");
            TryCreateFile(path, JsonConvert.SerializeObject(configData, Formatting.Indented));
        }
    }
}