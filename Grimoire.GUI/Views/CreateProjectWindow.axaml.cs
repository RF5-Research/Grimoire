using Avalonia;
using Avalonia.Controls;
using Grimoire.GUI.Models;
using PropertyChanged;
using System.IO;

namespace Grimoire.GUI.Views
{
    public partial class CreateProjectWindow : Window
    {        
        public CreateProjectWindow()
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

            var project = new Project(NameTextBox.Text, ROMPathTextBox.Text, ProjectPathTextBox.Text);
            ((MainWindow)Owner)!.Projects.Add(project);
            _ = ((MainWindow)Owner).OpenProject(project);
            Close();
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
