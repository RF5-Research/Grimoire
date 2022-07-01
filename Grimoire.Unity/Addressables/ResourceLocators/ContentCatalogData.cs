using Grimoire.Unity.Addressables.ResourceManager.ResourceLocations;
using Grimoire.Unity.Addressables.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Grimoire.Unity.Addressables.ResourceLocators
{
    public class ContentCatalogDataEntry
    {
        /// <summary>
        /// Internl id.
        /// </summary>
        public string InternalId { get; set; }
        /// <summary>
        /// IResourceProvider identifier.
        /// </summary>
        public string Provider { get; private set; }
        /// <summary>
        /// Keys for this location.
        /// </summary>
        public List<object> Keys { get; private set; }
        /// <summary>
        /// Dependency keys.
        /// </summary>
        public List<object> Dependencies { get; private set; }
        /// <summary>
        /// Serializable data for the provider.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// The type of the resource for th location.
        /// </summary>
        public Type ResourceType { get; private set; }

        /// <summary>
        /// Creates a new ContentCatalogEntry object.
        /// </summary>
        /// <param name="type">The entry type.</param>
        /// <param name="internalId">The internal id.</param>
        /// <param name="provider">The provider id.</param>
        /// <param name="keys">The collection of keys that can be used to retrieve this entry.</param>
        /// <param name="dependencies">Optional collection of keys for dependencies.</param>
        /// <param name="extraData">Optional additional data to be passed to the provider.  For example, AssetBundleProviders use this for cache and crc data.</param>
        public ContentCatalogDataEntry(Type type, string internalId, string provider, IEnumerable<object> keys, IEnumerable<object> dependencies = null, object extraData = null)
        {
            InternalId = internalId;
            Provider = provider;
            ResourceType = type;
            Keys = new List<object>(keys);
            Dependencies = dependencies == null ? new List<object>() : new List<object>(dependencies);
            Data = extraData;
        }
    }

    /// <summary>
    /// Container for ContentCatalogEntries.
    /// </summary>
    public class ContentCatalogData
    {
        [JsonIgnore]
        internal string localHash;
        [JsonIgnore]
        internal IResourceLocation location;
        [JsonInclude]
        //internal string m_LocatorId;
        public string m_LocatorId;

        /// <summary>
        /// Stores the id of the data provider.
        /// </summary>
        [JsonIgnore]
        public string ProviderId
        {
            get { return m_LocatorId; }
            internal set { m_LocatorId = value; }
        }

        [JsonInclude]
        //string m_InstanceProviderData;
        public object m_InstanceProviderData;
        /// <summary>
        /// Data for the Addressables.ResourceManager.InstanceProvider initialization;
        /// </summary>
        [JsonIgnore]
        public object InstanceProviderData
        {
            get
            {
                return m_InstanceProviderData;
            }
            set
            {
                m_InstanceProviderData = value;
            }
        }
        [JsonInclude]
        //string m_SceneProviderData;
        public object m_SceneProviderData;
        /// <summary>
        /// Data for the Addressables.ResourceManager.InstanceProvider initialization;
        /// </summary>
        [JsonIgnore]
        public object SceneProviderData
        {
            get
            {
                return m_SceneProviderData;
            }
            set
            {
                m_SceneProviderData = value;
            }
        }
        [JsonInclude]
        //internal List<string> m_ResourceProviderData = new List<string>();
        public List<object> m_ResourceProviderData = new List<object>();
        /// <summary>
        /// The list of resource provider data.  Each entry will add an IResourceProvider to the Addressables.ResourceManager.ResourceProviders list.
        /// </summary>
        [JsonIgnore]
        public List<object> ResourceProviderData
        {
            get { return m_ResourceProviderData; }
            set { m_ResourceProviderData = value; }
        }

        /// <summary>
        /// The IDs for the Resource Providers.
        /// </summary>
        [JsonIgnore]
        public string[] ProviderIds
        {
            get { return m_ProviderIds; }
        }

        /// <summary>
        /// Internal Content Catalog Entry IDs for Addressable Assets.
        /// </summary>
        [JsonIgnore]
        public string[] InternalIds
        {
            get { return m_InternalIds; }
        }

        [JsonInclude]
        //internal string[] m_ProviderIds = null;
        public string[] m_ProviderIds = null;
        [JsonInclude]
        //internal string[] m_InternalIds = null;
        public string[] m_InternalIds = null;
        [JsonInclude]
        //internal string m_KeyDataString = null;
        public string m_KeyDataString = null;
        [JsonInclude]
        //internal string m_BucketDataString = null;
        public string m_BucketDataString = null;
        [JsonInclude]
        //internal string m_EntryDataString = null;
        public string m_EntryDataString = null;

        const int kBytesPerInt32 = 4;
        const int k_EntryDataItemPerEntry = 7;

        [JsonInclude]
        public string m_ExtraDataString = null;
        //internal string m_ExtraDataString = null;

        [JsonInclude]
        //internal string[] m_resourceTypes = null;
        public object[] m_resourceTypes = null;

        [JsonInclude]
        //string[] m_InternalIdPrefixes = null;
        public string[] m_InternalIdPrefixes = null;

        struct Bucket
        {
            public int dataOffset;
            public int[] entries;
        }

        class CompactLocation : IResourceLocation
        {
            ResourceLocationMap m_Locator;
            string m_InternalId;
            string m_ProviderId;
            object m_Dependency;
            object m_Data;
            int m_HashCode;
            int m_DependencyHashCode;
            string m_PrimaryKey;
            Type m_Type;

            public string InternalId { get { return m_InternalId; } }
            public string ProviderId { get { return m_ProviderId; } }
            public IList<IResourceLocation> Dependencies
            {
                get
                {
                    if (m_Dependency == null)
                        return null;
                    IList<IResourceLocation> results;
                    m_Locator.Locate(m_Dependency, typeof(object), out results);
                    return results;
                }
            }
            public bool HasDependencies { get { return m_Dependency != null; } }

            public int DependencyHashCode { get { return m_DependencyHashCode; } }

            public object Data { get { return m_Data; } }

            public string PrimaryKey
            {
                get { return m_PrimaryKey; }
                set { m_PrimaryKey = value; }
            }

            public Type ResourceType { get { return m_Type; } }

            public override string ToString()
            {
                return m_InternalId;
            }

            public int Hash(Type t)
            {
                return (m_HashCode * 31 + t.GetHashCode()) * 31 + DependencyHashCode;
            }

            public CompactLocation(ResourceLocationMap locator, string internalId, string providerId, object dependencyKey, object data, int depHash, string primaryKey, Type type)
            {
                m_Locator = locator;
                m_InternalId = internalId;
                m_ProviderId = providerId;
                m_Dependency = dependencyKey;
                m_Data = data;
                m_HashCode = internalId.GetHashCode() * 31 + providerId.GetHashCode();
                m_DependencyHashCode = depHash;
                m_PrimaryKey = primaryKey;
                m_Type = type == null ? typeof(object) : type;
            }
        }
        //https://github.com/needle-mirror/com.unity.addressables/blob/094f43386f79f60e87c9ab7198157bf8ddfc81cf/Runtime/AddressablesImpl.cs#L571
        //internal void CleanData()
        //{
        //    m_KeyDataString = "";
        //    m_BucketDataString = "";
        //    m_EntryDataString = "";
        //    m_ExtraDataString = "";
        //    m_InternalIds = null;
        //    m_LocatorId = "";
        //    m_ProviderIds = null;
        //    m_ResourceProviderData = null;
        //    m_resourceTypes = null;
        //}

        //internal ResourceLocationMap CreateCustomLocator(string overrideId, string providerSuffix = null)
        //{
        //    m_LocatorId = overrideId;
        //    return CreateLocator(providerSuffix);
        //}

        /// <summary>
        /// Create IResourceLocator object
        /// </summary>
        /// <param name="providerSuffix">If specified, this value will be appeneded to all provider ids.  This is used when loading additional catalogs that need to have unique providers.</param>
        /// <returns>ResourceLocationMap, which implements the IResourceLocator interface.</returns>
        public ResourceLocationMap CreateLocator(string providerSuffix = null)
        {
            //var bucketData = Convert.FromBase64String(m_BucketDataString);
            //Bucket[] buckets;
            //using (var ms = new MemoryStream(bucketData))
            //using (var reader = new BinaryReader(ms))
            //{
            //    var bucketCount = reader.ReadInt32();
            //    buckets = new Bucket[bucketCount];
            //    for (int i = 0; i < bucketCount; i++)
            //    {
            //        var index = reader.ReadInt32();
            //        var entryCount = reader.ReadInt32();
            //        var entryArray = new int[entryCount];
            //        for (int c = 0; c < entryCount; c++)
            //        {
            //            entryArray[c] = reader.ReadInt32();
            //        }
            //        buckets[i] = new Bucket { entries = entryArray, dataOffset = index };
            //    }
            //}
            //if (!string.IsNullOrEmpty(providerSuffix))
            //{
            //    for (int i = 0; i < m_ProviderIds.Length; i++)
            //    {
            //        if (!m_ProviderIds[i].EndsWith(providerSuffix, StringComparison.Ordinal))
            //            m_ProviderIds[i] = m_ProviderIds[i] + providerSuffix;
            //    }
            //}
            //var extraData = Convert.FromBase64String(m_ExtraDataString);

            //var keyData = Convert.FromBase64String(m_KeyDataString);
            //object[] keys;
            //using (var ms = new MemoryStream(keyData))
            //using (var reader = new BinaryReader(ms))
            //{
            //    var keyCount = reader.ReadInt32();
            //    keys = new object[keyCount];
            //    for (int i = 0; i < buckets.Length; i++)
            //    {
            //        reader.BaseStream.Position = buckets[i].dataOffset;
            //        keys[i] = reader.ReadInt32();
            //    }
            //}

            //var locator = new ResourceLocationMap(m_LocatorId, buckets.Length);

            //var entryData = Convert.FromBase64String(m_EntryDataString);
            //IResourceLocation[] locations;

            //using (var ms = new MemoryStream(entryData))
            //using (var reader = new BinaryReader(ms))
            //{
            //    int count = reader.ReadInt32();
            //    locations = new IResourceLocation[count];
            //    for (int i = 0; i < count; i++)
            //    {
            //        var internalId = reader.ReadInt32();
            //        var providerIndex = reader.ReadInt32();
            //        var dependencyKeyIndex = reader.ReadInt32();
            //        var depHash = reader.ReadInt32();
            //        var dataIndex = reader.ReadInt32();
            //        var primaryKey = reader.ReadInt32();
            //        var resourceType = reader.ReadInt32();
            //        object data = null;
            //        //object data = dataIndex < 0 ? null : SerializationUtilities.ReadObjectFromByteArray(extraData, dataIndex);
            //        locations[i] = new CompactLocation(locator, ResolveInternalId(ExpandInternalId(m_InternalIdPrefixes, m_InternalIds[internalId])),
            //            m_ProviderIds[providerIndex], dependencyKeyIndex < 0 ? null : keys[dependencyKeyIndex], data, depHash, keys[primaryKey].ToString(), null/*m_resourceTypes[resourceType].Value*/);
            //    }
            //}

            //for (int i = 0; i < buckets.Length; i++)
            //{
            //    var bucket = buckets[i];
            //    var key = keys[i];
            //    var locs = new IResourceLocation[bucket.entries.Length];
            //    for (int b = 0; b < bucket.entries.Length; b++)
            //        locs[b] = locations[bucket.entries[b]];
            //    locator.Add(key, locs);
            //}

            //return locator;
            var bucketData = Convert.FromBase64String(m_BucketDataString);
            int bucketCount = BitConverter.ToInt32(bucketData, 0);
            var buckets = new Bucket[bucketCount];
            int bi = 4;
            for (int i = 0; i < bucketCount; i++)
            {
                var index = SerializationUtilities.ReadInt32FromByteArray(bucketData, bi);
                bi += 4;
                var entryCount = SerializationUtilities.ReadInt32FromByteArray(bucketData, bi);
                bi += 4;
                var entryArray = new int[entryCount];
                for (int c = 0; c < entryCount; c++)
                {
                    entryArray[c] = SerializationUtilities.ReadInt32FromByteArray(bucketData, bi);
                    bi += 4;
                }
                buckets[i] = new Bucket { entries = entryArray, dataOffset = index };
            }
            if (!string.IsNullOrEmpty(providerSuffix))
            {
                for (int i = 0; i < m_ProviderIds.Length; i++)
                {
                    if (!m_ProviderIds[i].EndsWith(providerSuffix, StringComparison.Ordinal))
                        m_ProviderIds[i] = m_ProviderIds[i] + providerSuffix;
                }
            }
            var extraData = Convert.FromBase64String(m_ExtraDataString);

            var keyData = Convert.FromBase64String(m_KeyDataString);
            var keyCount = BitConverter.ToInt32(keyData, 0);
            var keys = new object[keyCount];
            for (int i = 0; i < buckets.Length; i++)
                //This is where I messed up...
                keys[i] = SerializationUtilities.ReadObjectFromByteArray(keyData, buckets[i].dataOffset);

            var locator = new ResourceLocationMap(m_LocatorId, buckets.Length);

            var entryData = Convert.FromBase64String(m_EntryDataString);
            int count = SerializationUtilities.ReadInt32FromByteArray(entryData, 0);
            var locations = new IResourceLocation[count];
            for (int i = 0; i < count; i++)
            {
                var index = kBytesPerInt32 + i * (kBytesPerInt32 * k_EntryDataItemPerEntry);
                var internalId = SerializationUtilities.ReadInt32FromByteArray(entryData, index);
                index += kBytesPerInt32;
                var providerIndex = SerializationUtilities.ReadInt32FromByteArray(entryData, index);
                index += kBytesPerInt32;
                var dependencyKeyIndex = SerializationUtilities.ReadInt32FromByteArray(entryData, index);
                index += kBytesPerInt32;
                var depHash = SerializationUtilities.ReadInt32FromByteArray(entryData, index);
                index += kBytesPerInt32;
                var dataIndex = SerializationUtilities.ReadInt32FromByteArray(entryData, index);
                index += kBytesPerInt32;
                var primaryKey = SerializationUtilities.ReadInt32FromByteArray(entryData, index);
                index += kBytesPerInt32;
                var resourceType = SerializationUtilities.ReadInt32FromByteArray(entryData, index);
                object data = dataIndex < 0 ? null : SerializationUtilities.ReadObjectFromByteArray(extraData, dataIndex);
                var internalIDString = ResolveInternalId(ExpandInternalId(m_InternalIdPrefixes, m_InternalIds[internalId]));
                locations[i] = new CompactLocation(locator, internalIDString,
                    m_ProviderIds[providerIndex], dependencyKeyIndex < 0 ? null : keys[dependencyKeyIndex], data, depHash, keys[primaryKey].ToString(), null);
            }

            for (int i = 0; i < buckets.Length; i++)
            {
                var bucket = buckets[i];
                var key = keys[i];
                var locs = new IResourceLocation[bucket.entries.Length];
                for (int b = 0; b < bucket.entries.Length; b++)
                    locs[b] = locations[bucket.entries[b]];
                locator.Add(key, locs);
            }

            return locator;

        }

        /// <summary>
        /// Used to resolve a string using addressables config values
        /// </summary>
        /// <param name="id">The internal id to resolve.</param>
        /// <returns>Returns the string that the internal id represents.</returns>
        public string ResolveInternalId(string id)
        {
            var path = EvaluateString(id);
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_XBOXONE
            if (path.Length >= 260 && path.StartsWith(Application.dataPath))
                path = path.Substring(Application.dataPath.Length + 1);
#endif
            return path;
        }

        /// <summary>
        /// Evaluates all tokens deliminated by '{' and '}' in a string and evaluates them with the EvaluateProperty method.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The evaluated string after resolving all tokens.</returns>
        public static string EvaluateString(string input)
        {
            return EvaluateString(input, '{', '}', EvaluateProperty);
        }


        internal static string ExpandInternalId(string[] internalIdPrefixes, string v)
        {
            if (internalIdPrefixes == null || internalIdPrefixes.Length == 0)
                return v;
            int nextHash = v.LastIndexOf('#');
            if (nextHash < 0)
                return v;
            int index = 0;
            var numStr = v.Substring(0, nextHash);
            if (!int.TryParse(numStr, out index))
                return v;
            return internalIdPrefixes[index] + v.Substring(nextHash + 1);
        }

        // cache these to avoid GC allocations
        static Stack<string> s_TokenStack = new Stack<string>(32);
        static Stack<int> s_TokenStartStack = new Stack<int>(32);
        static bool s_StaticStacksAreInUse = false;


        /// <summary>
        /// Evaluates all tokens deliminated by the specified delimiters in a string and evaluates them with the supplied method.
        /// </summary>
        /// <param name="inputString">The string to evaluate.</param>
        /// <param name="startDelimiter">The start token delimiter.</param>
        /// <param name="endDelimiter">The end token delimiter.</param>
        /// <param name="varFunc">Func that has a single string parameter and returns a string.</param>
        /// <returns>The evaluated string.</returns>
        public static string EvaluateString(string inputString, char startDelimiter, char endDelimiter, Func<string, string> varFunc)
        {
            if (string.IsNullOrEmpty(inputString))
                return string.Empty;

            string originalString = inputString;

            Stack<string> tokenStack;
            Stack<int> tokenStartStack;

            if (!s_StaticStacksAreInUse)
            {
                tokenStack = s_TokenStack;
                tokenStartStack = s_TokenStartStack;
                s_StaticStacksAreInUse = true;
            }
            else
            {
                tokenStack = new Stack<string>(32);
                tokenStartStack = new Stack<int>(32);
            }

            tokenStack.Push(inputString);
            int popTokenAt = inputString.Length;
            char[] delimiters = { startDelimiter, endDelimiter };
            bool delimitersMatch = startDelimiter == endDelimiter;

            int i = inputString.IndexOf(startDelimiter);
            int prevIndex = -2;
            while (i >= 0)
            {
                char c = inputString[i];
                if (c == startDelimiter && (!delimitersMatch || tokenStartStack.Count == 0))
                {
                    tokenStartStack.Push(i);
                    i++;
                }
                else if (c == endDelimiter && tokenStartStack.Count > 0)
                {
                    int start = tokenStartStack.Peek();
                    string token = inputString.Substring(start + 1, i - start - 1);
                    string tokenVal;

                    if (popTokenAt <= i)
                    {
                        tokenStack.Pop();
                    }

                    // check if the token is already included
                    if (tokenStack.Contains(token))
                        tokenVal = "#ERROR-CyclicToken#";
                    else
                    {
                        tokenVal = varFunc == null ? string.Empty : varFunc(token);
                        tokenStack.Push(token);
                    }

                    i = tokenStartStack.Pop();
                    popTokenAt = i + tokenVal.Length + 1;

                    if (i > 0)
                    {
                        int rhsStartIndex = i + token.Length + 2;
                        if (rhsStartIndex == inputString.Length)
                            inputString = inputString.Substring(0, i) + tokenVal;
                        else
                            inputString = inputString.Substring(0, i) + tokenVal + inputString.Substring(rhsStartIndex);
                    }
                    else
                        inputString = tokenVal + inputString.Substring(i + token.Length + 2);
                }

                bool infiniteLoopDetected = prevIndex == i;
                if (infiniteLoopDetected)
                    return "#ERROR-" + originalString + " contains unmatched delimiters#";

                prevIndex = i;
                i = inputString.IndexOfAny(delimiters, i);
            }

            tokenStack.Clear();
            tokenStartStack.Clear();
            if (ReferenceEquals(tokenStack, s_TokenStack))
                s_StaticStacksAreInUse = false;
            return inputString;
        }

        static Assembly[] GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }


        static Dictionary<string, string> s_CachedValues = new Dictionary<string, string>();

        /// <summary>
        /// Evaluates a named property using cached values and static public fields and properties.  Be aware that a field or property may be stripped if not referenced anywhere else.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>The value of the property.  If not found, the name is returned.</returns>
        public static string EvaluateProperty(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            string cachedValue;
            if (s_CachedValues.TryGetValue(name, out cachedValue))
                return cachedValue;

            int i = name.LastIndexOf('.');
            if (i < 0)
                return name;

            var className = name.Substring(0, i);
            var propName = name.Substring(i + 1);
            foreach (var a in GetAssemblies())
            {
                Type t = a.GetType(className, false, false);
                if (t == null)
                    continue;
                try
                {
                    var pi = t.GetProperty(propName, BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
                    if (pi != null)
                    {
                        var v = pi.GetValue(null, null);
                        if (v != null)
                        {
                            s_CachedValues.Add(name, v.ToString());
                            return v.ToString();
                        }
                    }
                    var fi = t.GetField(propName, BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
                    if (fi != null)
                    {
                        var v = fi.GetValue(null);
                        if (v != null)
                        {
                            s_CachedValues.Add(name, v.ToString());
                            return v.ToString();
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return name;
        }

        /// <summary>
        /// Create a new ContentCatalogData object without any data.
        /// </summary>
        public ContentCatalogData() {}
    }
}
