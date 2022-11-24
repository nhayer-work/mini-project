using MiniProject.Core.Editor.Utilities;

namespace MiniProject.Core.Editor.PackageWizard
{
    public class PackageJsonWriter : FileWriterBase
    {
        protected override void TryCreateFile(in string filePath, in string fileContents)
        {
            FileOperations.Create(filePath, fileContents);
        }

        protected override void TryUpdateFile(in string filePath, in string fileContents)
        {
            throw new System.NotImplementedException();
        }

        public void Generate()
        {
            
        }
    }
}