using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
        URP,
        HDRP
    }

    // TODO: Move to using lists instead of dictionary
    private static Dictionary<ExperienceTag, string> _experienceTags = new Dictionary<ExperienceTag, string>();
    private static Dictionary<RenderingPipeline, string> _renderingPipelines = 
        new Dictionary<RenderingPipeline, string>();

    /// <summary>
    /// Dictionary mapping ExperienceTag enum to string values
    /// </summary>
    public static Dictionary<ExperienceTag, string> ExperienceTags
    {
        get
        {
            if (_experienceTags.Count == 0)
            {
                foreach (ExperienceTag tag in System.Enum.GetValues(typeof(ExperienceTag)))
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

    /// <summary>
    /// Dictionary mapping Platform enum to string values
    /// </summary>
    public Dictionary<Platform, string> Platforms { get; set; }

    /// <summary>
    /// Dictionary mapping UnityVersion enum to string values
    /// </summary>
    public Dictionary<UnityVersion, string> UnityVersions { get; set; }

    /// <summary>
    /// Dictionary mapping RenderingPipeline enum to string values
    /// </summary>
    public static Dictionary<RenderingPipeline, string> RenderingPipelines
    {
        get
        {
            if (_renderingPipelines.Count == 0)
            {
                _renderingPipelines.Add(RenderingPipeline.URP, "Universal Render Pipeline");
                _renderingPipelines.Add(RenderingPipeline.HDRP, "High Definition Render Pipeline");
                return _renderingPipelines;
            }
            else
            {
                return _renderingPipelines;
            }
        }
    }

    public string Name { get; set; }
    public bool HasEditorFolder { get; set; }
    public bool HasSamples { get; set; }

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
            info += $"\t{kvp.Value}\n";
        }
        Debug.Log(info);
        
        info = "PLATFORMS: \n";
        foreach (var kvp in Platforms)
        {
            info += $"\t{kvp.Value}\n";
        }
        Debug.Log(info);
        
        info = "SUPPORTED UNITY VERSIONS: \n";
        foreach (var kvp in UnityVersions)
        {
            info += $"\t{kvp.Value}\n";
        }
        
        info = "SUPPORTED RENDERING PIPELINES: \n";
        foreach (var kvp in RenderingPipelines)
        {
            info += $"\t{kvp.Value}\n";
        }
        Debug.Log(info);
    }
}
