using Grimoire;
using GrimoireGUI.Core;
using System.IO;

namespace GrimoireGUI.Models
{
    public static class ProjectManager
    {
        public static Project Project { get; private set; }

        public static void Initialize(Project project)
        {
            Project = project;
            InitializeGlobalServices();
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
            path = path.Replace(Project.ROMPath, Project.ProjectPath);
            new FileInfo(path).Directory!.Create();
            return path;
        }

        public static void InitializeGlobalServices()
        {
            Application.Initialize(Project.ROMPath, Project.ProjectPath, Project.Platform);
            Addressables.Initialize();
            AssetsLoader.Initialize(Project.GameLanguage);
            LoaderID.Initialize();
            DefineID.Initialize();
        }
    }
}
