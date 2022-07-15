using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grimoire
{
    public static class Application
    {
        public static string? ROMPath;
        public static string? ProjectPath;
        public static string DataPath;
        public static string StreamingAssetsPath { get => $"{DataPath}/StreamingAssets"; }
        public static Platform Platform;

        public static void Initialize(string romPath, string projectPath, Platform platform)
        {
            //Hardcoding
            DataPath = platform == Platform.Switch ? $"{romPath}/Data" : $"{romPath}/Rune Factory 5_Data";
            ROMPath = romPath;
            ProjectPath = projectPath;
            Platform = platform;
        }
    }
}
