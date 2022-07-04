using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.IO;
using System.Reactive.Linq;

namespace GrimoireGUI.ViewModels
{
    public class ProjectCreateWindowViewModel : ViewModelBase
    {
        private bool IsButtonEnabled { get; set; }
        [Reactive] private int SelectedIndex { get; set; } = -1;
        [Reactive] private string ROMPathText { get; set; }
        [Reactive] private string ProjectPathText { get; set; }

        public ProjectCreateWindowViewModel()
        {
            this.WhenAnyValue(
                x => x.SelectedIndex, x => x.ROMPathText, x => x.ProjectPathText,
                (selectedIndex, romPath, projectPath) => selectedIndex != -1 &&
                romPath != null &&
                Directory.Exists(romPath) &&
                projectPath != null &&
                Path.IsPathFullyQualified(projectPath))
                .DistinctUntilChanged()
                .Subscribe(SetButtonState);
        }
        private void SetButtonState(bool enable) => IsButtonEnabled = enable;
    }
}
