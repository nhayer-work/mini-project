using System.IO;
using MiniProject.Core.Editor.Utilities;
using Scripts.Core;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace MiniProject.Core.Editor.PackageWizard
{
    public class ReadmeWriter : FileWriterBase
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
            var packageInfo = PackageInfo.FindForAssembly(GetType().Assembly);
            var obj = (TextAsset) EditorGUIUtility.Load($"Packages/{packageInfo.name}/Editor/Resources/ReadMeTemplate.md");
            var text = obj.text.Replace("[TITLE]", packageData.DisplayName);
            text = text.Replace("[Description]", packageData.Description);
            
            var path = Path.Combine(pathToRuntimeDirectory, "README.md");
            TryCreateFile(path, text);
        }
    }
}