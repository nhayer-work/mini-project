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


        [MenuItem("Window/Package Wizard")]
        public static void Init()
        {
            PackageWizard wnd = GetWindow<PackageWizard>();
            wnd.titleContent = new GUIContent(R.Title);
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(R.PathToUxml);
            VisualElement labelFromUXML = visualTree.Instantiate();
            root.Add(labelFromUXML);

            // Import USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(R.PathToUSS);
            /*VisualElement labelWithStyle = new Label("Hello! With Style");
            labelWithStyle.styleSheets.Add(styleSheet);
            root.Add(labelWithStyle);*/

            _packageNameInputField = root.Q<TextInputBaseField<string>>(R.PackageNameInputField);


            VisualElement tagsGroup = root.Q<GroupBox>(R.ExperienceTagsFieldName);
            foreach (var tag in Enum.GetValues(typeof(PackageData.ExperienceTag)))
            {
                var toggleItem = new Toggle(tag.ToString());
                tagsGroup.Add(toggleItem);
            }

            VisualElement platformOptionsPlaceholder = root.Q<VisualElement>(R.PlatformOptionsPlaceholderFieldName);
            EnumFlagsField platformOptions = new EnumFlagsField(R.PlatformOptionsFieldName);
            foreach (PackageData.Platform platformType in (PackageData.Platform[])Enum.GetValues(
                         typeof(PackageData.Platform)))
            {
                platformOptions.Init(platformType);
            }

            platformOptionsPlaceholder.Add(platformOptions);

            EnumField renderPipeline = root.Q<EnumField>(R.RenderingPipelineFieldName);
            foreach (PackageData.RenderingPipeline renderPipelineType in (PackageData.RenderingPipeline[])
                     Enum.GetValues(typeof(PackageData.RenderingPipeline)))
            {
                renderPipeline.Init(renderPipelineType);
            }
            // default value
            //renderPipeline.value = PackageData.RenderingPipeline.URP;

            _editorVersion = root.Q<DropdownField>(R.UnityEditorVersionFieldName);
            List<string> versions = new List<string>();
            foreach (var version in Enum.GetValues(typeof(PackageData.UnityVersion)))
            {
                versions.Add(version.ToString());
            }
            
            _editorVersion.choices = versions;

            Button generateButton = root.Q<Button>(R.GenerateButton);
            generateButton.clicked += GenerateButtonClicked;
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
                { { PackageData.Platform.Android, "Android" }, {PackageData.Platform.iOS, "iOS"} };
            var generator = new PackageGenerator(_packageData);

            generator.Generate();
        }
    }
}