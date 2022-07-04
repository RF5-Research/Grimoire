using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;

namespace GrimoireGUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        [Reactive] private int SelectedIndex { get; set; }
        private bool IsButtonEnabled { get; set; }

        public MainWindowViewModel()
        {
            this.WhenAnyValue(x => x.SelectedIndex, (index) => index != -1)
                .Subscribe(SetButtonState);
        }

        private void SetButtonState(bool enable) => IsButtonEnabled = enable;
    }
}
