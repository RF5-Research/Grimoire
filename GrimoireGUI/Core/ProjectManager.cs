using Grimoire;
using GrimoireGUI.Core;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GrimoireGUI.Models
{
    public static class ProjectManager
    {
        public static Project Project { get; private set; }

        public static async Task InitializeAsync(Project project, CancellationTokenSource cts)
        {
            Project = project;
            await InitializeServicesAsync(cts);
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
            return File.Exists(exportPath) ? exportPath : path;
        }

        /// <summary>
        /// Returns the export path in the project directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetExportPath(string path)
        {
            path = path.Replace(Project.GamePath, Project.ProjectPath);
            new FileInfo(path).Directory!.Create();
            return path;
        }

        public static async Task InitializeServicesAsync(CancellationTokenSource cts)
        {
            var token = cts.Token;
            var tasks = new List<Task>()
            {
                Task.Run(() => LoaderID.Initialize(), token),
                Task.Run(() => DefineID.Initialize(), token)
            };
            token.ThrowIfCancellationRequested();
            Application.Initialize(Project.GamePath, Project.ProjectPath, Project.Platform);
            token.ThrowIfCancellationRequested();
            Addressables.Initialize();
            token.ThrowIfCancellationRequested();
            AssetsLoader.Initialize(Project.GameLanguage);
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(tasks);
        }
    }
}
