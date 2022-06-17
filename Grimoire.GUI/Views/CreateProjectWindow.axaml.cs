using Avalonia;
using Avalonia.Controls;
using Grimoire.GUI.Models;
using PropertyChanged;
using System.IO;

namespace Grimoire.GUI.Views
{
    [DoNotNotify]
    public partial class CreateProjectWindow : Window
    {
        private new MainWindow Parent;
        
        public CreateProjectWindow(MainWindow parent)
        {
            Setup();
            Parent = parent;
        }

        public CreateProjectWindow()
        {
            Setup();
        }

        private void Setup()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            ROMPathBrowseButton.Click += ROMPathBrowseButton_Click;
            ProjectPathBrowseButton.Click += ProjectPathBrowseButton_Click;
            CancelButton.Click += CancelButton_Click;
            CreateButton.Click += CreateButton_Click;
        }
        private void CreateButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!Directory.Exists(ProjectPathTextBox.Text))
                Directory.CreateDirectory(ProjectPathTextBox.Text);

            Parent.ProjectSettings.Add(
                new ProjectSettings(NameTextBox.Text, ROMPathTextBox.Text, ProjectPathTextBox.Text)
            );
            Parent.Close();
        }

        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        private async void ROMPathBrowseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            ROMPathTextBox.Text = await dialog.ShowAsync(this);
        }

        private async void ProjectPathBrowseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            ProjectPathTextBox.Text = await dialog.ShowAsync(this);
        }
    }
}
