using System.ComponentModel;

namespace Grimoire.GUI.Models
{
    public class Project : INotifyPropertyChanged
    {
        public string Name { get; set; }
        //public string GameLanguage { get; set; }
        //public string Platform { get; set; }
        public string ROMPath { get; set; }
        public string ProjectPath { get; set; }

        public Project(string name, string romPath, string projectPath)
        {
            Name = name;
            ROMPath = romPath;
            ProjectPath = projectPath;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
