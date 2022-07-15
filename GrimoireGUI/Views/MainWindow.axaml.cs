using Avalonia;
using Avalonia.Controls;
using GrimoireGUI.Models;
using GrimoireGUI.ViewModels;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GrimoireGUI.Views
{
    public partial class MainWindow : Window
    {
        internal Settings Settings;
        private const string SettingsFilename = "settings.json";

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            if (File.Exists(SettingsFilename))
            {
                using (var fs = new FileStream(SettingsFilename, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(fs))
                {
                    Settings = JsonSerializer.Deserialize<Settings>(reader.ReadToEnd());
                    //try
                    //{
                    //    Settings = JsonSerializer.Deserialize<Settings>(reader.ReadToEnd());
                    //    Projects = Settings.Projects;
                    //}
                    //catch
                    //{
                    //    Settings = new Settings();
                    //    Projects = new ObservableCollection<Project>();
                    //}
                }
            }
            else
            {
                Settings = new();
            }

            DataContext = new MainWindowViewModel(Settings.Projects);

            NewProjectButton.Click += NewProjectButton_Click;
            OpenProjectButton.Click += OpenProjectButton_Click;
            DeleteProjectButton.Click += DeleteProjectButton_Click;
            ProjectSettingsButton.Click += ProjectSettingsButton_Click;
            Closed += MainWindow_Closed;
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

        internal void SaveSettings()
        {
            if (Settings != null)
            {
                using (var fs = new FileStream(SettingsFilename, FileMode.Create, FileAccess.Write))
                using (var writer = new StreamWriter(fs))
                    writer.Write(JsonSerializer.Serialize(Settings));
            }
        }

        internal async Task OpenProject(Project project)
        {
            var dialog = new LoadingWindow(() => ProjectManager.Initialize(project), () =>
            {
                new ProjectMainWindow().Show();
                Close();
            });
            await dialog.ShowDialog(this);
        }
    }
}
