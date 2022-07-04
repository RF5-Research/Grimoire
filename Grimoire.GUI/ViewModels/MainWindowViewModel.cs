using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;

namespace Grimoire.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        [Reactive] public int SelectedIndex { get; set; }
        public bool IsButtonEnabled { get; set; }

        public MainWindowViewModel()
        {
            this.WhenAnyValue(x => x.SelectedIndex)
                .Subscribe(ButtonState);
        }

        public void ButtonState(int _) => IsButtonEnabled = (SelectedIndex == -1) ? false : true;
    }
}
