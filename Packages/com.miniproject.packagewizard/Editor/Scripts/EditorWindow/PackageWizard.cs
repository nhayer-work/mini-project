using System;
using System.Collections.Generic;
using System.IO;
using MiniProject.Core.Editor.PackageWizard;
using MiniProject.PackageWizard.ScriptableObjects;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEngine.Serialization;

namespace MiniProject.PackageWizard.EditorWindow
{
    public class PackageWizard : UnityEditor.EditorWindow
    {
	    private static PackageWizard s_Window;
	    
	    private PackageData m_PackageData;
	    
        private TextInputBaseField<string> m_PackageNameInputField;
		private Button m_RefreshButton;
		private ScrollView m_FoldoutTags;
        private Toggle[] m_TagToggles;
        private Toggle m_UsesEditorToggle;
        private Toggle m_UsesScoreToggle;
        private DropdownField m_MinEditorVersion;
        private EnumField m_RenderPipeline;

		//State dependent Elements
		private VisualElement m_ButtonContainer;
        private ProgressBar m_ProgressBar;

		//Buttons
		private Button m_ClearButton;
		private Button m_GenerateButton;
		private Button m_LoadButton;
		
		//Warning
		private VisualElement m_WarningContainer;
		private Label m_WarningLabel;

		//Author Details
		private TextInputBaseField<string> m_AuthorName;
		private TextInputBaseField<string> m_AuthorDesc;

		// TODO: Confirm this member is needed
		// Only created for an example of searching
		// a package in registry and getting its display name
		private List<string> m_Dependencies = new List<String>();

		private SearchRequest m_SearchReq;
 

		//Additional Optional Dependencies
		private Foldout m_FoldoutDependencies;
		private ScrollView m_ScrollviewDependencies;
		private ScrollView m_ScrollviewProjects;
		//private Toggle[] _dependencyToggles;

		[FormerlySerializedAs("_customDependencies")] [SerializeField]
		private List<PackageData.DependencyData> customDependencies;
		private Dictionary<Toggle,List<Toggle>> m_DependencyToToggle;
		private Dictionary<Toggle, PackageData.DependencyData> m_ToggleToDependencyData;
		
		private Dictionary<Toggle, PackageWizardSettingsScriptableObject.ProjectInfo> m_ToggleToProjectInfo;
		

		[MenuItem("Mini Project/Package Wizard")]
        public static void Init()
        {
	        s_Window = null;
	        s_Window = GetWindow<PackageWizard>();
	        s_Window.titleContent = new GUIContent(R.UI.Title);
        }

