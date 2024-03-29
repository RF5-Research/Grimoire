﻿using GrimoireGUI.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

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

            Master = LoadEnum($"{loaderIDPath}/{nameof(Master)}.json");
            Event = LoadEnum($"{loaderIDPath}/{nameof(Event)}.json");
        }

        private static Dictionary<string, int> LoadEnum(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(fs);
            return JsonSerializer.Deserialize<Dictionary<string, int>>(reader.ReadToEnd())!;
        }
    }
}
