using System;
using System.Collections.Generic;
using Scripts.Core;
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
            _packageData.DisplayName = _packageNameInputField.text;
            _packageData.HasEditorFolder = true;
            _packageData.HasSamples = true;

            _packageData.Version = "0.0.1";
            _packageData.Version = "A new experience";

            _packageData.AuthorInfo = new PackageData.Author
            {
                Name = "Smart Developer",
                Email = "",
                Url = ""
            };
        
            var unityVersion = (PackageData.UnityVersion)_editorVersion.index;
            Debug.Log($"Selected Editor Version {_editorVersion.index} | {unityVersion}");
            _packageData.UnityVersions = new List<PackageData.UnityVersion> { unityVersion };

            _packageData.Platforms = new List<PackageData.Platform>
                { PackageData.Platform.Android, PackageData.Platform.iOS };
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