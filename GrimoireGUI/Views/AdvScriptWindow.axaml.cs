using Avalonia.Controls;
using GrimoireGUI.ViewModels;

namespace GrimoireGUI.Views
{
    public partial class AdvScriptWindow : Window
    {
        public AdvScriptWindow()
        {
            InitializeComponent();
            DataContext = new AdvScriptWindowViewModel();
        }
    }
}
