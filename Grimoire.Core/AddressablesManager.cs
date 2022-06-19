using Grimoire.Unity.Addressables.ResourceLocators;
using Grimoire.Unity.Addressables.ResourceManager.ResourceLocations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Grimoire
{
    public static class AddressablesManager
    {
        private static ResourceLocationMap ResourceLocationMap;

        public static async Task InitializeAsync(string catalogPath, string path = null)
        {
            await Task.Run(() => Initialize(catalogPath, path));
        }

        public static void Initialize(string catalogPath, string path = null)
        {
            using (var fs = new FileStream(catalogPath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(fs))
            {
                var catalog = JsonSerializer.Deserialize<ContentCatalogData>(reader.ReadToEnd());
                ResourceLocationMap = catalog.CreateLocator(addressableAssetsPath: path);
            }
        }

        public static IResourceLocation LocateKey(string key)
        {
            IList<IResourceLocation> locs;
            ResourceLocationMap.Locate(key, typeof(object), out locs);
            //Need to check this further
            //https://github.com/needle-mirror/com.unity.addressables/blob/094f43386f79f60e87c9ab7198157bf8ddfc81cf/Runtime/ResourceManager/ResourceManager.cs#L329
            return locs[0];
        }
    }
}