        public void CreateGUI()
        {

            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(R.UI.PathToUxml);
            VisualElement labelFromUxml = visualTree.Instantiate();
            root.Add(labelFromUxml);
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(R.UI.PathToUSS);
            root.styleSheets.Add(styleSheet);

			GetReferences(root);

            var tags = Enum.GetValues(typeof(PackageData.ExperienceTag));
            m_TagToggles = new Toggle[tags.Length];
            var i = 0;
            foreach (var tag in tags)
            {
                var toggleItem = new Toggle(tag.ToString());
                // tagsGroup.Add(toggleItem);
				m_FoldoutTags.Add(toggleItem);
                m_TagToggles[i++] = toggleItem;
            }

            //Setup Render Pipeline selection Enum
            //----------------------------------------------------------//
            m_RenderPipeline = root.Q<EnumField>(R.UI.RenderingPipelineFieldName);
            foreach (Enum renderPipelineType in Enum.GetValues(typeof(PackageData.RenderingPipeline)))
            {
                m_RenderPipeline.Init(renderPipelineType);
            }

            m_RenderPipeline.value = (PackageData.RenderingPipeline)0;
            //----------------------------------------------------------//

            m_MinEditorVersion = root.Q<DropdownField>(R.UI.UnityEditorVersionFieldName);

            var versions = R.MinVersions.UnityVersions;
            m_MinEditorVersion.choices = versions;
			m_MinEditorVersion.value = versions[0];
			
			//TODO Disable/deselect projects based on the min version selected
			/*m_MinEditorVersion.RegisterCallback<ChangeEvent<int>>(e =>
			{
				var version = versions[e.newValue];
				foreach (var projectInfo in m_ToggleToProjectInfo)
				{
					if(projectInfo.Value.unityVersion.Contains(version))
				}
			});*/

	        //Project Selection Setup
			//----------------------------------------------------------//
			{
				m_ToggleToProjectInfo = new Dictionary<Toggle, PackageWizardSettingsScriptableObject.ProjectInfo>();
				var projectDirectories = R.Projects.GroupedProjectDirectories;
				
				//todo Group By Editor Versions
				foreach (var groupedProject in projectDirectories)
				{
					/*var projectName = projectInfo.name;
					
					//Create toggle group
					//----------------------------------------------------------//
					//Use the display name as the toggle text, and let the domain be used for the tooltip
					var newProjectToggle = new Toggle(projectName)
					{
						tooltip = projectInfo.unityVersion
					};
					m_ToggleToProjectInfo.Add(newProjectToggle, projectInfo);

					newProjectToggle.RegisterCallback<ChangeEvent<bool>>(
						e => newProjectToggle.value = e.newValue);*/
					
					var projectVersion = groupedProject.Key;
					//Create group box
					//----------------------------------------------------------//
					var newSectionGroupBox = new GroupBox
					{
						name = projectVersion,
						text = projectVersion,
						focusable = false,
						tabIndex = 0,
						viewDataKey = null,
						userData = null,
						usageHints = UsageHints.None,
						pickingMode = PickingMode.Position,
						visible = true,
						generateVisualContent = null,
						tooltip = null,
					};
					newSectionGroupBox.AddToClassList("box-group");
					newSectionGroupBox.AddToClassList("dependency-group");
					newSectionGroupBox.AddToClassList("project-header");

					//Create basic container for all toggles
					//----------------------------------------------------------//\
					var newSectionContainer = new VisualElement()
					{
						name = projectVersion,
					};
					newSectionContainer.AddToClassList("container");

					//Add toggle container to Group Box
					//----------------------------------------------------------//
					newSectionGroupBox.Add(newSectionContainer);

					//Create toggle group
					//----------------------------------------------------------//
					foreach (var projectInfo in groupedProject.Value)
					{
						//Use the display name as the toggle text, and let the domain be used for the tooltip
						var newProjectToggle = new Toggle(projectInfo.projectName)
						{
							tooltip = projectInfo.directory
						};
						m_ToggleToProjectInfo.Add(newProjectToggle, projectInfo);

						newSectionContainer.Add(newProjectToggle);
						newProjectToggle.RegisterCallback<ChangeEvent<bool>>(
							e => newProjectToggle.value = e.newValue);
					}

					//----------------------------------------------------------//
					//Add GroupBox to Dependency scroll view
					m_ScrollviewProjects.Add(newSectionGroupBox);
				}
			}
			//Dependency Setup
			//----------------------------------------------------------//
			{
				m_DependencyToToggle = new Dictionary<Toggle, List<Toggle>>();
				m_ToggleToDependencyData = new Dictionary<Toggle, PackageData.DependencyData>();
				foreach (var dependency in R.Dependencies.DependencyDatas)
				{
					var dependencyName = dependency.Key;
					//Create group box
					//----------------------------------------------------------//
					var newSectionGroupBox = new GroupBox
					{
						name = dependencyName,
						focusable = false,
						tabIndex = 0,
						viewDataKey = null,
						userData = null,
						usageHints = UsageHints.None,
						pickingMode = PickingMode.Position,
						visible = true,
						generateVisualContent = null,
						tooltip = null,
					};
					newSectionGroupBox.AddToClassList("box-group");
					newSectionGroupBox.AddToClassList("dependency-group");
					//Create basic container for all toggles
					//----------------------------------------------------------//\
					var newSectionContainer = new VisualElement()
					{
						name = dependencyName,
					};
					newSectionContainer.AddToClassList("container");

					//Add toggle container to Group Box
					//----------------------------------------------------------//
					newSectionGroupBox.Add(newSectionContainer);

					//Create header toggle
					//----------------------------------------------------------//
					Toggle dependencyToggle = new Toggle($"{dependencyName} Packages");
					newSectionGroupBox.Insert(0, dependencyToggle);

					//Create toggle group
					//----------------------------------------------------------//
					var packageToggleList = new List<Toggle>();
					foreach (var packageData in dependency.Value)
					{
						//Use the display name as the toggle text, and let the domain be used for the tooltip
						var newDependencyToggle = new Toggle(packageData.DisplayName)
						{
							tooltip = packageData.Domain
						};
						m_ToggleToDependencyData.Add(newDependencyToggle, packageData);

						packageToggleList.Add(newDependencyToggle);
						newSectionContainer.Add(newDependencyToggle);
						dependencyToggle.RegisterCallback<ChangeEvent<bool>>(
							e => newDependencyToggle.value = e.newValue);
					}

					//----------------------------------------------------------//

					//Add GroupBox to Dependency scroll view
					m_ScrollviewDependencies.Add(newSectionGroupBox);
				
					//Add toggle to list for future referencing
					m_DependencyToToggle.Add(dependencyToggle, packageToggleList);
				}
			}
			//Custom Dependencies
			//----------------------------------------------------------//
			{
				var customDependencies = root.Q<ListView>("custom-dependencies");
				SerializedObject serializedObject = new UnityEditor.SerializedObject(this);
				SerializedProperty serializedPropertyCustomDependencies = serializedObject.FindProperty(nameof(customDependencies));
				customDependencies.BindProperty(serializedPropertyCustomDependencies);

			}
			//----------------------------------------------------------//



			SubscribeEvents();
			ClearTool();
            HandleGenerateButtonState();
			SetWarning(false, "");
        }

