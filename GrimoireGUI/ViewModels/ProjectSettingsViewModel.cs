using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using System.IO;
using System.Reactive;

namespace GrimoireGUI.ViewModels
{
    public class ProjectSettingsViewModel : ReactiveValidationObject
    {
        [Reactive] private int PlatformSelectedIndex { get; set; } = -1;
        [Reactive] private int GameLanguageSelectedIndex { get; set; } = -1;
        [Reactive] private string GamePathText { get; set; }
        [Reactive] private string ProjectPathText { get; set; }
        [ObservableAsProperty] private bool IsButtonEnabled { get; set; }

        public ReactiveCommand<Unit, Unit> Finish { get; }
        public ProjectSettingsViewModel()
        {
            this.ValidationRule(
                vm => vm.PlatformSelectedIndex,
                platform => platform != -1,
                "A target platform is required");

            this.ValidationRule(
                vm => vm.GameLanguageSelectedIndex,
                gameLanguage => gameLanguage != -1,
                "A game language is required");

            this.ValidationRule(
                vm => vm.GamePathText,
                gamePath => gamePath != null && Directory.Exists(gamePath),
                "A valid game directory is required:\nromFS directory for Switch; game directory containing the executable for Steam");

            this.ValidationRule(
                vm => vm.ProjectPathText,
                projectPath => projectPath != null && Path.IsPathFullyQualified(projectPath),
                "A valid export directory is required");

            this.IsValid().ToPropertyEx(this, x => x.IsButtonEnabled);
        }
    }
}
