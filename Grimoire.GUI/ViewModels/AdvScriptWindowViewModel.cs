using Avalonia.Controls;
using Grimoire.GUI.Views;
using ReactiveUI;
using RF5Game.Define;
using System;
using System.Collections.ObjectModel;


namespace Grimoire.GUI.ViewModels
{

    public class AdvScriptWindowViewModel : ViewModelBase
    {
        ScriptManager ScriptManager;
        ObservableCollection<string> ScriptNamesList { get; }
        
        private string scriptText;
        public string ScriptText { get => scriptText; set => this.RaiseAndSetIfChanged(ref scriptText, value); }

        int selectedIndex;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                ScriptManager.PackedScripts[selectedIndex] = ScriptText;
                this.RaiseAndSetIfChanged(ref selectedIndex, value);
                ScriptText = ScriptManager.PackedScripts[value];
            }
        }

        public AdvScriptWindowViewModel()
        {
            ScriptManager = new ScriptManager("Resources/script-functions.json");
            ScriptManager.ReadPackedFile("Resources/pack.bytes", "Resources/AdvIndexData.json");
            ScriptNamesList = new ObservableCollection<string>();
            for (var index = 0; index < ScriptManager.PackedScripts.Count; index++)
            {
                var name = Enum.GetName((AdvScriptId)index + 1);
                if (name != null)
                    ScriptNamesList.Add(name);
                else
                    ScriptNamesList.Add(index.ToString());
            }

            ScriptText = ScriptManager.PackedScripts[selectedIndex];
        }
    }
}