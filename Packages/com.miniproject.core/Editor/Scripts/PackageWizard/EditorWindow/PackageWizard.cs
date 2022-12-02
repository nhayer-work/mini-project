using System;
using System.Collections.Generic;
using Scripts.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;

namespace MiniProject.Core.Editor.PackageWizard.EditorWindow
{
    public class PackageWizard : UnityEditor.EditorWindow
    {
	    private PackageData _packageData;
	    
        private TextInputBaseField<string> m_packageNameInputField;
        private EnumFlagsField _platformOptions;
        private Toggle[] _tagToggles;
        private Toggle _usesEditorToggle;
        private Toggle _usesScoreToggle;
        private DropdownField _editorVersion;
        private EnumField _renderPipeline;

		//State dependent Elements
		private VisualElement buttonContainer;
        private ProgressBar m_progressBar;

		//Buttons
		private Button m_ClearButton;
		private Button m_GenerateButton;
		private Button m_LoadButton;
		
		//Warning
		private VisualElement m_warningContainer;
		private Label m_warningLabel;

		//Author Details
		private TextInputBaseField<string> m_AuthorName;
		private TextInputBaseField<string> m_AuthorDesc;

		// TODO: Confirm this member is needed
		// Only created for an example of searching
		// a package in registry and getting its display name
		private List<string> _dependencies = new List<String>();

		private SearchRequest _searchReq;
 

        [MenuItem("Mini Project/Package Wizard/New Package")]
        public static void Init()
        {
            PackageWizard wnd = GetWindow<PackageWizard>();
            wnd.titleContent = new GUIContent(R.UI.Title);
        }

        public void CreateGUI()
        {

            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(R.UI.PathToUxml);
            VisualElement labelFromUXML = visualTree.Instantiate();
            root.Add(labelFromUXML);
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(R.UI.PathToUSS);

			GetReferences(root);

            VisualElement tagsGroup = root.Q<GroupBox>(R.UI.ExperienceTagsFieldName);
            var tags = Enum.GetValues(typeof(PackageData.ExperienceTag));
            _tagToggles = new Toggle[tags.Length];
            var i = 0;
            foreach (var tag in tags)
            {
                var toggleItem = new Toggle(tag.ToString());
                tagsGroup.Add(toggleItem);
                _tagToggles[i++] = toggleItem;
            }

            VisualElement platformOptionsPlaceholder = root.Q<VisualElement>(R.UI.PlatformOptionsPlaceholderFieldName);
            _platformOptions = new EnumFlagsField(R.UI.PlatformOptionsFieldName);
            foreach (Enum platformType in Enum.GetValues(typeof(PackageData.Platform)))
            {
	            _platformOptions.Init(platformType);
            }

            platformOptionsPlaceholder.Add(_platformOptions);

            _renderPipeline = root.Q<EnumField>(R.UI.RenderingPipelineFieldName);
            foreach (Enum renderPipelineType in Enum.GetValues(typeof(PackageData.RenderingPipeline)))
            {
                _renderPipeline.Init(renderPipelineType);
            }

            _editorVersion = root.Q<DropdownField>(R.UI.UnityEditorVersionFieldName);
            List<string> versions = new List<string>();
            foreach (var version in Enum.GetValues(typeof(PackageData.UnityVersion)))
            {
                versions.Add(version.ToString());
            }
            _editorVersion.choices = versions;
			_editorVersion.value = versions[0];
			
			_usesEditorToggle = root.Q<Toggle>(R.UI.IfRequireEditorScriptsFieldName);
			_usesScoreToggle = root.Q<Toggle>(R.UI.IfScoreFieldName);

			SuscribeEvents();
			ClearTool();
            HandleGenerateButtonState();
			SetWarning(false, "");
        }

		private void GetReferences(VisualElement root)
		{
            m_progressBar = root.Q<ProgressBar>(R.UI.ProgressBar);
			m_ClearButton = root.Q<Button>(R.UI.ClearButtonName);
			m_GenerateButton = root.Q<Button>(R.UI.GenerateButtonName);
			m_LoadButton = root.Q<Button>(R.UI.GenerateButtonName);
            m_packageNameInputField = root.Q<TextInputBaseField<string>>(R.UI.PackageNameInputField);

			m_warningContainer = root.Q<VisualElement>(R.UI.WarningContainer);
			m_warningLabel = root.Q<Label>(R.UI.WarningLabel);

			m_AuthorName = root.Q<TextInputBaseField<string>>(R.UI.AuthorNameField);
			m_AuthorDesc = root.Q<TextInputBaseField<string>>(R.UI.AuthorDescription);
		}

