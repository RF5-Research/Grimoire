using Grimoire.Unity.Addressables.ResourceLocators;
using Grimoire.Unity.Addressables.ResourceManager.ResourceLocations;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Grimoire.GUI.Core.Services
{
    public static class AddressablesService
    {
        private static ResourceLocationMap ResourceLocationMap;

        public static async Task InitializeAsync(string catalogPath, string addressableAssetsPath = null)
        {
            await Task.Run(() => Initialize(catalogPath, addressableAssetsPath));
        }

        public static void Initialize(string catalogPath, string addressableAssetsPath = null)
        {
            using (var fs = new FileStream(catalogPath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(fs))
            {
                var catalog = JsonSerializer.Deserialize<ContentCatalogData>(reader.ReadToEnd());
                ResourceLocationMap = catalog.CreateLocator(addressableAssetsPath: addressableAssetsPath);
            }
        }

        public static IResourceLocation LocateKey(string key)
        {
            IList<IResourceLocation> locs;
            ResourceLocationMap.Locate(key, typeof(object), out locs);
            //Need to check this further
            //But it's typically the first dep
            //https://github.com/needle-mirror/com.unity.addressables/blob/094f43386f79f60e87c9ab7198157bf8ddfc81cf/Runtime/ResourceManager/ResourceManager.cs#L329
            return locs[0];
        }

        public static string GetAssetBundlePath(IResourceLocation loc)
        {
            return loc.Dependencies[0].InternalId;
        }

        public static string GetKeyAssetBundlePath(string key)
        {
            var assetIndex = LocateKey(key);
            return GetAssetBundlePath(assetIndex);
        }
    }
}
