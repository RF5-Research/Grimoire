using Grimoire.Models.RF5.Loader.ID;
using GrimoireGUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GrimoireGUI.Core
{
    public static class LoaderID
    {
        public static Dictionary<string, int> Master { get; set; }
        public static Dictionary<string, int> Event { get; set; }

        public static void Initialize()
        {
            var resourcePath = ProjectManager.Project.Platform == Grimoire.Platform.Switch ? "Resources/Switch" : "Resources/Steam";
            var loaderIDPath = $"{resourcePath}/Metadata/Loader/ID";
            LoadEnum(Master, $"{loaderIDPath}/{nameof(Master)}");
            LoadEnum(Event, $"{loaderIDPath}/{nameof(Event)}");
        }

        public static void LoadEnum(Dictionary<string, int> @enum, string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(fs))
            {
                @enum = JsonSerializer.Deserialize<Dictionary<string, int>>(reader.ReadToEnd())!;
            }
        }
    }
}
