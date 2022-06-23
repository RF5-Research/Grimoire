using Avalonia;
using Avalonia.Controls;

namespace Grimoire.GUI.Views
{
    public partial class ProjectMainWindow : Window
    {
        public ProjectMainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            ScriptEditorButton.Click += ScriptEditorButton_Click;
            CloseProjectMenuItem.Click += CloseProjectMenuItem_Click;
        }

        private void CloseProjectMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }

        private void ScriptEditorButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = new AdvScriptWindow();
            window.Show();
        }
    }
}
