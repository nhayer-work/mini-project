using System.IO;
using UnityEngine;

namespace MiniProject.Core.Editor.Utilities
{
    public class DirectoryOperations
    {
        /// <summary>
        /// Creates a directory at a given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool CreateFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Debug.LogError($"{path} already exists");
                return false;
            }

            Directory.CreateDirectory(path);
            return true;
        }
        
        /// <summary>
        /// Delete a directory at a given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool DeleteFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Debug.LogError($"{path} does not exists");
                return false;
            }


            Directory.Delete(path);
            return true;
        }
    }
}