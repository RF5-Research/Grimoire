using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Grimoire.GUI.Models;
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


        /// <summary>
        /// Required for service usage
        /// </summary>
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
            //But it's typically the first or w/e valid one comes first
            //https://github.com/needle-mirror/com.unity.addressables/blob/094f43386f79f60e87c9ab7198157bf8ddfc81cf/Runtime/ResourceManager/ResourceManager.cs#L329
            return locs[0];
        }

        /// <summary>
        /// Loads all the dependency bundles in the dependency tree of the key and each bundle's serialized assets into the AssetManager
        /// </summary>
        /// <param name="am"></param>
        /// <param name="key"></param>
        public static void LoadSerializedAssets(AssetsManager am, string key)
        {
            LoadBundles(am, key);
            foreach (var bundle in am.bundles)
            {
                LoadSerializedAssetFromBundle(am, bundle);
            }
        }

        /// <summary>
        /// Loads all the dependency bundles in the dependency tree of the key into the AssetManager
        /// </summary>
        public static void LoadBundles(AssetsManager am, string key)
        {
            var bundlePaths = GetBundlePaths(key);
            foreach (var bundlePath in bundlePaths)
            {
                var bundle = am.LoadBundleFile(bundlePath);
            }
        }

        /// <summary>
        /// Loads the serialized asset file from the bundle into the AssetManager
        /// </summary>
        /// <param name="am"></param>
        /// <param name="bundle"></param>
        public static void LoadSerializedAssetFromBundle(AssetsManager am, BundleFileInstance bundle)
        {
            int index = 0;
            //The serialized file is typically the first, but just to be safe
            for (int i = 0; i < bundle.file.bundleInf6.dirInf.Length; i++)
            {
                var dirInf = bundle.file.bundleInf6.dirInf;
                if (!dirInf[i].name.EndsWith(".resS") || dirInf[i].name.EndsWith(".resource"))
                {
                    index = i;
                    break;
                }
            }
            am.LoadAssetsFileFromBundle(bundle, index, true);
        }

        /// <summary>
        /// Returns a list of paths to all dependencies in the dependency tree of the asset key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<string> GetBundlePaths(string key)
        {
            var location = LocateKey(key);
            var bundles = new List<string>();
            foreach (var path in GetAllDependencies(location))
            {
                bundles.Add(ProjectManager.GetFilePath(path));
            }
            return bundles;
        }

        /// <summary>
        /// Returns all the bundle dependencies of the asset key including the dependencies of the dependencies in the dependency tree
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public static List<string> GetAllDependencies(IResourceLocation loc)
        {
            var dependencies = new List<string>();
            if (loc.HasDependencies)
            {
                foreach (var dep in loc.Dependencies)
                {
                    dependencies.Add(dep.InternalId);
                    //Load the dependency's dependencies as well
                    //Typically this is never needed
                    //dependencies.AddRange(GetAllDependencies(dep));
                }
            }
            return dependencies;
        }

        /// <summary>
        /// Returns only the root bundle dependencies of the asset key in the dependency tree
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public static List<string> GetRootDependencies(IResourceLocation loc)
        {
            var dependencies = new List<string>();
            if (loc.HasDependencies)
            {
                foreach (var dep in loc.Dependencies)
                {
                    dependencies.Add(dep.InternalId);
                }
            }
            return dependencies;
        }
    }
}
