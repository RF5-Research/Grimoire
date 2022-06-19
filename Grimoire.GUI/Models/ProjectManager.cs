using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grimoire.GUI.Models
{
    public static class ProjectManager
    {
        public static Project Project { get; private set; }

        public static string DataPath { get => $"{Project.ROMPath}/Data"; }

        public static string StreamingAssetsPath { get => $"{DataPath}/StreamingAssets"; }

        public static string AddressableAssetsPath { get => $"{StreamingAssetsPath}/aa"; }

        public static async Task Initialize(Project project)
        {
            Project = project;
            await InitializeGlobalManagers();
        }

        public static async Task InitializeGlobalManagers()
        {
            await Task.WhenAll(
                AddressablesManager.InitializeAsync($"{AddressableAssetsPath}/catalog.json", AddressableAssetsPath)
            );
        }
    }
}
