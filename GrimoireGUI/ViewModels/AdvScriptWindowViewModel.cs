using Grimoire;
using Grimoire.Models.RF5.Define;
using GrimoireGUI.Core;
using GrimoireGUI.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace GrimoireGUI.ViewModels
{
    public class AdvScriptWindowViewModel : ViewModelBase
    {
        //TODO: Probably just create a container item
        private Dictionary<AdvScriptId, string> Scripts;
        private AdvScript AdvScript;

        private ObservableCollection<AdvScriptId> ScriptList { get; set; }
        [Reactive] public string ScriptText { get; set; }
        [Reactive] public AdvScriptId SelectedItem { get; set; }
        [Reactive] private string SearchText { get; set; }

        public AdvScriptWindowViewModel()
        {
            var platform = ProjectManager.Project.Platform == Platform.Switch ? "Switch" : "Steam";
            //Need to fix this; Avalonia Designer does not like this
            AdvScript = new AdvScript($"Resources/{platform}/AdvScriptFunctions.json");

            Scripts = AdvScript.LoadPackScript(LoaderID.Master["ADVINDEXDATA"], LoaderID.Event["PACK"]);
            ScriptList = new ObservableCollection<AdvScriptId>(Scripts.Keys);
            var script = Scripts[SelectedItem + 1];
            ScriptText = string.IsNullOrEmpty(script) ? string.Empty : script;

            this.WhenAnyValue(x => x.ScriptText)
                .Subscribe(UpdateText);

            this.WhenAnyValue(
                x => x.SearchText,
                searchText => !string.IsNullOrEmpty(searchText)
            )
            .Subscribe(Search);
            
            this.WhenAnyValue(x => x.SelectedItem)
                .Subscribe(LoadScript);
        }

        public List<CommandData> GetSymbols()
        {
            return AdvScript.Commands;
        }

        private void UpdateText(string scriptText)
        {
             Scripts[SelectedItem + 1] = scriptText;
        }

        private void Search(bool search)
        {
            if (search)
            {
                var queue = new ObservableCollection<AdvScriptId>();
                foreach (var script in Scripts)
                {
                    if (script.Value != null && script.Value.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                        queue.Add(script.Key + 1);
                }
                ScriptList = queue;
            }
            else
            {
                ScriptList = new ObservableCollection<AdvScriptId>(Scripts.Keys);
            }
        }
        private void LoadScript(AdvScriptId index)
        {
            if ((int)index != -1)
            {
                var script = Scripts[index + 1];
                ScriptText = string.IsNullOrEmpty(script) ? string.Empty : script;
            }
        }

        public void Save()
        {
            AdvScript.SavePackScript(Scripts.Values.ToArray(), LoaderID.Master["ADVINDEXDATA"], LoaderID.Event["PACK"]);
        }
    }
}