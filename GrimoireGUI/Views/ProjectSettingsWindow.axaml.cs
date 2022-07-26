using Avalonia;
using Avalonia.Controls;
using Grimoire;
using Grimoire.Models.UnityEngine;
using GrimoireGUI.Models;
using GrimoireGUI.ViewModels;
using PropertyChanged;
using System;
using System.IO;
using System.Linq;

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
            GamePathTextBox.Text = Settings.GamePath;
            ProjectPathTextBox.Text = Settings.ProjectPath;
            GameLanguageComboBox.SelectedItem = Settings.GameLanguage;
            PlatformComboBox.SelectedItem = Settings.Platform;
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
            GamePathBrowseButton.Click += GamePathBrowseButton_Click;
            ProjectPathBrowseButton.Click += ProjectPathBrowseButton_Click;
            CancelButton.Click += CancelButton_Click;
            SaveButton.Click += SaveButton_Click;
            GameLanguageComboBox.Items = AssetsLoader.GameLanguages.Keys;
            PlatformComboBox.Items = Enum.GetValues(typeof(Platform)).Cast<Platform>();
        }

        private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!Directory.Exists(ProjectPathTextBox.Text))
                Directory.CreateDirectory(ProjectPathTextBox.Text);

            Settings.Name = NameTextBox.Text;
            Settings.GamePath = GamePathTextBox.Text;
            Settings.ProjectPath = ProjectPathTextBox.Text;
            Settings.GameLanguage = (SystemLanguage)GameLanguageComboBox.SelectedItem!;
            Settings.Platform = (Platform)PlatformComboBox.SelectedItem!;
            Close();
        }

        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        private async void GamePathBrowseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            var text = await dialog.ShowAsync(this);
            if (!string.IsNullOrEmpty(text))
                GamePathTextBox.Text = text;
        }

        private async void ProjectPathBrowseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            var text = await dialog.ShowAsync(this);
            if (!string.IsNullOrEmpty(text))
                ProjectPathTextBox.Text = text;
        }
    }
}
