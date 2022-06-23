using Avalonia.Controls;
using Grimoire.GUI.ViewModels;

namespace Grimoire.GUI.Views
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
