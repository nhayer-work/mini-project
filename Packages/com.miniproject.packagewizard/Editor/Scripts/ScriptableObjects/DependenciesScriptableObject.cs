using System;
using System.Collections.Generic;
using System.Linq;
using MiniProject.PackageWizard.EditorWindow;
using UnityEngine;

namespace MiniProject.PackageWizard.ScriptableObjects
{
    //[MenuItem("Mini Project/Package Wizard/New Package")]
    [CreateAssetMenu(fileName = "Package Dependency Collection", menuName = "Mini Project/Package Wizard/Dependency Collection", order = 1)]
    public class DependenciesScriptableObject : ScriptableObject
    {
        [Serializable]
        private class DepedencyDataGroup
        {
            public string name;
            [NonReorderable]
            public DependencyData[] dependencies;
        }
        
        [Serializable]
        private struct DependencyData
        {
            public string DisplayName;
            public string Domain;
            public string Version;
            //for com.unity packages, no need to include the source, the version should
            public string Source;

            public DependencyData(PackageData.DependencyData dependencyData)
            {
                DisplayName = dependencyData.DisplayName;
                Domain = dependencyData.Domain;
                Version = dependencyData.Version;
                Source = dependencyData.Source;
            }
            public PackageData.DependencyData GetPackageDependencyData()
            {
                return new PackageData.DependencyData
                {
                    DisplayName = DisplayName,
                    Domain = Domain,
                    Version = Version,
                    Source = Source
                };
            }
        }

        //================================================================================================================//

        public IReadOnlyDictionary<string, PackageData.DependencyData[]> Dependencies
        {
            get
            {
                if (_dependencies == null)
                    GenerateDependencyData();

                return _dependencies;
            }
        }

        private Dictionary<string, PackageData.DependencyData[]> _dependencies;

        [SerializeField, NonReorderable]
        private DepedencyDataGroup[] depedencyDataGroups;
        
        //================================================================================================================//

        private void GenerateDependencyData()
        {
            _dependencies = new Dictionary<string, PackageData.DependencyData[]>();
            foreach (var depedencyDataGroup in depedencyDataGroups)
            {
                _dependencies.Add(depedencyDataGroup.name,
                    depedencyDataGroup.dependencies.Select(x => x.GetPackageDependencyData()).ToArray());
            }
        }
        
        //================================================================================================================//

        /*[ContextMenu("Get From R")]
        private void GetFromR()
        {
            
            var kvps = R.Dependencies.DependencyDatas;
            depedencyDataGroups = new DepedencyDataGroup[kvps.Count];
            var i = 0;
            foreach (var kvp in kvps)
            {
                depedencyDataGroups[i] = new DepedencyDataGroup()
                {
                    name = kvp.Key.ToString(),
                    dependencies = kvp.Value.Select(x => new DependencyData(x)).ToArray()
                };
                
                i++;
            }
        }*/
    }
}