		private void SuscribeEvents()
		{
			m_GenerateButton.RegisterCallback<ClickEvent>((e) => GenerateButtonClicked());
			m_ClearButton.RegisterCallback<ClickEvent>((e) => ClearTool());
			m_packageNameInputField.RegisterCallback<ChangeEvent<string>>((e) => HandleGenerateButtonState());
		}

        private void GenerateButtonClicked()
        {
			m_progressBar.style.display = DisplayStyle.Flex;

            _packageData = new PackageData
            {
	            DisplayName = m_packageNameInputField.text,
	            HasEditorFolder = _usesEditorToggle.value,
	            KeepsScore = _usesScoreToggle.value,
	            HasSamples = false,//TODO Will need to add some support for this
	            Version = "0.0.1",
	            Description = m_AuthorDesc.text,
	            AuthorName = m_AuthorName.text,
	            RenderPipeline = _renderPipeline.value.ToString(),
	            AuthorInfo = new PackageData.Author
	            {
		            Name = "MiniProject",
		            Email = "",
		            Url = "https://github.com/navhayer1015/mini-project"
	            }
            };

            var unityVersion = (PackageData.UnityVersion)_editorVersion.index;
            _packageData.UnityVersions = new List<PackageData.UnityVersion> { unityVersion };

            //Get Selected Tags
            //----------------------------------------------------------//
            _packageData.ExperienceTags = new List<PackageData.ExperienceTag>();
            for (var i = 0; i < _tagToggles.Length; i++)
            {
	            var tagToggle = _tagToggles[i];
	            
	            if(tagToggle.value == false)
		            continue;
	            
	            _packageData.ExperienceTags.Add((PackageData.ExperienceTag)i);
            }

            //Determine which Platforms were selected 
            //----------------------------------------------------------//
            _packageData.Platforms = new List<PackageData.Platform>();
            var selectedPlatforms = (PackageData.Platform)_platformOptions.value;
            foreach (Enum platform in Enum.GetValues(typeof(PackageData.Platform)))
            {
	            if (selectedPlatforms.HasFlag(platform) == false)
		            continue;
	            
	            _packageData.Platforms.Add((PackageData.Platform)platform);
            }
            
            //Setup selected dependencies
            //----------------------------------------------------------//
            _packageData.Dependencies = new List<PackageData.Dependency>();
            //TODO Need to connect selected dependencies here
            throw new NotImplementedException("Need to connect selected dependencies here");

            //Custom Dependencies
            //----------------------------------------------------------//
            _packageData.CustomDependencies = new List<PackageData.DependencyData>();
            //TODO Need to add a list of custom dependencies here that the user can specify
            throw new NotImplementedException("Need to connect selected custom dependencies here");

            //----------------------------------------------------------//

            var generator = new PackageGenerator(_packageData);
            generator.OnProgressChanged += OnProgressChanged;
            generator.Generate();
        }

        private void OnProgressChanged(object sender, ProgressEventArgs progress)
        {
            m_progressBar.value = progress.Progress * 100;
            m_progressBar.title = progress.Info;
        }

		private void HandleGenerateButtonState()
		{
			bool textIsEmpty = m_packageNameInputField.text.Trim().Equals("");
			m_GenerateButton.SetEnabled(!textIsEmpty);
			SetWarning(textIsEmpty, R.ErrorMessages.EmptyNameError);

		}

		private void SetWarning(bool show, string message = ""){
			m_warningLabel.text = message;
			m_warningContainer.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
		}

		private void ClearTool()
		{
			m_progressBar.style.display = DisplayStyle.None;
			m_packageNameInputField.SetValueWithoutNotify("");
		}

		private PackageData.DependencyData[] GetDependencies(string enumString)
		{
			var depList = R.Dependencies.DependencyDatas[(PackageData.Dependency)Enum.Parse(typeof(PackageData.Dependency), enumString)];

			return depList;
		}

		
		// TODO: Confirm these functions are needed
		// Example functions to grab package display name from domain name
		private void SearchPackage(string name)
		{
			_searchReq = Client.Search(name);
			EditorApplication.update += SearchPackageHandle;
		}

		private void SearchPackageHandle()
		{
			if (_searchReq != null && _searchReq.IsCompleted)
			{
				
				if (_searchReq.Status == StatusCode.Success)
				{
					_dependencies.Add(_searchReq.Result[0].displayName);
				}
				else
				{
					// Couldn't find the package from registry
				}
				_searchReq = null;
			}
		}
    }
}