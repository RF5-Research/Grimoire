using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GrimoireGUI.Models;
using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace GrimoireGUI.Views
{
    public partial class LoadingWindow : Window
    {
        private CancellationTokenSource Cts = new();
        private Action Action;
        private Action? Callback;
        public delegate void WorkCompletedCallBack(string result);

        public LoadingWindow() 
        {
            Setup();
        }

        private void Setup()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Closed += (object? sender, EventArgs e) => Cts.Cancel();
            CancelButton.Click += CancelButton_Click;
            Opened += LoadingWindow_Opened;
        }

        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Cts.Cancel();
            Task.Delay(500);
            Close();
        }

        public LoadingWindow(Action action, Action? callback = null)
        {
            Setup();
            Action = action;
            Callback = callback;
        }

        private async void LoadingWindow_Opened(object? sender, EventArgs e)
        {
            try 
            {
                await Task.Run(() => Action(), Cts.Token);
                if (!Cts.IsCancellationRequested)
                    Callback?.Invoke();
                Close();
            }
            catch
            {
                Close();
            }
        }
    }
}
