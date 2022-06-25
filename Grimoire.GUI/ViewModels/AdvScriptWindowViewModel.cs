using AssetsTools.NET.Extra;
using Grimoire.Core;
using Grimoire.GUI.Core.Services;
using Grimoire.GUI.Models.RF5;
using Grimoire.GUI.Models.RF5.Define;
using Grimoire.GUI.Models.RF5.Loader.ID;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static Grimoire.Core.Serialization;

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
            var am = new AssetsManager();
            LoaderService.LoadID(am, (int)Master.ADVINDEXDATA);
            (var advIndexDataAsset, var advIndexDataAssetFile) = LoaderService.FindSerializedAsset(am, Master.ADVINDEXDATA.ToString(), AssetClassID.MonoBehaviour);
            var advIndexData = LoaderService.GetDeserializedObject<AdvIndexData>(am, advIndexDataAsset, advIndexDataAssetFile);
            am.UnloadAll();

            LoaderService.LoadID(am, (int)Event.PACK);
            (var packAsset, var _) = LoaderService.FindSerializedAsset(am, Event.PACK.ToString(), AssetClassID.TextAsset);
            var pack = LoaderService.GetTextAsset(packAsset);
            am.UnloadAll();

            Scripts = AdvScriptService.DecompilePack(pack, advIndexData);
            ScriptList = new ObservableCollection<AdvScriptId>(Scripts.Keys);
            ScriptText = Scripts[selectedItem + 1];
        }
    }
}