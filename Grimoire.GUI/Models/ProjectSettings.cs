using PropertyChanged;
using System.ComponentModel;

namespace Grimoire.GUI.Models
{
    public class ProjectSettings : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string ROMPath { get; set; }
        public string ProjectPath { get; set; }

        public ProjectSettings(string name, string romPath, string projectPath)
        {
            Name = name;
            ROMPath = romPath;
            ProjectPath = projectPath;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
