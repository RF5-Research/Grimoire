using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Grimoire.GUI.Models;
using Grimoire.GUI.Models.RF5.Loader;
using System.Linq;
using static Grimoire.Core.Serialization;

namespace Grimoire.GUI.Core.Services
{
    public static class LoaderService
    {
        public static AssetDataTable AssetKeys;
        private const string RegionFreeKey = "All/";

        /// <summary>
        /// Required for service usage
        /// </summary>
        public static void Initialize()
        {
            //They load via level0 -> AssetManager -> AssetReference and get the GUID as the key
            //This would require a dummy DLL dump of the assemblies
            //However, they already have the `AssetKeys` key in the Addressables so...
            var am = new AssetsManager();
            var keyName = "AssetKeys";
            AddressablesService.LoadSerializedAssets(am, keyName);
            (var assetKey, var assetFile) = FindSerializedAsset(am, keyName, AssetClassID.MonoBehaviour);
            AssetKeys = DeserializeObject<AssetDataTable>(am, assetKey.GetBaseField(), assetFile);
        }

        /// <summary>
        /// Returns whether or not the ID has language variations for the given region
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static bool HasLanguageVariants(int id)
        {
            return AssetKeys.AssetTables.First(x => x.Id == id).Region;
        }

        /// <summary>
        /// Returns the root key that will be concatenated to the ID 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string RootKey(int id)
        {
            if (HasLanguageVariants(id))
                return Settings.GameLanguages[ProjectManager.Project.GameLanguage];
            else
                return RegionFreeKey;
        }

        /// <summary>
        /// Loads the serialized assets of the assetID into the AssetsManager
        /// </summary>
        /// <param name="am"></param>
        /// <param name="assetID"></param>
        public static void LoadID(AssetsManager am, int assetID)
        {
            var rootKey = RootKey(assetID);
            AddressablesService.LoadSerializedAssets(am, $"{rootKey}{assetID}");
        }

        /// <summary>
        /// Finds the specified serialized asset in the files loaded in the AssetManager
        /// </summary>
        /// <param name="am"></param>
        /// <param name="assetName"></param>
        /// <param name="assetClass"></param>
        /// <returns></returns>
        public static (AssetTypeInstance?, AssetsFileInstance?) FindSerializedAsset(AssetsManager am, string assetName, AssetClassID assetClass)
        {
            foreach (var assetFile in am.files)
            {
                //This method returns a possible null
                var assetInfo = assetFile.table.GetAssetInfo(assetName, (uint)assetClass, false);

                //If was null Monobehaviour
                if (assetInfo == null && assetClass == AssetClassID.MonoBehaviour)
                {
                    var monoscriptInfo = assetFile.table.GetAssetInfo(assetName, (uint)AssetClassID.MonoScript, false);
                    var index = assetFile.file.preloadTable.items.FindIndex(x => x.pathID == monoscriptInfo.index);
                    if (monoscriptInfo != null)
                    {
                        foreach (var monobehaviour in AssetHelper.GetAssetsOfType(assetFile.table, (int)AssetClassID.MonoBehaviour))
                        {
                            if (AssetHelper.GetScriptIndex(assetFile.file, monobehaviour) == index)
                                return (am.GetTypeInstance(assetFile, monobehaviour), assetFile);
                        }
                    }
                }
                if (assetInfo != null)
                {
                    return (am.GetTypeInstance(assetFile, assetInfo), assetFile);
                }
            }
            return (null, null);
        }

        //public static void GetAssetInfo(AssetsManager am)
        //{
        //    am.
        //    foreach (var file in am.files)
        //    {

        //    }
        //}

        //public static void WriteMonoBehaviourObject<T>(int assetID, T obj, string subAssetName = null)
        //{
        //    var rootKey = RootKey(assetID);
        //    var key = $"{rootKey}{assetID}";
        //    var bundlePath = GetBundlePath(key);

        //    var am = new AssetsManager();
        //    var bundle = am.LoadBundleFile(bundlePath);
        //    var assetFile = am.LoadAssetsFileFromBundle(bundle, 0, true);
        //    subAssetName = subAssetName == null ? typeof(T).Name : subAssetName;
        //    var assetInfo = assetFile.table.GetAssetInfo(subAssetName, (int)AssetClassID.MonoBehaviour, caseSensitive: false);
        //    var baseField = am.GetTypeInstance(assetFile, assetInfo).GetBaseField();

        //    SerializeObject(obj, baseField, Target.Properties);

        //    var monoBehaviourBytes = baseField.WriteToByteArray();
        //    var repl = new AssetsReplacerFromMemory(
        //        0,
        //        assetInfo.index,
        //        (int)assetInfo.curFileType,
        //        AssetHelper.GetScriptIndex(assetFile.file, assetInfo),
        //        monoBehaviourBytes);

        //    byte[] newAssetData;
        //    using (var stream = new MemoryStream())
        //    using (var writer = new AssetsFileWriter(stream))
        //    {
        //        assetFile.file.Write(writer, 0, new List<AssetsReplacer>() { repl }, 0);
        //        newAssetData = stream.ToArray();
        //    }
        //    var bunRepl = new BundleReplacerFromMemory(assetFile.name, null, true, newAssetData, -1);

        //    using (var bunWriter = new AssetsFileWriter(File.Create(GetExportPath(bundlePath))))
        //    {
        //        bundle.file.Write(bunWriter, new List<BundleReplacer>() { bunRepl });
        //    }
        //}

        public static T? GetDeserializedObject<T>(AssetsManager am, AssetTypeInstance asset, AssetsFileInstance? assetFile = null)
        {
            return DeserializeObject<T>(am, asset.GetBaseField(), assetFile);
        }

        public static byte[] GetTextAsset(AssetTypeInstance asset)
        {
            return asset.GetBaseField().Get("m_Script").GetValue().AsStringBytes();
        }
    }
}
