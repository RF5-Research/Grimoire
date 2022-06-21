﻿using System.Threading.Tasks;

namespace Grimoire.GUI.Models
{
    public static class ProjectManager
    {
        public static Project Project { get; private set; }
        //Prob add all refs to services here
        public static string DataPath { get => $"{Project.ROMPath}/Data"; }

        public static string StreamingAssetsPath { get => $"{DataPath}/StreamingAssets"; }

        public static string AddressableAssetsPath { get => $"{StreamingAssetsPath}/aa"; }

        public static async Task Initialize(Project project)
        {
            Project = project;
            await InitializeGlobalServices();
        }

        public static async Task InitializeGlobalServices()
        {
            await Task.WhenAll(
                AddressablesService.InitializeAsync($"{AddressableAssetsPath}/catalog.json", AddressableAssetsPath)
            );
        }
    }
}
