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
    }
}