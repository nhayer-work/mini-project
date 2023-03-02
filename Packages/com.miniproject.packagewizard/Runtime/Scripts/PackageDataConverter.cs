using System.Collections.Generic;

namespace MiniProject.PackageWizard
{
    public static class PackageDataConverter
    {
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