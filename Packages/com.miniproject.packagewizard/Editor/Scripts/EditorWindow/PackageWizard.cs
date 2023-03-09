using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

	    //Properties
	    //================================================================================================================//

	    #region Properties

	    private PackageData m_PackageData;
	    
	    private TextInputBaseField<string> m_PackageNameInputField;
	    private Button m_RefreshButton;
	    private ScrollView m_FoldoutTags;
	    private Toggle[] m_TagToggles;
	    private Toggle m_UsesEditorToggle;
	    private Toggle m_UsesScoreToggle;
	    private DropdownField m_MinEditorVersion;
	    private EnumField m_RenderPipeline;

	    //Package Save location
	    private TextInputBaseField<string> m_PackageLocationField;

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
	    private ScrollView m_ScrollviewDependencies;

	    [FormerlySerializedAs("_customDependencies")] [SerializeField]
	    private List<PackageData.DependencyData> customDependencies;
	    private Dictionary<Toggle,List<Toggle>> m_DependencyToToggle;
	    private Dictionary<Toggle, PackageData.DependencyData> m_ToggleToDependencyData;
		
	    //Project selection
	    private ScrollView m_ScrollviewProjects;
	    private Dictionary<Toggle, PackageWizardSettingsScriptableObject.ProjectInfo> m_ToggleToProjectInfo;

	    #endregion //Properties

	    //EditorWindow Functions
		//================================================================================================================//

		[MenuItem("Mini Project/Package Wizard")]
        public static void Init()
        {
	        s_Window = null;
	        s_Window = GetWindow<PackageWizard>();
	        s_Window.titleContent = new GUIContent(R.UI.Title);
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(R.UI.PathToUxml);
            VisualElement labelFromUxml = visualTree.Instantiate();
            root.Add(labelFromUxml);
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(R.UI.PathToUSS);
            root.styleSheets.Add(styleSheet);

			GetReferences(root);

			//Package Save Location
			//----------------------------------------------------------//

			CreatePackageSaveLocationUI(ref root);
			
			//Experience Tags
			//----------------------------------------------------------//

			CreatePackageTagsUI();

            //Setup Render Pipeline selection Enum
            //----------------------------------------------------------//
            CreateRenderPipelineUI();
            
            //Min Version
            //----------------------------------------------------------//
            CreateMinVersionUI();

	        //Project Selection Setup
			//----------------------------------------------------------//
			CreateProjectSelectionUI();
			
			//Dependency Setup
			//----------------------------------------------------------//
			CreateDependenciesUI();
			
			//Custom Dependencies
			//----------------------------------------------------------//
			CreateCustomDependenciesUI(ref root);
			
			//----------------------------------------------------------//

			SubscribeEvents();
			ClearTool();
            HandleGenerateButtonState();
			SetWarning(false);
        }

        //Create Window UI
        //================================================================================================================//

        #region Create Window UI

        private void CreatePackageSaveLocationUI(ref VisualElement root)
        {
	        //TODO Fix potential issue of user storing the project in C:\
	        m_PackageLocationField.value = new DirectoryInfo(Application.dataPath).Parent?.Parent?.FullName.Replace("\\","/");
	        var packageLocationButton = root.Q<Button>(R.UI.PackageLocationButton);
	        packageLocationButton.clicked += () =>
	        {
		        m_PackageLocationField.value = EditorUtility.OpenFolderPanel(
			        "Save Location for Package",
			        Application.dataPath,
			        "");
	        };
        }
        private void CreatePackageTagsUI()
        {
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
        }
        private void CreateRenderPipelineUI()
        {
	        foreach (Enum renderPipelineType in Enum.GetValues(typeof(PackageData.RenderingPipeline)))
	        {
		        m_RenderPipeline.Init(renderPipelineType);
	        }

	        m_RenderPipeline.value = PackageData.RenderingPipeline.BuiltIn;
        }
        private void CreateMinVersionUI()
        {
	        var versions = R.MinVersions.UnityVersions;
	        m_MinEditorVersion.choices = versions.ToList();
	        m_MinEditorVersion.value = versions[0];
	        
	        //TODO Disable/deselect projects based on the min version selected
	        m_MinEditorVersion.RegisterCallback<ChangeEvent<string>>(e =>
	        {
		        var selectedMinVersion = UnityVersionToVersion(e.newValue);
		        foreach (var projectInfo in m_ToggleToProjectInfo)
		        {
			        var projectVersion = UnityVersionToVersion(projectInfo.Value.unityVersion);

			        var isSelectable = projectVersion >= selectedMinVersion;
			        
			        projectInfo.Key.SetEnabled(isSelectable);
			        
			        if(isSelectable)
				        continue;

			        projectInfo.Key.SetValueWithoutNotify(false);
		        }
	        });
        }
        private void CreateProjectSelectionUI()
        {
	        m_ToggleToProjectInfo = new Dictionary<Toggle, PackageWizardSettingsScriptableObject.ProjectInfo>();
	        var projectDirectories = R.Projects.GroupedProjectDirectories;

	        //todo Group By Editor Versions
	        foreach (var groupedProject in projectDirectories)
	        {
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
			        /*newProjectToggle.RegisterCallback<ChangeEvent<bool>>(
				        e => newProjectToggle.value = e.newValue);*/
		        }

		        //----------------------------------------------------------//
		        //Add GroupBox to Dependency scroll view
		        m_ScrollviewProjects.Add(newSectionGroupBox);
	        }
        }
        private void CreateDependenciesUI()
        {
	        m_DependencyToToggle = new Dictionary<Toggle, List<Toggle>>();
	        m_ToggleToDependencyData = new Dictionary<Toggle, PackageData.DependencyData>();
	        foreach (var (dependencyName, dependencyDatas) in R.Dependencies.DependencyDatas)
	        {
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
		        foreach (var packageData in dependencyDatas)
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
        private void CreateCustomDependenciesUI(ref VisualElement root)
        {
	        var customDependenciesListView = root.Q<ListView>(R.UI.CustomDependenciesListView);
	        SerializedObject serializedObject = new SerializedObject(this);
	        SerializedProperty serializedPropertyCustomDependencies = serializedObject.FindProperty(nameof(customDependencies));
	        customDependenciesListView.BindProperty(serializedPropertyCustomDependencies);
        }

        #endregion //Create Window UI

        //Window Setup
        //================================================================================================================//
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

			m_ScrollviewDependencies = root.Q<ScrollView>(R.UI.DependenciesScrollview);
			m_ScrollviewProjects = root.Q<ScrollView>(R.UI.ProjectsScrollview);
			
			m_MinEditorVersion = root.Q<DropdownField>(R.UI.UnityEditorVersionFieldName);
			m_RenderPipeline = root.Q<EnumField>(R.UI.RenderingPipelineFieldName);
			m_PackageLocationField = root.Q<TextInputBaseField<string>>(R.UI.PackageLocationInputField);
		}

		private void SubscribeEvents()
		{
			m_GenerateButton.RegisterCallback<ClickEvent>((e) => GenerateButtonClicked());
			m_ClearButton.RegisterCallback<ClickEvent>((e) => ClearTool());
			m_PackageNameInputField.RegisterCallback<ChangeEvent<string>>((e) => HandleGenerateButtonState());
			m_RefreshButton.RegisterCallback<ClickEvent>((e) => ForceRefresh());
		}

        //Callbacks
        //================================================================================================================//

        #region Callbacks

        private void GenerateButtonClicked()
        {
	        m_ProgressBar.style.display = DisplayStyle.Flex;

	        m_PackageData = new PackageData
	        {
		        DisplayName = m_PackageNameInputField.text,
		        Path = m_PackageLocationField.text,
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
		        },
		        MinSupportedVersion = m_MinEditorVersion.value,
		        //Get Selected Tags
		        //----------------------------------------------------------//
		        ExperienceTags = new List<PackageData.ExperienceTag>()
	        };

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
		
        private void ClearTool()
        {
	        m_ProgressBar.style.display = DisplayStyle.None;
	        m_PackageNameInputField.SetValueWithoutNotify("");
        }

        private static void ForceRefresh() => AssetDatabase.Refresh();

        #endregion //Callbacks
		
		//Misc
		//================================================================================================================//
		#region Misc Functions

		private void SetWarning(bool show, string message = ""){
			m_WarningLabel.text = message;
			m_WarningContainer.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
		}

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

		//Unity Versions
		//================================================================================================================//

		private static Dictionary<int, Version> s_StoredUnityVersions;
		/// <summary>
		/// Converts a Unity Version string into a usable Version. This is stored within s_StoredUnityVersions to reduce
		/// number of allocations
		/// </summary>
		/// <param name="unityVersion"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		private static Version UnityVersionToVersion(in string unityVersion)
		{
			if (string.IsNullOrWhiteSpace(unityVersion))
				throw new ArgumentNullException();
	        
			if (s_StoredUnityVersions == null)
				s_StoredUnityVersions = new Dictionary<int, Version>();

			var versionHash = unityVersion.GetHashCode();

			if (s_StoredUnityVersions.TryGetValue(versionHash, out var version))
				return version;

			int major = 0;
			int minor = 0;
			int patch = 0;
		        
			var split = unityVersion.Split('.', StringSplitOptions.RemoveEmptyEntries);
			major = int.Parse(split[0]);
	        
			//Check to see if the version provided has the min version number
			if (split.Length > 1)
				minor = int.Parse(split[1]);

			//Check to see if the version provided has the patch/release value
			if (split.Length > 2)
			{
				var regex = new Regex("[0-9]+(?![abc])", RegexOptions.Singleline | RegexOptions.CultureInvariant);
				var match = regex.Match(split[2]);

				patch = int.Parse(match.Value);
			}

			version = new Version(major, minor, patch);
			s_StoredUnityVersions.Add(versionHash, version);

			return version;
		}

		#endregion //Misc Functions
		
		//================================================================================================================//

    }
}