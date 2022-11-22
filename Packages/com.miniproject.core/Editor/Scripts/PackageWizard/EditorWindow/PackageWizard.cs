using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class PackageWizard : EditorWindow
{
    private const string pathToUXML =
        "Packages/com.miniproject.core/Editor/Scripts/PackageWizard/EditorWindow/PackageWizard.uxml";
    private const string pathToUSS = 
        "Packages/com.miniproject.core/Editor/Scripts/PackageWizard/EditorWindow/PackageWizard.uss";
    private const string experienceTagsFieldName = "ExperienceTags";
    private const string platformOptionsPlaceholderFieldName = "PlatformOptionsPlaceholder";
    private const string platformOptionsFieldName = "PlatformOptions";
    private const string renderingPipelineFieldName = "RenderPipeline";
    private const string unityEditorVersionFieldName = "UnityEditorVersion";

    [MenuItem("Window/Package Wizard")]
    public static void Init()
    {
        PackageWizard wnd = GetWindow<PackageWizard>();
        wnd.titleContent = new GUIContent("PackageWizard");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        
        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(pathToUXML);
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);
        
        // Import USS
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(pathToUSS);
        /*VisualElement labelWithStyle = new Label("Hello! With Style");
        labelWithStyle.styleSheets.Add(styleSheet);
        root.Add(labelWithStyle);*/
        
        VisualElement tagsGroup = root.Q<GroupBox>(experienceTagsFieldName);
        foreach (var tag in Enum.GetValues(typeof(PackageData.ExperienceTag)))
        {
            var toggleItem = new Toggle(tag.ToString());
            tagsGroup.Add(toggleItem);
        }

        VisualElement platformOptionsPlaceholder = root.Q<VisualElement>(platformOptionsPlaceholderFieldName);
        EnumFlagsField platformOptions = new EnumFlagsField(platformOptionsFieldName);
        foreach (PackageData.Platform platformType in (PackageData.Platform[]) Enum.GetValues(typeof(PackageData.Platform)))
        {
            platformOptions.Init(platformType);
        }
        platformOptionsPlaceholder.Add(platformOptions);

        EnumField renderPipeline = root.Q<EnumField>(renderingPipelineFieldName);
        foreach (PackageData.RenderingPipeline renderPipelineType in (PackageData.RenderingPipeline[]) Enum.GetValues(typeof(PackageData.RenderingPipeline)))
        {
            renderPipeline.Init(renderPipelineType);
        }
        // default value
        //renderPipeline.value = PackageData.RenderingPipeline.URP;

        DropdownField editorVersion = root.Q<DropdownField>(unityEditorVersionFieldName);
        List<string> versions = new List<string>();
        foreach (var version in Enum.GetValues(typeof(PackageData.UnityVersion)))
        {
            versions.Add(version.ToString());
        }
        editorVersion.choices = versions;
        
    }
}