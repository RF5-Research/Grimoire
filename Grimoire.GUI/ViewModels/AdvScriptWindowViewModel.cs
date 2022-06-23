using Grimoire.GUI.Core.Services;
using Grimoire.GUI.Models.RF5;
using Grimoire.GUI.Models.RF5.Define;
using Grimoire.GUI.Models.RF5.Loader.ID;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Grimoire.GUI.ViewModels
{

    public class AdvScriptWindowViewModel : ViewModelBase
    {
        private Dictionary<AdvScriptId, string> Scripts;
        private ObservableCollection<AdvScriptId> ScriptList { get; }

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
            var advIndexData = AssetsService.GetMonoBehaviourObject<AdvIndexData>((int)Master.ADVINDEXDATA);
            var pack = AssetsService.GetTextAssetObject((int)Event.PACK, Event.PACK.ToString());
            Scripts = AdvScriptService.DecompilePack(pack, advIndexData);
            ScriptList = new ObservableCollection<AdvScriptId>(Scripts.Keys);
            ScriptText = Scripts[selectedItem + 1];
        }
    }
}