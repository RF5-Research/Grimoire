using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.IO;

namespace GrimoireGUI.ViewModels
{
    public class ProjectSettingsViewModel : ViewModelBase
    {
        [Reactive] private int SelectedIndex { get; set; } = -1;
        [Reactive] private string ROMPathText { get; set; }
        [Reactive] private string ProjectPathText { get; set; }
        [ObservableAsProperty] private bool IsButtonEnabled { get; set; }

        public ProjectSettingsViewModel()
        {
            this.WhenAnyValue(
                x => x.SelectedIndex, x => x.ROMPathText, x => x.ProjectPathText,
                (selectedIndex, romPath, projectPath) => selectedIndex != -1 &&
                romPath != null &&
                Directory.Exists(romPath) &&
                projectPath != null &&
                Path.IsPathFullyQualified(projectPath)
                )
                .ToPropertyEx(this, x => x.IsButtonEnabled);
        }
    }
}
