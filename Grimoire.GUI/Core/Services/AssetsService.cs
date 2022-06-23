using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Grimoire.GUI.Models;
using Grimoire.GUI.Models.RF5.Loader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Grimoire.Core.Serialization;

namespace Grimoire.GUI.Core.Services
{
    public static class AssetsService
    {
        public static AssetDataTable AssetKeys;
        private const string RegionFreeKey = "All/";

        public static void Initialize()
        {
            //They load via level0 -> AssetManager -> AssetReference and get the GUID as the key
            //This would require a dummy DLL dump of the assemblies
            //However, they already have the `AssetKeys` key in the Addressables so...
            var am = new AssetsManager();
            var assetName = "AssetKeys";
            var bundlePath = AddressablesService.GetKeyAssetBundlePath(assetName);
            var bundle = am.LoadBundleFile(bundlePath);
            var asset = am.LoadAssetsFileFromBundle(bundle, 0, true);
            var info = asset.table.GetAssetInfo(assetName, (int)AssetClassID.MonoBehaviour, caseSensitive: false);
            var baseField = am.GetTypeInstance(asset, info).GetBaseField();
            AssetKeys = DeserializeObject<AssetDataTable>(baseField, Target.Properties);
        }

        public static bool HasRegionalVariants(int id)
        {
            return AssetKeys.AssetTables.First(x => x.Id == id).Region;
        }

        public static string RootKey(int id)
        {
            if (HasRegionalVariants(id))
                return Settings.GameLanguages[ProjectManager.Project.GameLanguage];
            else
                return RegionFreeKey;
        }

        public static string GetBundlePath(string key)
        {
            var path = AddressablesService.GetKeyAssetBundlePath(key);
            var projectPath = GetExportPath(path);
            if (File.Exists(projectPath))
                return projectPath;
            else
                return path;
        }

        public static string GetExportPath(string path)
        {
            path = path.Replace(ProjectManager.Project.ROMPath, ProjectManager.Project.ProjectPath);
            new FileInfo(path).Directory!.Create();
            return path;
        }

        public static void WriteMonoBehaviourObject<T>(int assetID, T obj, string subAssetName = null)
        {
            var rootKey = RootKey(assetID);
            var key = $"{rootKey}{assetID}";
            var bundlePath = GetBundlePath(key);

            var am = new AssetsManager();
            var bundle = am.LoadBundleFile(bundlePath);
            var assetFile = am.LoadAssetsFileFromBundle(bundle, 0, true);
            subAssetName = subAssetName == null ? typeof(T).Name : subAssetName;
            var assetInfo = assetFile.table.GetAssetInfo(subAssetName, (int)AssetClassID.MonoBehaviour, caseSensitive: false);
            var baseField = am.GetTypeInstance(assetFile, assetInfo).GetBaseField();

            SerializeObject(obj, baseField, Target.Properties);
            
            var monoBehaviourBytes = baseField.WriteToByteArray();
            var repl = new AssetsReplacerFromMemory(
                0,
                assetInfo.index,
                (int)assetInfo.curFileType,
                AssetHelper.GetScriptIndex(assetFile.file, assetInfo),
                monoBehaviourBytes);

            byte[] newAssetData;
            using (var stream = new MemoryStream())
            using (var writer = new AssetsFileWriter(stream))
            {
                assetFile.file.Write(writer, 0, new List<AssetsReplacer>() { repl }, 0);
                newAssetData = stream.ToArray();
            }
            var bunRepl = new BundleReplacerFromMemory(assetFile.name, null, true, newAssetData, -1);

            using (var bunWriter = new AssetsFileWriter(File.Create(GetExportPath(bundlePath))))
            {
                bundle.file.Write(bunWriter, new List<BundleReplacer>() { bunRepl });
            }
        }

        public static T GetMonoBehaviourObject<T>(int assetID, string? subAssetName = null)
        {
            var rootKey = RootKey(assetID);
            var bundlePath = GetBundlePath($"{rootKey}{assetID}");

            var am = new AssetsManager();
            var bundle = am.LoadBundleFile(bundlePath);
            var assetFile = am.LoadAssetsFileFromBundle(bundle, 0, true);
            subAssetName = subAssetName == null ? typeof(T).Name : subAssetName;
            var assetInfo = assetFile.table.GetAssetInfo(subAssetName, (int)AssetClassID.MonoBehaviour, caseSensitive: false);
            var baseField = am.GetTypeInstance(assetFile, assetInfo).GetBaseField();

            return DeserializeObject<T>(baseField, Target.Properties);
        }

        public static byte[] GetTextAssetObject(int assetID, string subAssetName)
        {
            var rootKey = RootKey(assetID);
            var bundlePath = GetBundlePath($"{rootKey}{assetID}");

            var am = new AssetsManager();
            var bundle = am.LoadBundleFile(bundlePath);
            var assetFile = am.LoadAssetsFileFromBundle(bundle, 0, true);
            var assetInfo = assetFile.table.GetAssetInfo(subAssetName, (int)AssetClassID.TextAsset, caseSensitive: false);
            return am.GetTypeInstance(assetFile, assetInfo).GetBaseField().Get("m_Script").GetValue().AsStringBytes();
        }
    }
}
