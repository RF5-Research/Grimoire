using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Grimoire;
using GrimoireGUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;

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
            var x = new AdvScriptWindowViewModel();
            x.Save();
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
            var window = new ScriptWindow();
            Save += window.Save;
            window.Show(this);
        }
    }
}
