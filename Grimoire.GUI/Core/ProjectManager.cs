using Grimoire.GUI.Core.Services;
using System.IO;
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
            await InitializeGlobalServices();
            //Add services
        }


        /// <summary>
        /// Determines and gets which file to be loaded.
        /// Returns path in project directory if it exists
        /// else will return from the ROM directory.
        /// Should be used for every reads
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFilePath(string path)
        {
            var exportPath = GetExportPath(path);
            if (File.Exists(exportPath))
                return exportPath;
            else
                return path;
        }

        /// <summary>
        /// Returns the export path in the project directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetExportPath(string path)
        {
            path = path.Replace(Project.ROMPath, Project.ProjectPath);
            new FileInfo(path).Directory!.Create();
            return path;
        }

        public static async Task InitializeGlobalServices()
        {
            await Task.WhenAll(
                AddressablesService.InitializeAsync($"{AddressableAssetsPath}/catalog.json", AddressableAssetsPath),
                AdvScriptService.InitializeAsnyc("Resources/AdvScriptFunctions.json")
            );
            //Relies on AddressablesService
            LoaderService.Initialize();
        }
    }
}
