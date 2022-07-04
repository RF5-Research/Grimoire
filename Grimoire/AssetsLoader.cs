using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Grimoire.Models.RF5.Loader;
using Grimoire.Models.UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grimoire
{
    public static class AssetsLoader
    {
        public static AssetDataTable? AssetKeys;
        private const string RegionFreeKey = "All/";
        private static SystemLanguage Language;

        public static readonly Dictionary<SystemLanguage, string> GameLanguages = new()
        {
            { SystemLanguage.English, "Eng/" },
            { SystemLanguage.German, "Ger/" },
            { SystemLanguage.French, "Fre/" },
        };

        /// <summary>
        /// Required before usage
        /// </summary>
        public static void Initialize(SystemLanguage language)
        {
            Language = language;
            //They load via level0 -> AssetManager -> AssetReference and get the GUID as the key
            //This would require a dummy DLL dump of the assemblies
            //However, they already have the `AssetKeys` key in the Addressables so...
            var keyName = "AssetKeys";
            var am = new AssetsManager();
            AssetKeys = Addressables.LoadAsset<AssetDataTable>(am, keyName);
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
                return GameLanguages[Language];
            else
                return RegionFreeKey;
        }

        /// <summary>
        /// Loads the serialized assets of the assetID into the AssetsManager
        /// </summary>
        /// <param name="am"></param>
        /// <param name="assetID"></param>
        public static async Task<T?> LoadIDAsync<T>(int assetID, AssetsManager am)
        {
            am.UnloadAll();
            var rootKey = RootKey(assetID);
            var obj = await Addressables.LoadAssetAsync<T>(am, $"{rootKey}{assetID}");
            return obj;
        }

        /// <summary>
        /// Loads the serialized assets of the assetID into the AssetsManager
        /// </summary>
        /// <param name="am"></param>
        /// <param name="assetID"></param>
        public static T? LoadID<T>(int assetID, AssetsManager am)
        {
            am.UnloadAll();
            var rootKey = RootKey(assetID);
            var obj = Addressables.LoadAsset<T>(am, $"{rootKey}{assetID}");
            return obj;
        }

        ///// <summary>
        ///// Finds the specified serialized asset in the files loaded in the AssetManager
        ///// </summary>
        ///// <param name="am"></param>
        ///// <param name="assetName"></param>
        ///// <param name="assetClass"></param>
        ///// <returns></returns>
        //public static (AssetTypeInstance?, AssetsFileInstance?) FindSerializedAsset(AssetsManager am, string assetName, AssetClassID assetClass)
        //{
        //    foreach (var assetFile in am.files)
        //    {
        //        //This method returns a possible null
        //        var assetInfo = assetFile.table.GetAssetInfo(assetName, (uint)assetClass, false);

        //        //If was null Monobehaviour
        //        if (assetInfo == null && assetClass == AssetClassID.MonoBehaviour)
        //        {
        //            var monoscriptInfo = assetFile.table.GetAssetInfo(assetName, (uint)AssetClassID.MonoScript, false);
        //            if (monoscriptInfo != null)
        //            {
        //                var index = assetFile.file.preloadTable.items.FindIndex(x => x.pathID == monoscriptInfo.index);
        //                foreach (var monobehaviour in AssetHelper.GetAssetsOfType(assetFile.table, (int)AssetClassID.MonoBehaviour))
        //                {
        //                    if (AssetHelper.GetScriptIndex(assetFile.file, monobehaviour) == index)
        //                        return (am.GetTypeInstance(assetFile, monobehaviour), assetFile);
        //                }
        //            }
        //        }
        //        if (assetInfo != null)
        //        {
        //            return (am.GetTypeInstance(assetFile, assetInfo), assetFile);
        //        }
        //    }
        //    return (null, null);
        //}

        ///// <summary>
        ///// Returns the specified asset in the files loaded in the AssetManager
        ///// </summary>
        ///// <param name="am"></param>
        ///// <param name="assetName"></param>
        ///// <param name="assetClass"></param>
        ///// <returns></returns>
        //public static (AssetTypeInstance?, AssetsFileInstance?) GetAsset(AssetsManager am, string assetName, AssetClassID assetClass)
        //{
        //    foreach (var assetFile in am.files)
        //    {
        //        //This method returns a possible null
        //        var assetInfo = assetFile.table.GetAssetInfo(assetName, (uint)assetClass, false);

        //        //If was null Monobehaviour
        //        if (assetInfo == null && assetClass == AssetClassID.MonoBehaviour)
        //        {
        //            var monoscriptInfo = assetFile.table.GetAssetInfo(assetName, (uint)AssetClassID.MonoScript, false);
        //            if (monoscriptInfo != null)
        //            {
        //                var index = assetFile.file.preloadTable.items.FindIndex(x => x.pathID == monoscriptInfo.index);
        //                foreach (var monobehaviour in AssetHelper.GetAssetsOfType(assetFile.table, (int)AssetClassID.MonoBehaviour))
        //                {
        //                    if (AssetHelper.GetScriptIndex(assetFile.file, monobehaviour) == index)
        //                        return (am.GetTypeInstance(assetFile, monobehaviour), assetFile);
        //                }
        //            }
        //        }
        //        if (assetInfo != null)
        //        {
        //            return (am.GetTypeInstance(assetFile, assetInfo), assetFile);
        //        }
        //    }
        //    return (null, null);
        //}

        public static T? GetDeserializedObject<T>(AssetsManager am, AssetTypeInstance asset, AssetsFileInstance? assetFile = null)
        {
            return Serialization.DeserializeObject<T>(am, asset.GetBaseField(), assetFile);
        }

        public static byte[] GetTextAsset(AssetTypeInstance asset)
        {
            return asset.GetBaseField().Get("m_Script").GetValue().AsStringBytes();
        }
    }
}
