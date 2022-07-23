using Grimoire;
using GrimoireGUI.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimoireGUI.ViewModels
{
    public class RigbarthAdvScriptWindowViewModel : ViewModelBase
    {
        private AdvScript AdvScript;

        [Reactive] public string ScriptText { get; set; }

        public RigbarthAdvScriptWindowViewModel()
        {
            var platform = ProjectManager.Project.Platform == Platform.Switch ? "Switch" : "Steam";
            //Need to fix this; Avalonia Designer does not like this
            AdvScript = new AdvScript($"Resources/{platform}/AdvScriptFunctions.json");
        }

        public List<CommandData> GetSymbols()
        {
            return AdvScript.Commands;
        }

        public void Save(string path)
        {
            //TODO: use an interaction instead
            AdvScript.SaveScript(ScriptText, path);
        }

        public void Load(string path)
        {
            //TODO: use an interaction instead
            ScriptText = AdvScript.LoadScript(path);
        }

    }
}
