using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grimoire.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private int selectedIndex;
        public int SelectedIndex {
            get => selectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedIndex, value);
                IsButtonEnabled = (selectedIndex == -1) ? false : true;
            }
        }
        public bool IsButtonEnabled { get; set; }
    }
}
