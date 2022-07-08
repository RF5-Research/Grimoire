using System.IO;

namespace Grimoire
{
    internal class PathUtilities
    {
        public static string GetFilePath(string fileROMPath, string romPath, string projectPath)
        {
            var fileProjectPath = fileROMPath.Replace(romPath, projectPath);
            return File.Exists(fileProjectPath) ? fileProjectPath : fileROMPath;
        }

        /// <summary>
        /// Returns the export path in the project directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetExportPath(string fileROMPath)
        {
            fileROMPath = Path.GetFullPath(fileROMPath.Replace(Path.GetFullPath(Addressables.ROMPath), Path.GetFullPath(Addressables.ProjectPath)));
            new FileInfo(fileROMPath).Directory!.Create();
            return fileROMPath;
        }

    }
}
