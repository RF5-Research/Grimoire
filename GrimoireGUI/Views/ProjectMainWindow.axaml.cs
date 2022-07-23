using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using System;

namespace GrimoireGUI.Views
{
    public partial class ProjectMainWindow : Window
    {
        public bool IsClosingProject;
        private AdvScriptWindow AdvScriptWindow;
        public ProjectMainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            CloseMenuItem.Click += CloseProjectMenuItem_Click;
            SaveMenuItem.Click += SaveMenuItem_Click;

            //CharactersButton.Click += CharactersButton_Click;
            ScriptEditorButton.Click += ScriptEditorButton_Click;
            RigbarthScriptEditorButton.Click += RigbarthScriptEditorButton_Click;
            AssetsButton.Click += AssetsButton_Click;
        }

        private void RigbarthScriptEditorButton_Click(object? sender, RoutedEventArgs e)
        {
            var window = new RigbarthAdvScriptWindow();
            window.Show(this);
        }

        public static readonly RoutedEvent<RoutedEventArgs> SaveEvent =
            RoutedEvent.Register<ProjectMainWindow, RoutedEventArgs>(nameof(Save), RoutingStrategies.Bubble);

        public event EventHandler<RoutedEventArgs> Save
        {
            add => AddHandler(SaveEvent, value);
            remove => RemoveHandler(SaveEvent, value);
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
            AdvScriptWindow = new AdvScriptWindow();
            Save += AdvScriptWindow.Save;
            AdvScriptWindow.Show(this);

            //Implement later
            //if (AdvScriptWindow == null)
            //{
            //    AdvScriptWindow = new AdvScriptWindow();
            //    Save += AdvScriptWindow.Save;
            //    AdvScriptWindow.Show(this);
            //}
            //else if (!AdvScriptWindow.IsVisible)
            //    AdvScriptWindow.Show();
        }
    }
}
