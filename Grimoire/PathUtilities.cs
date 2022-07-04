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
    }
}
