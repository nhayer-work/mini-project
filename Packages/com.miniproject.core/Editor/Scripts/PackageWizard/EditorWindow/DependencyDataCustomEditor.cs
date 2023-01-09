using Scripts.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MiniProject.Core.Editor.PackageWizard.EditorWindow
{
    [CustomPropertyDrawer(typeof(PackageData.DependencyData))]
    public class DependencyDataCustomEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create a new VisualElement to be the root of our inspector UI
            VisualElement myInspector = new VisualElement();
            // Load and clone a visual tree from UXML
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(R.UI.DependencyData.UXMLPath);
            visualTree.CloneTree(myInspector);

            var groupBox = myInspector.Q<Foldout>(R.UI.DependencyData.GroupBoxName);
            
            var foldoutLabel = groupBox
                .Q<Label>(className: Foldout.textUssClassName);
            var displayNameProperty = property.FindPropertyRelative(nameof(PackageData.DependencyData.DisplayName));
            foldoutLabel.BindProperty(displayNameProperty);

            
            var displayNameTextField = groupBox.Q<TextField>(R.UI.DependencyData.DisplayNameField);
            displayNameTextField.BindProperty(displayNameProperty);
            var domainNameTextField = groupBox.Q<TextField>(R.UI.DependencyData.DomainNameField);
            domainNameTextField.BindProperty(property.FindPropertyRelative(nameof(PackageData.DependencyData.Domain)));
            var versionTextField = groupBox.Q<TextField>(R.UI.DependencyData.VersionField);
            versionTextField.BindProperty(property.FindPropertyRelative(nameof(PackageData.DependencyData.Version)));
            var sourceTextField = groupBox.Q<TextField>(R.UI.DependencyData.SourceField);
            sourceTextField.BindProperty(property.FindPropertyRelative(nameof(PackageData.DependencyData.Source)));

            // Return the finished inspector UI
            return myInspector;
        }
    }
}