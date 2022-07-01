using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Grimoire.GUI.ViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;

namespace Grimoire.GUI.Views
{
    public partial class AssetsWindow : Window
    {

        public AssetsWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = new AssetsWindowViewModel();
        }
    }
}
