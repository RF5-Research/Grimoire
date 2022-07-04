using Avalonia;
using Avalonia.Controls;
using GrimoireGUI.ViewModels;

namespace GrimoireGUI.Views
{
    public partial class CharactersWindow : Window
    {
        public CharactersWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Opened += CharactersWindow_Opened;
            Closing += CharactersWindow_Closing;
            DataContext = new CharactersWindowViewModel();
        }

        private void CharactersWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!((ProjectMainWindow)Owner).IsClosingProject)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void CharactersWindow_Opened(object? sender, System.EventArgs e)
        {
            ((ProjectMainWindow)Owner).Save += CharactersWindow_Save;
        }

        private void CharactersWindow_Save(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            //Implement what objects to write
            throw new System.NotImplementedException();
        }
    }
}
