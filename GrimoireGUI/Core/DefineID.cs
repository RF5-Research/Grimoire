using GrimoireGUI.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GrimoireGUI.Core
{
    public static class DefineID
    {
        public static Dictionary<string, int> AdvScriptId { get; set; }

        public static void Initialize()
        {
            var resourcePath = ProjectManager.Project.Platform == Grimoire.Platform.Switch ? "Resources/Switch" : "Resources/Steam";
            var loaderIDPath = $"{resourcePath}/Metadata/Define";

            AdvScriptId = LoadEnum($"{loaderIDPath}/{nameof(AdvScriptId)}.json");
        }

        private static Dictionary<string, int> LoadEnum(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(fs);
            return JsonSerializer.Deserialize<Dictionary<string, int>>(reader.ReadToEnd())!;
        }
    }
}
