using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace GrimoireGUI.Views
{
    public partial class ProjectMainWindow : Window
    {
        public bool IsClosingProject;
        public ProjectMainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            CloseMenuItem.Click += CloseProjectMenuItem_Click;
            SaveMenuItem.Click += SaveMenuItem_Click;

            CharactersButton.Click += CharactersButton_Click;
            ScriptEditorButton.Click += ScriptEditorButton_Click;
            AssetsButton.Click += AssetsButton_Click;
        }

        private void AssetsButton_Click(object? sender, RoutedEventArgs e)
        {
            var window = new AssetsWindow();
            window.Show(this);
        }

        private void CharactersButton_Click(object? sender, RoutedEventArgs e)
        {
            var window = new CharactersWindow();
            window.Show(this);
        }

        public static readonly RoutedEvent<RoutedEventArgs> SaveEvent =
            RoutedEvent.Register<ProjectMainWindow, RoutedEventArgs>(nameof(Save), RoutingStrategies.Bubble);

        public event EventHandler<RoutedEventArgs> Save
        {
            add => AddHandler(SaveEvent, value);
            remove => RemoveHandler(SaveEvent, value);
        }

        //Propagate event to child windows that subscribe to event
        private void SaveMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            var eventArgs = new RoutedEventArgs(SaveEvent);
            RaiseEvent(eventArgs);
        }

        private void CloseProjectMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            IsClosingProject = true;
            new MainWindow().Show();
            Close();
        }

        private void ScriptEditorButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = new AdvScriptWindow();
            window.Show(this);
        }
    }
}
