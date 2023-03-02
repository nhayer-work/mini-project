using System;
using System.IO;
using MiniProject.Core.Editor.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace MiniProject.PackageWizard.FileWriters
{
    //Based on: https://docs.unity3d.com/Manual/cus-asmdef.html
    public class AssemblyWriter : FileWriterBase
    {
        //FileWriterBase Functions
        //================================================================================================================//

        protected override void TryCreateFile(in string filePath, in string fileContents)
        {
            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Directory.Exists == false)
                throw new Exception($"Directory not found for {filePath}");

            if (fileInfo.Directory.GetFiles("*.asmdef", SearchOption.TopDirectoryOnly).Length > 0)
                throw new Exception($"Cannot create new asmdef, Assembly definition file already exists in {fileInfo.Directory}");


            File.WriteAllText(filePath, fileContents);

            AssetDatabase.Refresh();
        }

        protected override void TryUpdateFile(in string filePath, in string fileContents) =>
            throw new NotImplementedException();
        
        //================================================================================================================//

        public void GenerateAssemblyFiles(in string packageName, in string pathToPackageDirectory, in bool createEditorAssembly)
        {
            GenerateRuntimeAssembly(packageName, Path.Combine(pathToPackageDirectory, "Runtime"));

            if (createEditorAssembly)
                GenerateEditorAssembly(packageName, Path.Combine(pathToPackageDirectory, "Editor"));
        }

        private void GenerateRuntimeAssembly(in string packageName, in string pathToRuntimeDirectory)
        {
            var emptyArray = new JArray();
            var assemblyDefinition = new JObject
            {
                { "name", packageName },
                { "rootNamespace", string.Empty },
                { "references", emptyArray },
                { "includePlatforms", emptyArray },
                { "excludePlatforms", emptyArray },
                { "allowUnsafeCode", false },
                { "overrideReferences", true },
                { "precompiledReferences", emptyArray },
                { "autoReferenced", true },
                { "defineConstraints", emptyArray },
                { "versionDefines", emptyArray },
                { "noEngineReferences", false }
            };
            
            var path = Path.Combine(pathToRuntimeDirectory, $"{packageName}.asmdef");
            TryCreateFile(path, JsonConvert.SerializeObject(assemblyDefinition, Formatting.Indented));
        }
        private void GenerateEditorAssembly(in string packageName, in string pathToEditorDirectory)
        {
            var editorName = $"{packageName}.Editor";
            
            var emptyArray = new JArray();
            var assemblyDefinition = new JObject
            {
                { "name", editorName },
                { "rootNamespace", string.Empty },
                { "references", emptyArray },
                {
                    "includePlatforms", new JArray
                    {
                        "Editor"
                    }
                },
                { "excludePlatforms", emptyArray },
                { "allowUnsafeCode", false },
                { "overrideReferences", true },
                { "precompiledReferences", emptyArray },
                { "autoReferenced", true },
                { "defineConstraints", emptyArray },
                { "versionDefines", emptyArray },
                { "noEngineReferences", false }
            };

            var path = Path.Combine(pathToEditorDirectory, $"{editorName}.asmdef");
            TryCreateFile(path, JsonConvert.SerializeObject(assemblyDefinition, Formatting.Indented));
        }

        //Test Function
        //================================================================================================================//

        private const string CLIMB_TO_REPO_ROOT = @"..\..\..\..\..\";

        //[MenuItem("Mini Project/Package Wizard/Test Assembly Writer")]
        private static void Test()
        {
            
            var packagePath = Path.Combine(UnityEngine.Application.dataPath, CLIMB_TO_REPO_ROOT, "Packages", "com.miniproject.core");
            
            var assemblyWriter = new AssemblyWriter();
            assemblyWriter.GenerateAssemblyFiles("miniproject.test", packagePath, true);
        }

        //================================================================================================================//
    }
    
    /*
        //Runtime asmdef
        //================================================================================================================//
        {
            "name": "miniproject.core",
            "rootNamespace": "",
            "references": [],
            "includePlatforms": [],
            "excludePlatforms": [],
            "allowUnsafeCode": false,
            "overrideReferences": true,
            "precompiledReferences": [
                "Newtonsoft.Json.dll"
            ],
            "autoReferenced": true,
            "defineConstraints": [],
            "versionDefines": [],
            "noEngineReferences": false
        }
        //Editor asmdef
        //================================================================================================================//      
        {
            "name": "miniproject.core.Editor",
            "rootNamespace": "",
            "references": [],
            "includePlatforms": [
                "Editor"
            ],
            "excludePlatforms": [],
            "allowUnsafeCode": false,
            "overrideReferences": true,
            "precompiledReferences": [
                "Newtonsoft.Json.dll"
            ],
            "autoReferenced": true,
            "defineConstraints": [],
            "versionDefines": [],
            "noEngineReferences": false
        }
*/
}