using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using GrimoireGUI.Models;
using GrimoireGUI.ViewModels;
using ReactiveUI;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GrimoireGUI.Views
{
    public partial class MainWindow : Window
    {
        //TODO: databind in code
        internal Settings Settings = new();
        private const string SettingsFilename = "settings.json";

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            NewProjectButton.Click += NewProjectButton_Click;
            OpenProjectButton.Click += OpenProjectButton_Click;
            DeleteProjectButton.Click += DeleteProjectButton_Click;
            ProjectSettingsButton.Click += ProjectSettingsButton_Click;
            Opened += MainWindow_Opened;
            Closed += MainWindow_Closed;
        }

        private async void MainWindow_Opened(object? sender, EventArgs e)
        {
            if (File.Exists(SettingsFilename))
            {
                using (var fs = new FileStream(SettingsFilename, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(fs))
                {
                    var settings = JsonSerializer.Deserialize<Settings>(reader.ReadToEnd());
                    if (settings != null)
                        Settings = settings;
                    else
                    {
                        var dialog = MessageBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandardWindow(
                            "Error",
                            $"Failed to read {SettingsFilename}.\nCreating new settings.");
                        await dialog.ShowDialog(this);
                    }
                }
            }
            DataContext = new MainWindowViewModel(Settings.Projects);
        }

        private void MainWindow_Closed(object? sender, System.EventArgs e)
        {
            SaveSettings();
        }

        private async void ProjectSettingsButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dialog = new ProjectSettingsWindow(Settings.Projects[ProjectDataGrid.SelectedIndex]);
            await dialog.ShowDialog(this);
        }

        private void DeleteProjectButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Settings.Projects.RemoveAt(ProjectDataGrid.SelectedIndex);
        }

        private void OpenProjectButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _ = OpenProject(Settings.Projects[ProjectDataGrid.SelectedIndex]);
        }

        private async void NewProjectButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dialog = new ProjectCreateWindow();
            await dialog.ShowDialog(this);
        }

        //Maybe add to VM
        internal void SaveSettings()
        {
            using var fs = new FileStream(SettingsFilename, FileMode.Create, FileAccess.Write);
            using var writer = new StreamWriter(fs);
            writer.Write(JsonSerializer.Serialize(Settings));
        }

        internal async Task OpenProject(Project project)
        {
            var dialog = new LoadingWindow();
            var task = Task.Run(() => ProjectManager.InitializeAsync(project, dialog.Cts), dialog.Cts.Token);
            _ = dialog.ShowDialog(this);
            await task;

            if (task.IsCompletedSuccessfully)
            {
                var window = new ProjectMainWindow();
                App.SwapMainWindow(window);
                window.Show();
                Close();
            }
        }
    }
}
