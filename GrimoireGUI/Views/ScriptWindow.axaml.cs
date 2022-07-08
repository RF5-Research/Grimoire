using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using DynamicData;
using GrimoireGUI.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;

namespace GrimoireGUI.Views
{
    using Pair = KeyValuePair<int, IControl>;

    public partial class ScriptWindow : Window
    {
        private readonly TextMate.Installation _textMateInstallation;
        private CompletionWindow? CompletionWindow;
        private OverloadInsightWindow? _insightWindow;
        private ElementGenerator _generator = new ElementGenerator();
        private RegistryOptions _registryOptions;
        private int _currentTheme = (int)ThemeName.DarkPlus;

        public static readonly DirectProperty<ScriptWindow, string> TextProperty =
            AvaloniaProperty.RegisterDirect<ScriptWindow, string>(
                nameof(Text),
                o => o.Text,
                (o, v) => o.Text = v);

        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                SetAndRaise(TextProperty, ref _text, value);
                if (value != null)
                    TextEditor.Document = new TextDocument(value);
            }
        }

        public ScriptWindow()
        {
            InitializeComponent();
            TogglePaneButton.Click += TogglePaneButton_Click;
            //DataContext = new AdvScriptWindowViewModel();
            TextEditor.Background = Brushes.Transparent;
            TextEditor.ShowLineNumbers = true;
            TextEditor.ContextMenu = new ContextMenu
            {
                Items = new List<MenuItem>
                {
                    new MenuItem { Header = "Copy", InputGesture = new KeyGesture(Key.C, KeyModifiers.Control) },
                    new MenuItem { Header = "Paste", InputGesture = new KeyGesture(Key.V, KeyModifiers.Control) },
                    new MenuItem { Header = "Cut", InputGesture = new KeyGesture(Key.X, KeyModifiers.Control) }
                }
            };
            TextEditor.TextArea.Background = Background;
            TextEditor.TextArea.TextEntered += TextArea_TextEntered;
            TextEditor.TextArea.TextEntering += TextArea_TextEntering;
            TextEditor.Options.ShowBoxForControlCharacters = true;
            TextEditor.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.CSharp.CSharpIndentationStrategy(TextEditor.Options);
            TextEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            TextEditor.TextArea.RightClickMovesCaret = true;

            TextEditor.TextArea.TextView.ElementGenerators.Add(_generator);

            _registryOptions = new RegistryOptions(
                (ThemeName)_currentTheme);

            _textMateInstallation = TextEditor.InstallTextMate(_registryOptions);

            Language csharpLanguage = _registryOptions.GetLanguageByExtension(".cs");

            string scopeName = _registryOptions.GetScopeByLanguageId(csharpLanguage.Id);
            //TextEditor.Document = new TextDocument(
            //    "// AvaloniaEdit supports displaying control chars: \a or \b or \v" + Environment.NewLine +
            //    "// AvaloniaEdit supports displaying underline and strikethrough" + Environment.NewLine);
            _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(csharpLanguage.Id));
            TextEditor.TextArea.TextView.LineTransformers.Add(new UnderlineAndStrikeThroughTransformer());
            AddHandler(KeyDownEvent, (o, e) =>
            {
                switch (e.Key)
                {
                    case Key.Back:
                        {
                            if (CompletionWindow != null)
                            {
                                if (TextEditor.CaretOffset == 0)
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
            AddHandler(PointerWheelChangedEvent, (o, i) =>
            {
                if (i.KeyModifiers != KeyModifiers.Control) return;
                if (i.Delta.Y > 0) TextEditor.FontSize++;
                else TextEditor.FontSize = TextEditor.FontSize > 1 ? TextEditor.FontSize - 1 : 1;
            }, RoutingStrategies.Bubble, true);
        }

        public void Save(object? sender, RoutedEventArgs e)
        {
            ((AdvScriptWindowViewModel)DataContext).Save();
        }

        private void ShowCompletionWindow()
        {
            CompletionWindow = new(TextEditor.TextArea);
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
                var completionData = ((AdvScriptWindowViewModel)DataContext).GetSymbols()
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

        private void TogglePaneButton_Click(object? sender, RoutedEventArgs e)
        {
            if (SplitView.IsPaneOpen)
            {
                SplitView.IsPaneOpen = false;
            }
            else
            {
                SplitView.IsPaneOpen = true;
            }
        }

        private string GetCompletionPrefix()
        {
            var offset = TextEditor.TextArea.Caret.Offset;
            var stringBuilder = new StringBuilder();
            if (CompletionWindow != null)
            {
                while (true)
                {
                    if (offset != 0)
                    {
                        var character = TextEditor.TextArea.Document.GetCharAt(--offset);
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
                TextEditor.TextArea.Caret.Line,
                TextEditor.TextArea.Caret.Column);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _textMateInstallation.Dispose();
        }

        class UnderlineAndStrikeThroughTransformer : DocumentColorizingTransformer
        {
            protected override void ColorizeLine(DocumentLine line)
            {
                if (line.LineNumber == 2)
                {
                    string lineText = this.CurrentContext.Document.GetText(line);

                    int indexOfUnderline = lineText.IndexOf("underline");
                    int indexOfStrikeThrough = lineText.IndexOf("strikethrough");

                    if (indexOfUnderline != -1)
                    {
                        ChangeLinePart(
                            line.Offset + indexOfUnderline,
                            line.Offset + indexOfUnderline + "underline".Length,
                            visualLine => visualLine.TextRunProperties.Underline = true);
                    }

                    if (indexOfStrikeThrough != -1)
                    {
                        ChangeLinePart(
                            line.Offset + indexOfStrikeThrough,
                            line.Offset + indexOfStrikeThrough + "strikethrough".Length,
                            visualLine => visualLine.TextRunProperties.Strikethrough = true);
                    }
                }
            }
        }

        private class MyOverloadProvider : IOverloadProvider
        {
            private readonly IList<(string header, string content)> _items;
            private int _selectedIndex;

            public MyOverloadProvider(IList<(string header, string content)> items)
            {
                _items = items;
                SelectedIndex = 0;
            }

            public int SelectedIndex
            {
                get => _selectedIndex;
                set
                {
                    _selectedIndex = value;
                    OnPropertyChanged();
                    // ReSharper disable ExplicitCallerInfoArgument
                    OnPropertyChanged(nameof(CurrentHeader));
                    OnPropertyChanged(nameof(CurrentContent));
                    // ReSharper restore ExplicitCallerInfoArgument
                }
            }

            public int Count => _items.Count;
            public string CurrentIndexText => null;
            public object CurrentHeader => _items[SelectedIndex].header;
            public object CurrentContent => _items[SelectedIndex].content;

            public event PropertyChangedEventHandler? PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
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

        class ElementGenerator : VisualLineElementGenerator, IComparer<Pair>
        {
            public List<Pair> controls = new List<Pair>();

            /// <summary>
            /// Gets the first interested offset using binary search
            /// </summary>
            /// <returns>The first interested offset.</returns>
            /// <param name="startOffset">Start offset.</param>
            public override int GetFirstInterestedOffset(int startOffset)
            {
                int pos = controls.BinarySearch(new Pair(startOffset, null), this);
                if (pos < 0)
                    pos = ~pos;
                if (pos < controls.Count)
                    return controls[pos].Key;
                else
                    return -1;
            }

            public override VisualLineElement? ConstructElement(int offset)
            {
                int pos = controls.BinarySearch(new Pair(offset, null), this);
                if (pos >= 0)
                    return new InlineObjectElement(0, controls[pos].Value);
                else
                    return null;
            }

            int IComparer<Pair>.Compare(Pair x, Pair y)
            {
                return x.Key.CompareTo(y.Key);
            }
        }
    }
}