using Grimoire.Models.UnityEngine;
using System.ComponentModel;

namespace GrimoireGUI.Models
{
    public class Project : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string ROMPath { get; set; }
        public string ProjectPath { get; set; }
        public SystemLanguage GameLanguage { get; set; }

        public Project(string name, string romPath, string projectPath, SystemLanguage gameLanguage)
        {
            Name = name;
            ROMPath = romPath;
            ProjectPath = projectPath;
            GameLanguage = gameLanguage;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
