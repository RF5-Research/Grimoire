using Grimoire;
using Grimoire.Models.UnityEngine;
using System.ComponentModel;
using static GrimoireGUI.Models.Settings;

namespace GrimoireGUI.Models
{
    public class Project : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public Platform Platform { get; set; }
        public SystemLanguage GameLanguage { get; set; }
        public string ROMPath { get; set; }
        public string ProjectPath { get; set; }

        public Project(string name, Platform platform, SystemLanguage gameLanguage, string romPath, string projectPath)
        {
            Name = name;
            Platform = platform;
            GameLanguage = gameLanguage;
            ROMPath = romPath;
            ProjectPath = projectPath;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
