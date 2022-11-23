using System.IO;
using UnityEngine;

namespace MiniProject.Core.Editor.Utilities
{
    public class DirectoryOperations
    {
        public bool CreateFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Debug.LogError($"{path} already exists");
                return false;
            }


            Directory.CreateDirectory(path);
            return true;
        }
        
        public bool DeleteFolder(string path)
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