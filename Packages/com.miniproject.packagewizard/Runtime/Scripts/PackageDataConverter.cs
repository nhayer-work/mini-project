using System.Collections.Generic;

namespace Scripts.Core
{
    public static class PackageDataConverter
    {
        private static Dictionary<PackageData.UnityVersion, string> _unityVersions = new();

        /// <summary>
        /// Dictionary mapping UnityVersion enum to string values
        /// </summary>
        public static Dictionary<PackageData.UnityVersion, string> UnityVersions
        {
            get
            {
                if (_unityVersions.Count != 0) return _unityVersions;
                _unityVersions.Add(PackageData.UnityVersion.LTS2021, "2021.3");
                _unityVersions.Add(PackageData.UnityVersion.BETA2022, "2022.2");
                return _unityVersions;
            }
        }

        private static Dictionary<PackageData.UnityVersion, string> _unityReleases = new();

        /// <summary>
        /// Dictionary mapping UnityVersion enum to string values
        /// </summary>
        public static Dictionary<PackageData.UnityVersion, string> UnityReleases
        {
            get
            {
                if (_unityReleases.Count != 0) return _unityReleases;
                _unityReleases.Add(PackageData.UnityVersion.LTS2021, "12f1");
                _unityReleases.Add(PackageData.UnityVersion.BETA2022, "0b13");
                return _unityReleases;
            }
        }

        private static Dictionary<PackageData.ExperienceTag, string> _experienceTags = new();

        /// <summary>
        /// Dictionary mapping ExperienceTag enum to string values
        /// </summary>
        public static Dictionary<PackageData.ExperienceTag, string> ExperienceTags
        {
            get
            {
                if (_experienceTags.Count == 0)
                {
                    foreach (PackageData.ExperienceTag tag in System.Enum.GetValues(typeof(PackageData.ExperienceTag)))
                    {
                        _experienceTags.Add(tag, tag.ToString());
                    }

                    return _experienceTags;
                }
                else
                {
                    return _experienceTags;
                }
            }
        }

        private static Dictionary<PackageData.RenderingPipeline, string> _renderingPipelines = new();

        /// <summary>
        /// Dictionary mapping RenderingPipeline enum to string values
        /// </summary>
        public static Dictionary<PackageData.RenderingPipeline, string> RenderingPipelines
        {
            get
            {
                if (_renderingPipelines.Count == 0)
                {
                    _renderingPipelines.Add(PackageData.RenderingPipeline.URP, "Universal Render Pipeline");
                    _renderingPipelines.Add(PackageData.RenderingPipeline.HDRP, "High Definition Render Pipeline");
                    return _renderingPipelines;
                }
                else
                {
                    return _renderingPipelines;
                }
            }
        }
    }
}