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
        public string GamePath { get; set; }
        public string ProjectPath { get; set; }

        public Project(string name, Platform platform, SystemLanguage gameLanguage, string gamePath, string projectPath)
        {
            Name = name;
            Platform = platform;
            GameLanguage = gameLanguage;
            GamePath = gamePath;
            ProjectPath = projectPath;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
