using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.IO;

namespace GrimoireGUI.ViewModels
{
    public class ProjectSettingsViewModel : ViewModelBase
    {
        [Reactive] private int PlatformSelectedIndex { get; set; } = -1;
        [Reactive] private int GameLanguageSelectedIndex { get; set; } = -1;
        [Reactive] private string ROMPathText { get; set; }
        [Reactive] private string ProjectPathText { get; set; }
        [ObservableAsProperty] private bool IsButtonEnabled { get; set; }

        public ProjectSettingsViewModel()
        {
            this.WhenAnyValue(
                x => x.PlatformSelectedIndex, x => x.GameLanguageSelectedIndex, x => x.ROMPathText, x => x.ProjectPathText,
                (platform, gameLanguage, romPath, projectPath) => platform != -1 &&
                gameLanguage != -1 &&
                romPath != null &&
                Directory.Exists(romPath) &&
                projectPath != null &&
                Path.IsPathFullyQualified(projectPath)
                )
                .ToPropertyEx(this, x => x.IsButtonEnabled);
        }
    }
}
