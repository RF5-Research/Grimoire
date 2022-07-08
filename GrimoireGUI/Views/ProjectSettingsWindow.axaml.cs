using Avalonia;
using Avalonia.Controls;
using Grimoire;
using Grimoire.Models.UnityEngine;
using GrimoireGUI.Models;
using GrimoireGUI.ViewModels;
using PropertyChanged;
using System.IO;

namespace GrimoireGUI.Views
{
    public partial class ProjectSettingsWindow : Window
    {
        private Project Settings;
        public ProjectSettingsWindow(Project settings)
        {
            Setup();
            Settings = settings;
            NameTextBox.Text = Settings.Name;
            ROMPathTextBox.Text = Settings.ROMPath;
            ProjectPathTextBox.Text = Settings.ProjectPath;
            GameLanguageComboBox.SelectedItem = Settings.GameLanguage;
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
            DataContext = new ProjectSettingsViewModel();
            ROMPathBrowseButton.Click += ROMPathBrowseButton_Click;
            ProjectPathBrowseButton.Click += ProjectPathBrowseButton_Click;
            CancelButton.Click += CancelButton_Click;
            SaveButton.Click += SaveButton_Click;
            GameLanguageComboBox.Items = AssetsLoader.GameLanguages.Keys;
        }

        private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!Directory.Exists(ProjectPathTextBox.Text))
                Directory.CreateDirectory(ProjectPathTextBox.Text);

            Settings.Name = NameTextBox.Text;
            Settings.ROMPath = ROMPathTextBox.Text;
            Settings.ProjectPath = ProjectPathTextBox.Text;
            Settings.GameLanguage = (SystemLanguage)GameLanguageComboBox.SelectedItem;

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
