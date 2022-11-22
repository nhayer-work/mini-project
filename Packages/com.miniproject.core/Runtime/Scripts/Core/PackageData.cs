using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Class containing package data
/// </summary>
public static class PackageData
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
        Windows,
        MacOS,
        Android,
        iOS,
        WebGL,
    }

    public enum UnityVersion
    {
        LTS2021,
        BETA2022,
    }

    // TODO: Move to using lists instead of dictionary
    private static Dictionary<ExperienceTag, string> _experienceTags = new Dictionary<ExperienceTag, string>();
    private static Dictionary<Platform, string> _platforms = new Dictionary<Platform, string>();
    private static Dictionary<UnityVersion, string> _unityVersions = new Dictionary<UnityVersion, string>();

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
    public static Dictionary<Platform, string> Platforms
    {
        get
        {
            if (_platforms.Count == 0)
            {
                foreach (Platform platform in System.Enum.GetValues(typeof(Platform)))
                {
                    _platforms.Add(platform, platform.ToString());
                }

                return _platforms;
            }
            else
            {
                return _platforms;
            }
        }
    }

    /// <summary>
    /// Dictionary mapping UnityVersion enum to string values
    /// </summary>
    public static Dictionary<UnityVersion, string> UnityVersions
    {
        get
        {
            if (_unityVersions.Count == 0)
            {
                _unityVersions.Add(UnityVersion.LTS2021, "2021.3.12f1-lts");
                _unityVersions.Add(UnityVersion.BETA2022, "2022.2.0b13-beta");
                return _unityVersions;
            }
            else
            {
                return _unityVersions;
            }
        }
    }

    /// <summary>
    /// Prints all string values of package data
    /// </summary>
    /// <returns></returns>
    [MenuItem("Package Data/Print All String Data")]
    public static void PrintAllStringData()
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
        Debug.Log(info);
    }
}
