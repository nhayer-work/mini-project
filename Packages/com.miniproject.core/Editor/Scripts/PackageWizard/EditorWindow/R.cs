using System.Collections.Generic;
using Scripts.Core;

namespace MiniProject.Core.Editor.PackageWizard.EditorWindow
{
    public static class R
    {
        public class UI
        {
            public const string PathToUxml =
                "Packages/com.miniproject.core/Editor/Scripts/PackageWizard/EditorWindow/PackageWizard.uxml";
            public const string PathToUSS = 
                "Packages/com.miniproject.core/Editor/Scripts/PackageWizard/EditorWindow/PackageWizard.uss";
        
            public const string PackageNameInputField = "ExperienceName";
            public const string ExperienceTagsFieldName = "ExperienceTags";
            public const string PlatformOptionsPlaceholderFieldName = "PlatformOptionsPlaceholder";
            public const string PlatformOptionsFieldName = "PlatformOptions";
            public const string RenderingPipelineFieldName = "RenderPipeline";
            public const string UnityEditorVersionFieldName = "UnityEditorVersion";
            public const string IfRequireEditorScriptsFieldName = "IfRequireEditorScripts";
            public const string IfScoreFieldName = "IfScore";

			public const string ButtonsContainer = "StateButtons";
            public const string GenerateButtonName = "GeneratePackage";
            public const string LoadButtonName = "LoadPackage";
            public const string ClearButtonName = "ClearButton";

			public const string WarningContainer = "WarningContainer";
			public const string WarningLabel = "WarningLabel";

			public const string AuthorNameField = "IfAuthor";
			public const string AuthorDescription = "IfDescription";
        
            public const string ProgressBar = "FileProgressBar";
        
            public const string Title = "PackageWizard";
        }
        

        public class Progress
        {
            public const string CheckExisting = "Checking existing package";
            public const string Folder = "Creating package folders";
            public const string Files = "Creating package files";
            public const string Manifest = "Adding package to the [PROJECT] manifest";
            public const string Assembly = "Adding package assembly definition";
            public const string Completed = "Completed";
        }

		public class ErrorMessages
		{
			public const string EmptyNameError = "Warning: Package Cannot be Empty";
		}

        public class Dependencies
        {
            
            
            /// <summary>
            /// We store the DependencyDatas as an array so that a single dependency tag could come with multiple packages
            /// </summary>
            public static readonly Dictionary<PackageData.Dependency, PackageData.DependencyData[]> DependencyDatas = new()
            {
                [PackageData.Dependency.Common] = new[]
                {
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.feature.2d",
                        Version = "1.0.0"
                    },
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.editorcoroutines",
                        Version = "1.0.0"
                    },
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.postprocessing",
                        Version = "3.2.2"
                    },
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.probuilder",
                        Version = "5.0.6"
                    },
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.textmeshpro",
                        Version = "3.0.6"
                    },
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.timeline",
                        Version = "1.6.4"
                    },
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.visualeffectgraph",
                        Version = "12.1.7"
                    },
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.visualscripting",
                        Version = "1.7.8"
                    },
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.nuget.newtonsoft-json",
                        Version = "3.0.2"
                    }
                },
                [PackageData.Dependency.URP] = new[]
                {
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.render-pipelines.universal",
                        Version = "12.1.7"
                    }
                },
                [PackageData.Dependency.HDRP] = new[]
                {
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.render-pipelines.high-definition",
                        Version = "12.1.7"
                    }
                },
                [PackageData.Dependency.Android] = new[]
                {
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.mobile.android-logcat",
                        Version = "1.3.2"
                    }
                },
                [PackageData.Dependency.Cinemachine] = new[]
                {
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.cinemachine",
                        Version = "2.8.9"
                    }
                },
                [PackageData.Dependency.Shaders] = new[]
                {
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.shadergraph",
                        Version = "12.1.7"
                    }
                },
                [PackageData.Dependency.AR] = new[]
                {
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.xr.openxr",
                        Version = "1.5.3"
                    }
                },
                [PackageData.Dependency.VR] = new[]
                {
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.xr.openxr",
                        Version = "1.5.3"
                    }
                },
                [PackageData.Dependency.MachineLearning] = new[]
                {
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.mathematics",
                        Version = "1.2.6"
                    },
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.ml-agents",
                        Version = "2.0.1"
                    }
                },
                [PackageData.Dependency.NewInputSystem] = new[]
                {
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.inputsystem",
                        Version = "1.4.4"
                    }
                },
                [PackageData.Dependency.TerrainTools] = new[]
                {
                    new PackageData.DependencyData
                    {
                        Name = "com.unity.terrain-tools",
                        Version = "4.0.3"
                    }
                },
            };
        }
    }
}