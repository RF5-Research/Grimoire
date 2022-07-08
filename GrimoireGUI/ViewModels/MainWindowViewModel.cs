using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GrimoireGUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        [Reactive] private int SelectedIndex { get; set; }
        [ObservableAsProperty] private bool IsButtonEnabled { get; set; }

        public MainWindowViewModel()
        {
            this.WhenAnyValue(x => x.SelectedIndex, (index) => index != -1)
                .ToPropertyEx(this, x => x.IsButtonEnabled);
        }
    }
}
