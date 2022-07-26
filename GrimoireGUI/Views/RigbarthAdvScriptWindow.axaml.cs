using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using DynamicData;
using GrimoireGUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrimoireGUI.Views
{
    public partial class RigbarthAdvScriptWindow : Window
    {
        private CompletionWindow? CompletionWindow;
        private OverloadInsightWindow? _insightWindow;

        public RigbarthAdvScriptWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            SaveAsMenuItem.Click += SaveAsMenuItem_Click;
            OpenMenuItem.Click += OpenMenuItem_Click;
            DataContext = new RigbarthAdvScriptWindowViewModel();

            ScriptTextEditor.TextEditor.TextArea.TextEntered += TextArea_TextEntered;
            //This is horribly problematic when writing strings
            //ScriptTextEditor.TextEditor.TextArea.TextEntering += TextArea_TextEntering;
            ScriptTextEditor.TextEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;

            AddHandler(KeyDownEvent, (o, e) =>
            {
                switch (e.Key)
                {
                    case Key.Back:
                        {
                            if (CompletionWindow != null)
                            {
                                if (ScriptTextEditor.TextEditor.CaretOffset == 0)
                                {
                                    CloseCompletionWindow();
                                    return;
                                }
                                UpdateCompletionWindow();
                            }
                        }
                        break;

                }
                if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.J)
                {
                    ShowCompletionWindow();
                }
            }, RoutingStrategies.Bubble, true);
        }

        private async void OpenMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            var filters = new List<FileDialogFilter>
            {
                new FileDialogFilter { Extensions = new List<string> { "advScript" } }
            };
            var dialog = new OpenFileDialog()
            {
                AllowMultiple = false,
                Filters = filters
            };
            var path = await dialog.ShowAsync(this);
            if (path != null)
                ((RigbarthAdvScriptWindowViewModel)DataContext!).Load(path[0]);
        }

        private async void SaveAsMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            var filters = new List<FileDialogFilter>
            {
                new FileDialogFilter { Extensions = new List<string> { "advScript" } }
            };
            var dialog = new SaveFileDialog()
            {
                DefaultExtension = "advScript",
                Filters = filters
            };
            var path = await dialog.ShowAsync(this);
            if (!string.IsNullOrEmpty(path))
                ((RigbarthAdvScriptWindowViewModel)DataContext!).Save(path);
        }

        private void ShowCompletionWindow()
        {
            CompletionWindow = new(ScriptTextEditor.TextEditor.TextArea);
            CompletionWindow.Closed += (o, args) => CompletionWindow = null;
            --CompletionWindow.StartOffset;

            UpdateCompletionWindow();
            if (CompletionWindow != null)
                CompletionWindow.Show();
        }

        private void CloseCompletionWindow()
        {
            if (CompletionWindow != null)
            {
                CompletionWindow.Hide();
                CompletionWindow = null;
            }
        }

        private void UpdateCompletionWindow()
        {
            if (CompletionWindow != null)
            {
                var completionPrefix = GetCompletionPrefix();
                var completionData = ((RigbarthAdvScriptWindowViewModel)DataContext!).GetSymbols()
                    .Where(x => x.Name.Contains(completionPrefix, StringComparison.OrdinalIgnoreCase))
                    .Select(x => new CompletionData(x.Name, x.Description));
                if (completionData.Count() > 0)
                {
                    CompletionWindow.CompletionList.CompletionData.Clear();
                    CompletionWindow.CompletionList.CompletionData.Add(completionData);
                }
                else
                    CloseCompletionWindow();
            }
        }

        private string GetCompletionPrefix()
        {
            var offset = ScriptTextEditor.TextEditor.TextArea.Caret.Offset;
            var stringBuilder = new StringBuilder();
            if (CompletionWindow != null)
            {
                while (true)
                {
                    if (offset != 0)
                    {
                        var character = ScriptTextEditor.TextEditor.TextArea.Document.GetCharAt(--offset);
                        if (char.IsWhiteSpace(character))
                            break;
                        stringBuilder.Insert(0, character);
                        CompletionWindow.StartOffset = offset;
                    }
                    else
                        break;
                }
            }
            return stringBuilder.ToString();
        }

        private void TextArea_TextEntering(object? sender, TextInputEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Text) && CompletionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    CompletionWindow.CompletionList.RequestInsertion(e);
                }
            }

            _insightWindow?.Hide();

            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        private void TextArea_TextEntered(object? sender, TextInputEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Text))
            {
                if (CompletionWindow == null)
                {
                    ShowCompletionWindow();
                }
                else
                {
                    UpdateCompletionWindow();
                }
            }
            //else if (e.Text == "(")
            //{
            //    _insightWindow = new OverloadInsightWindow(TextEditor.TextArea);
            //    _insightWindow.Closed += (o, args) => _insightWindow = null;

            //    _insightWindow.Provider = new MyOverloadProvider(new[]
            //    {
            //        ("Method1(int, string)", "Method1 description"),
            //        ("Method2(int)", "Method2 description"),
            //        ("Method3(string)", "Method3 description"),
            //    });
            //    _insightWindow.Show();
            //}
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            StatusTextBlock.Text = string.Format("Line {0} Column {1}",
                ScriptTextEditor.TextEditor.TextArea.Caret.Line,
                ScriptTextEditor.TextEditor.TextArea.Caret.Column);
        }

        public class CompletionData : ICompletionData
        {
            public CompletionData(string symbol, string description)
            {
                Text = symbol;
                Description = description;
            }

            public IBitmap Image => null;

            public string Text { get; }

            // Use this property if you want to show a fancy UIElement in the list.
            public object Content => Text;
            public object Description { get; }
            public double Priority { get; } = 0;

            public void Complete(TextArea textArea, ISegment completionSegment,
                EventArgs insertionRequestEventArgs)
            {
                textArea.Document.Replace(completionSegment, Text);
            }
        }
    }
}
