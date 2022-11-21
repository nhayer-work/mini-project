namespace MiniProject.Core.Editor.Utilities
{
    public abstract class FileWriterBase
    {
        protected abstract void TryCreateFile(in string filePath, in string fileContents);
        protected abstract void TryUpdateFile(in string filePath, in string fileContents);
    }
}