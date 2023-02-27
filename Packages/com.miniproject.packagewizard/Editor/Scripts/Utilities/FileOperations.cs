using System.IO;
using UnityEngine;

namespace MiniProject.Core.Editor.Utilities
{
    public class FileOperations
    {
        /// <summary>
        /// Moves the specified file to destination path.
        /// </summary>
        /// <param name="sourcePath">Path of file to move.</param>
        /// <param name="destinationPath">Path of destination.</param>
        public static bool Move(string sourcePath, string destinationPath)
        {
            if (!File.Exists(sourcePath))
            {
                Debug.LogError($"{sourcePath} not found");
                return false;
            }
            File.Move(sourcePath, destinationPath);
            return true;
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="filePath">The name of the file to be deleted.</param>
        public static bool Delete(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"{filePath} not found");
                return false;
            }

            File.Delete(filePath);
            return true;
        }

        public static void Create(in string filePath, in string fileContents)
        {
            //todo check if Path exists
            File.WriteAllText(filePath, fileContents);
        }
    }
}