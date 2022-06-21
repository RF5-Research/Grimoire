using AssetsTools.NET.Extra;
using Loader;

namespace Grimoire.Core
{
    public static class AssetService
    {
        public static AssetDataTable AssetKeys;
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

            AssetKeys = Serialization.DeserializeObject<AssetDataTable>(baseField);
        }
    }
}
