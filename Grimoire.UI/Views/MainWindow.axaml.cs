using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Grimoire.UI.Models;
using Grimoire.UI.ViewModels;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace Grimoire.UI.Views
{
    [DoNotNotify]
    public partial class MainWindow : Window
    {
        internal ObservableCollection<ProjectSettings> ProjectSettings;
        private const string ProjectSettingsFilename = "settings.json";

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            //DataContext = new MainWindowViewModel();
            //((MainWindowViewModel)DataContext).ProjectSettings = new ObservableCollection<ProjectSettings>();
            //MyDataGrid.Items = ((MainWindowViewModel)DataContext).ProjectSettings;
            if (File.Exists(ProjectSettingsFilename))
            {
                using (var fs = new FileStream(ProjectSettingsFilename, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(fs))
                    ProjectSettings = JsonSerializer.Deserialize<ObservableCollection<ProjectSettings>>(reader.ReadToEnd());
            }
            else
            {
                ProjectSettings = new ObservableCollection<ProjectSettings>();
            }
            ProjectDataGrid.Items = ProjectSettings;

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
            var dialog = new EditProjectWindow(ProjectSettings[ProjectDataGrid.SelectedIndex]);
            await dialog.ShowDialog(this);
        }

        private void DeleteProjectButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ProjectSettings.RemoveAt(ProjectDataGrid.SelectedIndex);
        }

        private void OpenProjectButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private async void NewProjectButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dialog = new CreateProjectWindow(this);
            await dialog.ShowDialog(this);
        }

        internal void SaveSettings()
        {
            using (var fs = new FileStream(ProjectSettingsFilename, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(fs))
                writer.Write(JsonSerializer.Serialize(ProjectSettings));
            //writer.Write(JsonSerializer.Serialize(ProjectSettings));
        }
    }
}
