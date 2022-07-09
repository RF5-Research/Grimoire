using AssetsTools.NET.Extra;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using GrimoireGUI.ViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GrimoireGUI.Views
{
    public partial class AssetsWindow : Window
    {

        public AssetsWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = new AssetsWindowViewModel();
            var am = new AssetsManager();
        }
    }
}
