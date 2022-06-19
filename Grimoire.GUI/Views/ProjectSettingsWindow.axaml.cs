using Avalonia;
using Avalonia.Controls;
using Grimoire.GUI.Models;
using PropertyChanged;
using System.IO;

namespace Grimoire.GUI.Views
{
    public partial class ProjectSettingsWindow : Window
    {
        public new MainWindow Parent;
        private Project Settings;
        public ProjectSettingsWindow(Project settings)
        {
            Setup();
            Settings = settings;
            NameTextBox.Text = Settings.Name;
            ROMPathTextBox.Text = Settings.ROMPath;
            ProjectPathTextBox.Text = Settings.ProjectPath;
        }

        public ProjectSettingsWindow()
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
            SaveButton.Click += SaveButton_Click;

        }

        private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!Directory.Exists(ProjectPathTextBox.Text))
                Directory.CreateDirectory(ProjectPathTextBox.Text);

            Settings.Name = NameTextBox.Text;
            Settings.ROMPath = ROMPathTextBox.Text;
            Settings.ProjectPath = ProjectPathTextBox.Text;

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
