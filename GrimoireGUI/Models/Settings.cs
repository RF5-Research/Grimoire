using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GrimoireGUI.Models
{
    public class Settings : INotifyPropertyChanged
    {
	public bool LoadLastProject { get; set; }
        public ObservableCollection<Project> Projects { get; set; }

        public Settings()
        {
            Projects = new();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
