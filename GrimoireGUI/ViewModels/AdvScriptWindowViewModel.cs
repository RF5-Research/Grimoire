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
        [Reactive] private int FilterSelectedIndex { get; set; } = 0;
        private string[] Filters => new string[]
        {
            "Name",
            "Text"
        };

        private class Item
        {
            public string Text { get; set; }
            public AdvScriptId ScriptName { get; }
            public Item(AdvScriptId name, string text)
            {
                Text = string.IsNullOrEmpty(text) ? string.Empty : text;
                ScriptName = name;
            }
        }
        private List<Item> Scripts;
        private AdvScript AdvScript;

        private ObservableCollection<Item> SearchScriptList { get; set; }
        [Reactive] private string ScriptText { get; set; }
        [Reactive] private Item SelectedItem { get; set; }
        [Reactive] private string SearchText { get; set; }

        public AdvScriptWindowViewModel()
        {
            var platform = ProjectManager.Project.Platform == Platform.Switch ? "Switch" : "Steam";
            //Need to fix this; Avalonia Designer does not like this
            AdvScript = new AdvScript($"Resources/{platform}/AdvScriptFunctions.json");

            Scripts = new List<Item>(
                AdvScript.LoadPackScript(LoaderID.Master["ADVINDEXDATA"], LoaderID.Event["PACK"]).Select(
                    x => new Item(x.Key, x.Value)
                )
            );
            SearchScriptList = new ObservableCollection<Item>(Scripts);

            this.WhenAnyValue(x => x.ScriptText)
                .Subscribe(UpdateText);

            this.WhenAnyValue(
                x => x.SearchText, x => x.FilterSelectedIndex,
                (searchText, searchFilter) => !string.IsNullOrEmpty(searchText)
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
            if (SelectedItem != null)
            {
                var item = Scripts.Find(x => x.ScriptName == SelectedItem.ScriptName);
                item.Text = scriptText;
            }
        }

        private void Search(bool search)
        {
            if (search)
            {
                var queue = new ObservableCollection<Item>();
                var items = FilterSelectedIndex == 0 ?
                    Scripts.Where(x => x.ScriptName.ToString().Contains(SearchText, StringComparison.OrdinalIgnoreCase)) :
                    Scripts.Where(x => x.Text.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                SearchScriptList = new ObservableCollection<Item>(items);
            }
            else
            {
                SearchScriptList = new ObservableCollection<Item>(Scripts);
            }
        }

        private void LoadScript(Item selectedItem)
        {
            if (selectedItem != null)
            {
                ScriptText = selectedItem.Text;
            }
        }

        public void Save()
        {
            AdvScript.SavePackScript(Scripts.Select(x => x.Text).ToArray(), LoaderID.Master["ADVINDEXDATA"], LoaderID.Event["PACK"]);
        }
    }
}