		private void GetReferences(VisualElement root)
		{
            m_ProgressBar = root.Q<ProgressBar>(R.UI.ProgressBar);
			m_ClearButton = root.Q<Button>(R.UI.ClearButtonName);
			m_GenerateButton = root.Q<Button>(R.UI.GenerateButtonName);
			m_LoadButton = root.Q<Button>(R.UI.GenerateButtonName);
            m_PackageNameInputField = root.Q<TextInputBaseField<string>>(R.UI.PackageNameInputField);
			m_RefreshButton = root.Q<Button>(R.UI.RefreshButtonName);

			m_FoldoutTags = root.Q<ScrollView>(R.UI.FoldoutTagsName);
				
			m_UsesEditorToggle = root.Q<Toggle>(R.UI.IfRequireEditorScriptsFieldName);
			m_UsesScoreToggle = root.Q<Toggle>(R.UI.IfScoreFieldName);

			m_WarningContainer = root.Q<VisualElement>(R.UI.WarningContainer);
			m_WarningLabel = root.Q<Label>(R.UI.WarningLabel);

			m_AuthorName = root.Q<TextInputBaseField<string>>(R.UI.AuthorNameField);
			m_AuthorDesc = root.Q<TextInputBaseField<string>>(R.UI.AuthorDescription);

			m_FoldoutDependencies = root.Q<Foldout>(R.UI.DependenciesFoldout);
			m_ScrollviewDependencies = root.Q<ScrollView>(R.UI.DependenciesScrollview);
			m_ScrollviewProjects = root.Q<ScrollView>(R.UI.ProjectsScrollview);
		}

		private void SubscribeEvents()
		{
			m_GenerateButton.RegisterCallback<ClickEvent>((e) => GenerateButtonClicked());
			m_ClearButton.RegisterCallback<ClickEvent>((e) => ClearTool());
			m_PackageNameInputField.RegisterCallback<ChangeEvent<string>>((e) => HandleGenerateButtonState());
			m_RefreshButton.RegisterCallback<ClickEvent>((e) => ForceRefresh());
		}

