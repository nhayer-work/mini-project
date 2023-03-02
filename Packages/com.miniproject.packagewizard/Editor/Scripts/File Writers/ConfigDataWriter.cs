using System.IO;
using MiniProject.Core.Editor.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiniProject.PackageWizard.FileWriters
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
                { "Dependencies", emptyArray },
                { "Description", packageData.Description },
                { "Tags", string.Join(", ", packageData.ExperienceTags.ToArray()) },
                { "RenderPipeline", packageData.RenderPipeline }
            };
            var path = Path.Combine(pathToRuntimeDirectory, $"config.json");
            TryCreateFile(path, JsonConvert.SerializeObject(configData, Formatting.Indented));
        }
    }
}