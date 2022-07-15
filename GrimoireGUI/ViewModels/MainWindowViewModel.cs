using GrimoireGUI.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace GrimoireGUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        [Reactive] private ObservableCollection<Project> Projects { get; set; }
        [Reactive] private int SelectedIndex { get; set; }
        [ObservableAsProperty] private bool IsButtonEnabled { get; set; }

        public ReactiveCommand<Unit, Unit> Command { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public MainWindowViewModel(ObservableCollection<Project> projects)
        {
            Projects = projects;
            this.WhenAnyValue(x => x.SelectedIndex, (index) => index != -1)
                .ToPropertyEx(this, x => x.IsButtonEnabled);
        }
    }
}