        private void GenerateButtonClicked()
        {
			m_ProgressBar.style.display = DisplayStyle.Flex;

            m_PackageData = new PackageData
            {
	            DisplayName = m_PackageNameInputField.text,
	            HasEditorFolder = m_UsesEditorToggle.value,
	            KeepsScore = m_UsesScoreToggle.value,
	            HasSamples = false,//TODO Will need to add some support for this
	            Version = "0.0.1",
	            Description = m_AuthorDesc.text,
	            AuthorName = m_AuthorName.text,
	            RenderPipeline = m_RenderPipeline.value.ToString(),
	            AuthorInfo = new PackageData.Author
	            {
		            Name = "MiniProject",
		            Email = "",
		            Url = "https://github.com/navhayer1015/mini-project"
	            }
            };

            m_PackageData.MinSupportedVersion = m_MinEditorVersion.value;

            //Get Selected Tags
            //----------------------------------------------------------//
            m_PackageData.ExperienceTags = new List<PackageData.ExperienceTag>();
            for (var i = 0; i < m_TagToggles.Length; i++)
            {
	            var tagToggle = m_TagToggles[i];
	            
	            if(tagToggle.value == false)
		            continue;
	            
	            m_PackageData.ExperienceTags.Add((PackageData.ExperienceTag)i);
            }

            //Determine which Projects were selected 
            //----------------------------------------------------------//
            m_PackageData.SelectedProjects = new List<DirectoryInfo>();
            foreach (var project in m_ToggleToProjectInfo)
            {
	            if(project.Key.value == false)
		            continue;

	            if (string.IsNullOrWhiteSpace(project.Value.directory))
		            continue;
	            
	            
	            m_PackageData.SelectedProjects.Add(new DirectoryInfo(project.Value.directory));
            }
            
            //Setup selected dependencies
            //----------------------------------------------------------//
            m_PackageData.Dependencies = new List<PackageData.DependencyData>();
            foreach (var data in m_ToggleToDependencyData)
            {
	            if(data.Key.value == false)
		            continue;
	            
	            m_PackageData.Dependencies.Add(data.Value);
            }

            //Custom Dependencies
            //----------------------------------------------------------//

            m_PackageData.CustomDependencies = customDependencies == null
	            ? new List<PackageData.DependencyData>()
	            : new List<PackageData.DependencyData>(customDependencies);
            
            //----------------------------------------------------------//

            var generator = new PackageGenerator(m_PackageData);
            generator.OnProgressChanged += OnProgressChanged;
            generator.Generate();
        }

        private void OnProgressChanged(object sender, ProgressEventArgs progress)
        {
            m_ProgressBar.value = progress.Progress * 100;
            m_ProgressBar.title = progress.Info;
        }

		private void HandleGenerateButtonState()
		{
			bool textIsEmpty = m_PackageNameInputField.text.Trim().Equals("");
			m_GenerateButton.SetEnabled(!textIsEmpty);
			SetWarning(textIsEmpty, R.ErrorMessages.EmptyNameError);

		}

		private void SetWarning(bool show, string message = ""){
			m_WarningLabel.text = message;
			m_WarningContainer.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
		}

		private void ClearTool()
		{
			m_ProgressBar.style.display = DisplayStyle.None;
			m_PackageNameInputField.SetValueWithoutNotify("");
		}

		private void ForceRefresh(){
			AssetDatabase.Refresh();
		}

		/*/// <summary>
		/// Returns the list of dependency data for a given enum string.
		/// Enum string can be any enum of type ExperienceTag, Platform, or RenderingPipeline
		/// but it needs to be converted to string before being passed in as a parameter.
		/// If the enumString is empty, it returns the list of common dependency.
		/// </summary>
		/// <param name="enumString"></param>
		/// <returns></returns>
		private PackageData.DependencyData[] GetDependencies(string enumString = "")
		{
			if (String.IsNullOrEmpty(enumString))
			{
				enumString = PackageData.Dependency.Common.ToString();
			}
			
			if (Enum.IsDefined(typeof(PackageData.Dependency), enumString))
			{
				var depList = R.Dependencies.DependencyDatas[
					(PackageData.Dependency)Enum.Parse(typeof(PackageData.Dependency), enumString)];
				return depList;
			}

			return null;
		}*/

		
		// TODO: Confirm these functions are needed
		// Example functions to grab package display name from domain name
		private void SearchPackage(string name)
		{
			m_SearchReq = Client.Search(name);
			EditorApplication.update += SearchPackageHandle;
		}

		private void SearchPackageHandle()
		{
			if (m_SearchReq != null && m_SearchReq.IsCompleted)
			{
				
				if (m_SearchReq.Status == StatusCode.Success)
				{
					m_Dependencies.Add(m_SearchReq.Result[0].displayName);
				}
				else
				{
					// Couldn't find the package from registry
				}
				m_SearchReq = null;
			}
		}
    }
}