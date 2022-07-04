using AssetsTools.NET.Extra;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media.Imaging;
using Grimoire.GUI.Core;
using Grimoire.GUI.Core.Services;
using Grimoire.GUI.Core.Texture;
using Grimoire.GUI.Models.RF5;
using Grimoire.GUI.Models.RF5.Define;
using Grimoire.GUI.Models.RF5.Loader.ID;
using Grimoire.GUI.Models.UnityEngine;
using Grimoire.GUI.ViewModels;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Grimoire.GUI.Views
{
    public partial class CharactersWindow : Window
    {
        public CharactersWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Opened += CharactersWindow_Opened;
            Closing += CharactersWindow_Closing;
            DataContext = new CharactersWindowViewModel();
        }

        private void CharactersWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!((ProjectMainWindow)Owner).IsClosingProject)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void CharactersWindow_Opened(object? sender, System.EventArgs e)
        {
            ((ProjectMainWindow)Owner).Save += CharactersWindow_Save;
        }

        private void CharactersWindow_Save(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            //Implement what objects to write
            throw new System.NotImplementedException();
        }
    }
}
