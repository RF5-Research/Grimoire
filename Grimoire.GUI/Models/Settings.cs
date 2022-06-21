using System.Collections.ObjectModel;

namespace Grimoire.GUI.Models
{
    public class Settings
    {
        public bool LoadLastProject { get; set; }
        public ObservableCollection<Project> Projects { get; set; }

        public Settings()
        {
            Projects = new();
        }
    }
}
