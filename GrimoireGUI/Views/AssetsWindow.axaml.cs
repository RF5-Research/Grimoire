using Avalonia;
using Avalonia.Controls;
using GrimoireGUI.ViewModels;

namespace GrimoireGUI.Views
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
