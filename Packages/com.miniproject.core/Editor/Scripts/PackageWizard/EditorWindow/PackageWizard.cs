using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


namespace MiniProject.Core.Editor.PackageWizard.EditorWindow
{
    public class PackageWizard : UnityEditor.EditorWindow
    {
        private TextInputBaseField<string> _packageNameInputField;
        private PackageData _packageData;
        private DropdownField _editorVersion;
        private ProgressBar _progressBar;


        [MenuItem("Window/Package Wizard")]
        public static void Init()
        {
            PackageWizard wnd = GetWindow<PackageWizard>();
            wnd.titleContent = new GUIContent(R.UI.Title);
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(R.UI.PathToUxml);
            VisualElement labelFromUXML = visualTree.Instantiate();
            root.Add(labelFromUXML);

            // Import USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(R.UI.PathToUSS);
            /*VisualElement labelWithStyle = new Label("Hello! With Style");
            labelWithStyle.styleSheets.Add(styleSheet);
            root.Add(labelWithStyle);*/

            _packageNameInputField = root.Q<TextInputBaseField<string>>(R.UI.PackageNameInputField);


            VisualElement tagsGroup = root.Q<GroupBox>(R.UI.ExperienceTagsFieldName);
            foreach (var tag in Enum.GetValues(typeof(PackageData.ExperienceTag)))
            {
                var toggleItem = new Toggle(tag.ToString());
                tagsGroup.Add(toggleItem);
            }

            VisualElement platformOptionsPlaceholder = root.Q<VisualElement>(R.UI.PlatformOptionsPlaceholderFieldName);
            EnumFlagsField platformOptions = new EnumFlagsField(R.UI.PlatformOptionsFieldName);
            foreach (PackageData.Platform platformType in (PackageData.Platform[])Enum.GetValues(
                         typeof(PackageData.Platform)))
            {
                platformOptions.Init(platformType);
            }

            platformOptionsPlaceholder.Add(platformOptions);

            EnumField renderPipeline = root.Q<EnumField>(R.UI.RenderingPipelineFieldName);
            foreach (PackageData.RenderingPipeline renderPipelineType in (PackageData.RenderingPipeline[])
                     Enum.GetValues(typeof(PackageData.RenderingPipeline)))
            {
                renderPipeline.Init(renderPipelineType);
            }
            // default value
            //renderPipeline.value = PackageData.RenderingPipeline.URP;

            _editorVersion = root.Q<DropdownField>(R.UI.UnityEditorVersionFieldName);
            List<string> versions = new List<string>();
            foreach (var version in Enum.GetValues(typeof(PackageData.UnityVersion)))
            {
                versions.Add(version.ToString());
            }

            _editorVersion.choices = versions;

            Button generateButton = root.Q<Button>(R.UI.GenerateButton);
            generateButton.clicked += GenerateButtonClicked;
            
            _progressBar = root.Q<ProgressBar>(R.UI.ProgressBar);
        }

        private void GenerateButtonClicked()
        {
            _packageData = new PackageData();
            _packageData.Name = _packageNameInputField.text;
            _packageData.HasEditorFolder = true;

            Debug.Log(_editorVersion.index);

            _packageData.UnityVersions = new Dictionary<PackageData.UnityVersion, string>
                { { PackageData.UnityVersion.LTS2021, "LTS2021" } };
            _packageData.Platforms = new Dictionary<PackageData.Platform, string>
                { { PackageData.Platform.Android, "Android" }, { PackageData.Platform.iOS, "iOS" } };
            var generator = new PackageGenerator(_packageData);
            generator.OnProgressChanged += OnProgressChanged;
            generator.Generate();
        }

        private void OnProgressChanged(object sender, ProgressEventArgs progress)
        {
            _progressBar.value = progress.Progress * 100;
            _progressBar.title = progress.Info;
        }
    }
}