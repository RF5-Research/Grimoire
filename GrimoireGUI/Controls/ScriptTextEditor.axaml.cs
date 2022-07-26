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
using TextMateSharp.Grammars;

namespace GrimoireGUI.Controls
{
    using Pair = KeyValuePair<int, IControl>;

    public partial class ScriptTextEditor : UserControl
    {
        private readonly TextMate.Installation _textMateInstallation;
        private ElementGenerator _generator = new ElementGenerator();
        private RegistryOptions _registryOptions;
        private int _currentTheme = (int)ThemeName.DarkPlus;

        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<ScriptTextEditor, string>(nameof(Text));

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private void TextEditor_TextChanged(object? sender, EventArgs e)
        {
            if (TextEditor != null && TextEditor.Document != null)
            {
                Text = TextEditor.Document.Text;
            }
        }

        private void TextPropertyChanged(string text)
        {
            if (TextEditor != null && TextEditor.Document != null && text != null && text != TextEditor.Document.Text)
            {
                TextEditor.Document = new(text);
            }
        }

        public ScriptTextEditor()
        {
            InitializeComponent();
            TextEditor.TextChanged += TextEditor_TextChanged;
            this.GetObservable(TextProperty).Subscribe(TextPropertyChanged);


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
            TextEditor.Options.ShowBoxForControlCharacters = true;
            TextEditor.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.CSharp.CSharpIndentationStrategy(TextEditor.Options);
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

            AddHandler(PointerWheelChangedEvent, (o, i) =>
            {
                if (i.KeyModifiers != KeyModifiers.Control) return;
                if (i.Delta.Y > 0) TextEditor.FontSize++;
                else TextEditor.FontSize = TextEditor.FontSize > 1 ? TextEditor.FontSize - 1 : 1;
            }, RoutingStrategies.Bubble, true);
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

            private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                int pos = controls.BinarySearch(new Pair(startOffset, null!), this);
                if (pos < 0)
                    pos = ~pos;
                if (pos < controls.Count)
                    return controls[pos].Key;
                else
                    return -1;
            }

            public override VisualLineElement? ConstructElement(int offset)
            {
                int pos = controls.BinarySearch(new Pair(offset, null!), this);
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
