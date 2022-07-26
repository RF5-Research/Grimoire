//using Avalonia;
//using Avalonia.Controls;
//using Avalonia.Markup.Xaml;
//using Avalonia.ReactiveUI;
//using GrimoireGUI.Models;
//using GrimoireGUI.ViewModels;
//using ReactiveUI;
//using System;
//using System.Reactive.Disposables;
//using System.Reactive.Subjects;
//using System.Threading;
//using System.Threading.Tasks;

//namespace GrimoireGUI.Views
//{
//    public partial class LoadingWindow : ReactiveWindow<LoadingWindowViewModel>
//    {
//        public LoadingWindow()
//        {
//            InitializeComponent();
//#if DEBUG
//            this.AttachDevTools();
//#endif
//            this.WhenActivated(disposables =>
//            {
//                //var x = this.BindCommand(
//                //    ViewModel,
//                //    vm => vm.CancelCommand,
//                //    v => v,
//                //    nameof(Closed)
//                //).DisposeWith(disposables);

//                this.BindCommand(
//                    ViewModel,
//                    vm => vm.CancelCommand,
//                    v => v.CancelButton,
//                    nameof(CancelButton.Click)
//                ).DisposeWith(disposables);
//            });
//            CancelButton.Click += (s, e) => Close();
//        }
//    }
//}



using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using GrimoireGUI.ViewModels;
using System;
using System.Threading;

namespace GrimoireGUI.Views
{
    public partial class LoadingWindow : Window
    {
        public CancellationTokenSource Cts = new();
        public LoadingWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Closed += (object? sender, EventArgs e) => Cts.Cancel();
            CancelButton.Click += (s, e) =>
            {
                Cts.Cancel();
                Close();
            };
        }
    }
}
