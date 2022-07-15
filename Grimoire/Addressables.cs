using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Grimoire.Unity.Addressables.ResourceLocators;
using Grimoire.Unity.Addressables.ResourceManager.ResourceLocations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Grimoire
{
    public static class Addressables
    {
        public static string AddressableAssetsPath { get => $"{Application.StreamingAssetsPath}/aa"; }
        public static ResourceLocationMap ResourceLocationMap;
        /// <summary>
        /// Required before usage
        /// </summary>
        public static void Initialize()
        {
            string catalogPath = $"{AddressableAssetsPath}/catalog.json";
            using (var fs = new FileStream(catalogPath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(fs))
            {
                var catalog = JsonSerializer.Deserialize<ContentCatalogData>(reader.ReadToEnd());
                if (catalog != null)
                    ResourceLocationMap = catalog.CreateLocator();
                else
                    throw new Exception("Can't find `catalog.json`");
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
        public static T? LoadAsset<T>(AssetsManager am, string key)
        {
            var loc = LocateKey(key);
            if (loc != null)
            {
                LoadBundles(am, loc);
                foreach (var bundle in am.bundles)
                {
                    if (bundle != null)
                        LoadSerializedAssetFromBundle(am, bundle);
                }
                return GetDeserializedObject<T>(am, loc.InternalId);
            }
            return default;
        }

        /// <summary>
        /// Loads all the dependency bundles in the dependency tree of the key and each bundle's serialized assets into the AssetManager
        /// </summary>
        /// <param name="am"></param>
        /// <param name="key"></param>
        public static async Task<T?> LoadAssetAsync<T>(AssetsManager am, string key)
        {
            var loc = LocateKey(key);
            await LoadBundlesAsync(am, loc);
            var tasks = new List<Task>(am.bundles.Count);

            var parentName = loc.Dependencies.First().PrimaryKey;
            //Async doesn't gurantee order so load the first file synchronously as that's important
            var parentBundle = am.bundles.Find(x => x.name == parentName);
            if (parentBundle != null)
                LoadSerializedAssetFromBundle(am, parentBundle);
            else
                throw new Exception("Couldn't find parent bundle");

            foreach (var bundle in am.bundles)
            {
                if (bundle != parentBundle)
                {
                    tasks.Add(Task.Run(() => LoadSerializedAssetFromBundle(am, bundle)));
                }
            }
            await Task.WhenAll(tasks);

            return GetDeserializedObject<T>(am, loc.InternalId);
        }

        public static void WriteAsset<T>(AssetsManager am, T assetObj, string key)
        {
            var loc = LocateKey(key);
            if (loc != null)
            {
                LoadBundles(am, loc);
                foreach (var bundle in am.bundles)
                {
                    if (bundle != null)
                        LoadSerializedAssetFromBundle(am, bundle);
                }
                WriteSerializedObject<T>(am, assetObj, loc.InternalId);
            }
        }

        public static void WriteSerializedObject<T>(AssetsManager am, T assetObj, string assetName)
        {
            //https://github.com/needle-mirror/com.unity.addressables/blob/094f43386f79f60e87c9ab7198157bf8ddfc81cf/Runtime/ResourceManager/ResourceProviders/BundledAssetProvider.cs#L24
            //They load first bundle, which has the first serialized file
            var file = am.files[0];
            var abInfo = file.table.GetAssetsOfType((int)AssetClassID.AssetBundle).First();
            var abBaseField = am.GetTypeInstance(file, abInfo).GetBaseField();

            var m_Container = abBaseField.Get("m_Container").Get("Array");
            foreach (var data in m_Container.children)
            {
                if (assetName == data[0].GetValue().AsString())
                {
                    var asset = am.GetExtAsset(file, data[1].Get("asset"));
                    var baseField = asset.instance.GetBaseField();
                    Serialization.SerializeObject<T>(assetObj, am, asset.instance.GetBaseField(), file);

                    var assetBytes = baseField.WriteToByteArray();
                    var repl = new AssetsReplacerFromMemory(0, asset.info.index, (int)asset.info.curFileType, AssetHelper.GetScriptIndex(asset.file.file, asset.info), assetBytes);

                    byte[] newAssetData;
                    using (var stream = new MemoryStream())
                    using (var writer = new AssetsFileWriter(stream))
                    {
                        asset.file.file.Write(writer, 0, new List<AssetsReplacer>() { repl }, 0);
                        newAssetData = stream.ToArray();
                    }
                    var bunRepl = new BundleReplacerFromMemory(asset.file.name, null, true, newAssetData, -1);

                    var bundle = asset.file.parentBundle;
                    //Unload after
                    am.UnloadAll();
                    using (var bunWriter = new AssetsFileWriter(File.Create(PathUtilities.GetExportPath(bundle.path))))
                    {
                        bundle.file.Write(bunWriter, new List<BundleReplacer>() { bunRepl });
                    }
                    break;
                }
            }
        }

        public static T? GetDeserializedObject<T>(AssetsManager am, string assetName)
        {
            //https://github.com/needle-mirror/com.unity.addressables/blob/094f43386f79f60e87c9ab7198157bf8ddfc81cf/Runtime/ResourceManager/ResourceProviders/BundledAssetProvider.cs#L24
            //They load first bundle, which has the first serialized file
            var file = am.files[0];
            var abInfo = file.table.GetAssetsOfType((int)AssetClassID.AssetBundle).First();
            var abBaseField = am.GetTypeInstance(file, abInfo).GetBaseField();

            var m_Container = abBaseField.Get("m_Container").Get("Array");
            foreach (var data in m_Container.children)
            {
                if (assetName == data[0].GetValue().AsString())
                {
                    var asset = am.GetExtAsset(file, data[1].Get("asset"));
                    return Serialization.DeserializeObject<T>(am,  asset.instance.GetBaseField(), file);
                }
            }
            return default(T);
        }

        public static string GetRuntimePath(string path)
        {
            return path.Replace("UnityEngine.AddressableAssets.Addressables.RuntimePath", Path.GetFullPath($"{Application.DataPath}/StreamingAssets/aa"));
        }

        /// <summary>
        /// Loads all the dependency bundles in the dependency tree of the key into the AssetManager
        /// </summary>
        public static void LoadBundles(AssetsManager am, IResourceLocation loc)
        {
            var bundlePaths = GetBundlePaths(loc);
            foreach (var bundlePath in bundlePaths)
            {
                am.LoadBundleFile(bundlePath);
            }
        }

        /// <summary>
        /// Loads all the dependency bundles in the dependency tree of the key into the AssetManager
        /// </summary>
        public static async Task LoadBundlesAsync(AssetsManager am, IResourceLocation loc)
        {
            var bundlePaths = GetBundlePaths(loc);
            var tasks = new List<Task>(bundlePaths.Count);
            foreach (var bundlePath in bundlePaths)
            {
                tasks.Add(Task.Run(() => am.LoadBundleFile(bundlePath)));
            }
            await Task.WhenAll(tasks);
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
        public static List<string> GetBundlePaths(IResourceLocation loc)
        {
            var dependencies = GetAllDependencies(loc);
            var bundles = new List<string>(dependencies.Count);
            foreach (var path in dependencies)
            {
                bundles.Add(PathUtilities.GetFilePath(path, Application.ROMPath, Application.ProjectPath));
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
                dependencies.Capacity = loc.Dependencies.Count;
                foreach (var dep in loc.Dependencies)
                {
                    dependencies.Add(GetRuntimePath(dep.InternalId));
                    //Load the dependency's dependencies as well
                    //Typically this is never needed
                    //dependencies.AddRange(GetAllDependencies(dep));
                }
            }
            return dependencies;
        }
    }
}
