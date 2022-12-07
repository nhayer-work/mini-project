using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Scripts.Core
{
    /// <summary>
    /// Class containing package data
    /// </summary>
    public class PackageData
    {
        // TODO: Confirm the list of experience tags, platforms, and Unity versions 
        public enum ExperienceTag
        {
            Lighting,
            Physics,
            UIToolkit,
            Cinemachine,
            Shaders,
            Networking,
            AR,
            VR,
            MachineLearning,
            NewInputSystem,
            TerrainTools,
        }

        public enum Platform
        {
            Windows = 1 << 1,
            MacOS = 1 << 2,
            Android = 1 << 4,
            iOS = 1 << 8,
            WebGL = 1 << 16,
        }

        public enum UnityVersion
        {
            LTS2021,
            BETA2022,
        }

        public enum RenderingPipeline
        {
            BuiltIn,
            URP,
            HDRP
        }

        public enum Dependency
        {
            Common,
            URP,
            HDRP,
            Android,
            Cinemachine,
            Shaders,
            AR,
            VR,
            MachineLearning,
            NewInputSystem,
            TerrainTools
        }

        public struct Author
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Url { get; set; }
        }

        public struct DependencyData
        {
            public string DisplayName { get; set; }
            public string Domain { get; set; }
            public string Version { get; set; }
            //for com.unity packages, no need to include the source, the version should
            public string Source { get; set; }
        }

        /// <summary>
        /// Dictionary mapping ExperienceTag enum to string values
        /// </summary>
        public List<ExperienceTag> ExperienceTags{ get; set; }

        public List<Platform> Platforms { get; set; }
        
        public List<UnityVersion> UnityVersions { get; set; }

        /// <summary>
        /// Dictionary mapping RenderingPipeline enum to string values
        /// </summary>
        public List<RenderingPipeline> RenderingPipelines{ get; set; }

        public List<DependencyData> Dependencies { get; set; }
        public List<DependencyData> CustomDependencies { get; set; }

        public string DisplayName { get; set; }

        /// <summary>
        /// The officially registered package name
        /// <see href="https://docs.unity3d.com/Manual/upm-manifestPkg.html#name">Link Text</see>
        /// </summary>
        public string Name { get; set; }

        public bool HasEditorFolder { get; set; }
        public bool KeepsScore { get; set; }
        public bool HasSamples { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string AuthorName { get; set; }
        public string RenderPipeline { get; set; }
        public string SelectedTags { get; set; }

        public Author AuthorInfo;

        public string UnityRelease => PackageDataConverter.UnityReleases[UnityVersions[0]];
        public string UnityVersionFormatted => PackageDataConverter.UnityVersions[UnityVersions[0]];

        /// <summary>
        /// Prints all string values of package data
        /// </summary>
        /// <returns></returns>
        [MenuItem("Package Data/Print All String Data")]
        public void PrintAllStringData()
        {
            string info = "EXPERIENCE TAGS: \n";
            foreach (var kvp in ExperienceTags)
            {
                info += $"\t{kvp}\n";
            }

            Debug.Log(info);

            info = "PLATFORMS: \n";
            foreach (var kvp in Platforms)
            {
                info += $"\t{kvp}\n";
            }

            Debug.Log(info);

            info = "SUPPORTED UNITY VERSIONS: \n";
            foreach (var kvp in UnityVersions)
            {
                info += $"\t{kvp}\n";
            }

            info = "SUPPORTED RENDERING PIPELINES: \n";
            foreach (var kvp in RenderingPipelines)
            {
                info += $"\t{kvp}\n";
            }

            Debug.Log(info);
        }
    }
}