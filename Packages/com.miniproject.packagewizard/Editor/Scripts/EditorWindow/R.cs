using System.Collections.Generic;
using MiniProject.PackageWizard.ScriptableObjects;
using UnityEditor;

namespace MiniProject.PackageWizard.EditorWindow
{
    public static class R
    {
        public class UI
        {
            public const string PathToUxml =
                "Packages/com.miniproject.packagewizard/Editor/Scripts/EditorWindow/PackageWizard.uxml";
            public const string PathToUSS = 
                "Packages/com.miniproject.packagewizard/Editor/Scripts/EditorWindow/PackageWizard.uss";
        
            public const string PackageNameInputField = "ExperienceName";
            public const string ExperienceTagsFieldName = "ExperienceTags";
            public const string PlatformOptionsPlaceholderFieldName = "PlatformOptionsPlaceholder";
            public const string PlatformOptionsFieldName = "PlatformOptions";
            public const string RenderingPipelineFieldName = "RenderPipeline";
            public const string UnityEditorVersionFieldName = "UnityEditorVersion";
            public const string IfRequireEditorScriptsFieldName = "IfRequireEditorScripts";
            public const string IfScoreFieldName = "IfScore";

			public const string FoldoutTagsName = "FDTags";

			public const string ButtonsContainer = "StateButtons";
            public const string GenerateButtonName = "GeneratePackage";
            public const string LoadButtonName = "LoadPackage";
            public const string ClearButtonName = "ClearButton";
            public const string RefreshButtonName = "RefreshButton";

			public const string WarningContainer = "WarningContainer";
			public const string WarningLabel = "WarningLabel";

			public const string AuthorNameField = "IfAuthor";
			public const string AuthorDescription = "IfDescription";
        
            public const string ProgressBar = "FileProgressBar";

			public const string DependenciesFoldout = "SCDependencies";
			public const string DependenciesScrollview = "ScrollDependencies";
            public const string ProjectsScrollview = "ScrollProject";

            public const string PackageLocationInputField = "package-location";
            public const string PackageLocationButton = "select-location-button";
            
            public const string CustomDependenciesListView = "custom-dependencies";
        
            public const string Title = "PackageWizard";
            
            public class DependencyData
            {
                public const string UXMLPath =
                    "Packages/com.miniproject.packagewizard/Editor/Scripts/EditorWindow/DependencyData.uxml";
                public const string GroupBoxName = "custom-dependency-group";

                public const string DisplayNameField = "display-name";
                public const string DomainNameField = "domain-name";
                public const string VersionField = "version";
                public const string SourceField = "source";
            }
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

        //Package Wizard Settings Access
        //================================================================================================================//

        private const string PACKAGE_SETTINGS_PATH =
            "Packages/com.miniproject.packagewizard/Editor/Package Wizard Settings.asset";
        private static PackageWizardSettingsScriptableObject PackageWizardSettings
        {
            get
            {
                if (s_PackageWizardSettings == null)
                    s_PackageWizardSettings = AssetDatabase.LoadAssetAtPath<PackageWizardSettingsScriptableObject>(
                        PACKAGE_SETTINGS_PATH);
                
                return s_PackageWizardSettings;
            }
        }
        private static PackageWizardSettingsScriptableObject s_PackageWizardSettings;
        
        public class MinVersions
        {
            public static IReadOnlyList<string> UnityVersions => PackageWizardSettings.unityVersions;

        }
        
        public class Projects
        {
            public static IReadOnlyDictionary<string, List<PackageWizardSettingsScriptableObject.ProjectInfo>>
                GroupedProjectDirectories => PackageWizardSettings.GetGroupedProjectDirectories();
            
            public static IReadOnlyList<PackageWizardSettingsScriptableObject.ProjectInfo> ProjectDirectories => PackageWizardSettings.ProjectDirectories;
        }

        public class Dependencies
        {
            public static IReadOnlyDictionary<string, PackageData.DependencyData[]> DependencyDatas =>
                PackageWizardSettings.Dependencies;
        }
        //================================================================================================================//

    }
}