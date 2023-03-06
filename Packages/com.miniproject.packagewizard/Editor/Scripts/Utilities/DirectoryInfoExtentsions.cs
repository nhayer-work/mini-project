using System;
using System.IO;

namespace MiniProject.Core.Editor.Utilities
{
    //Based On: https://stackoverflow.com/a/23697173
    public static class DirectoryInfoExtentsions
    {
        public static string GetRelativePathFrom<T>(this T to, T from) where T: FileSystemInfo
        {
            return from.GetRelativePathTo(to);
        }

        public static string GetRelativePathTo<T>(this T from, T to) where T: FileSystemInfo
        {
            string GetPath(FileSystemInfo fsi)
            {
                if (fsi is DirectoryInfo d)
                {
                    return d.FullName.TrimEnd('\\') + "\\";
                }

                return fsi.FullName;
            }

            var fromPath = GetPath(from);
            var toPath = GetPath(to);

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}