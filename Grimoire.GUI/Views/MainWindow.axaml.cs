using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Grimoire.Core;
using Grimoire.GUI.Models;
using Grimoire.GUI.ViewModels;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Grimoire.GUI.Views
{
    public partial class MainWindow : Window
    {
        internal Settings Settings;
        //internal ObservableCollection<Project> Projects;
        private const string SettingsFilename = "settings.json";

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = new MainWindowViewModel();
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
            ProjectDataGrid.Items = Settings.Projects;

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
            var dialog = new CreateProjectWindow();
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
            var dialog = new LoadingWindow();
            _ = dialog.ShowDialog(this);

            var cancellationToken = new CancellationTokenSource();
            dialog.Closed += (object? sender, System.EventArgs e) => cancellationToken.Cancel();
            await Task.Run(() => ProjectManager.Initialize(project), cancellationToken.Token);

            if (cancellationToken.IsCancellationRequested != true)
            {
                var window = new ProjectMainWindow();
                window.Show();
                Close();
            }
        }
    }
}
