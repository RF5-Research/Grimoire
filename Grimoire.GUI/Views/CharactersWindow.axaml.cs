using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Grimoire.GUI.ViewModels;

namespace Grimoire.GUI.Views
{
    public partial class CharactersWindow : Window
    {
        public CharactersWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = new CharactersWindowViewModel();
            Opened += CharactersWindow_Opened;
            Closing += CharactersWindow_Closing;
            var image = new Image();
            //TODO: Automatically generate TextBlocks for each field
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
