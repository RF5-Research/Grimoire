using GrimoireGUI.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;

namespace GrimoireGUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IActivatableViewModel
    {
        [Reactive] private ObservableCollection<Project> Projects { get; set; }
        [Reactive] private int SelectedIndex { get; set; }
        [ObservableAsProperty] private bool IsButtonEnabled { get; set; }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public MainWindowViewModel(ObservableCollection<Project> projects)
        {
            Projects = projects;
            this.WhenAnyValue(x => x.SelectedIndex, (index) => index != -1)
                .ToPropertyEx(this, x => x.IsButtonEnabled);
        }
    }
}
