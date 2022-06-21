using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Grimoire.Core;
using Grimoire.GUI.Models;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Grimoire.GUI.Views
{
    public partial class ProjectMainWindow : Window
    {
        public ProjectMainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            ScriptEditorButton.Click += ScriptEditorButton_Click;
        }

        private void ScriptEditorButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            AssetService.Initialize();
            var window = new AdvScriptWindow();
            window.Show();
        }
    }
}
