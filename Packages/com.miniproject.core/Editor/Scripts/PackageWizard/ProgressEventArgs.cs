using System;

namespace MiniProject.Core.Editor.PackageWizard
{
    public class ProgressEventArgs : EventArgs
    {
        public string Info { get; }
        public float Progress { get; }

        public ProgressEventArgs(string text, float progress)
        {
            Info = text;
            Progress = progress;
        }
    }
}