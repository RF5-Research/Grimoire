using Grimoire;
using Grimoire.Models.RF5.Define;
using GrimoireGUI.Core;
using GrimoireGUI.Models;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GrimoireGUI.ViewModels
{

    public class AdvScriptWindowViewModel : ViewModelBase
    {
        private Dictionary<AdvScriptId, string> Scripts;
        private ObservableCollection<AdvScriptId> ScriptList { get; }
        private AdvScript AdvScript;

        //Clean all this up later
        private string scriptText;
        public string ScriptText { get => scriptText; set => this.RaiseAndSetIfChanged(ref scriptText, value); }
        AdvScriptId selectedItem;
        public AdvScriptId SelectedItem
        {
            get => selectedItem;
            set
            {
                Scripts[selectedItem + 1] = ScriptText;
                this.RaiseAndSetIfChanged(ref selectedItem, value);
                ScriptText = Scripts[value + 1];
            }
        }

        public AdvScriptWindowViewModel()
        {
            var platform = ProjectManager.Project.Platform == Platform.Switch ? "Switch" : "Steam";
            AdvScript = new AdvScript($"Resources/{platform}/AdvScriptFunctions.json");

            Scripts = AdvScript.LoadPackScript();
            ScriptList = new ObservableCollection<AdvScriptId>(Scripts.Keys);
            ScriptText = Scripts[SelectedItem + 1];
        }

        public List<CommandData> GetSymbols()
        {
            return AdvScript.Commands;
        }

        //TODO: High priority unload assets to prevent file read locks
        //TODO: Set changes to current script manually when saving
        public void Save()
        {
            Scripts[selectedItem + 1] = ScriptText;
            AdvScript.SavePackScript(Scripts.Values.ToArray());
        }
    }
}