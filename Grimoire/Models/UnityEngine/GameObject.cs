using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Grimoire;
using Grimoire;
using System;
using System.Collections.Generic;

namespace Grimoire.Models.UnityEngine
{
    public class GameObject : ISerialization
    {
        public List<AssetPPtr> m_Component;

        void ISerialization.Deserialize(AssetsManager am, Type type, AssetTypeValueField assetTypeValueField, AssetsFileInstance? fileInstance)
        {
            m_Component = new List<AssetPPtr>();
            var components = assetTypeValueField.Get("m_Component").Get("Array");
            foreach (var component in components.GetChildrenList())
            {
                var ptr = component.Get("component");
                m_Component.Add(new AssetPPtr(ptr.Get("m_FileID").GetValue().AsInt(), ptr.Get("m_PathID").GetValue().AsInt64()));
            }
        }

        public T? GetComponent<T>(AssetsManager am, AssetClassID classID)
        {
            var assetFile = am.files[0];
            foreach (var component in m_Component)
            {
                //This method returns a possible null
                var asset = am.GetExtAsset(assetFile, component.fileID, component.pathID);

                if (asset.info.curFileType == (uint)classID)
                {
                    if (classID == AssetClassID.MonoBehaviour)
                    {
                        var index = AssetHelper.GetScriptIndex(asset.file.file, asset.info);
                        var scriptInfo = assetFile.table.GetAssetInfo(assetFile.file.preloadTable.items[index].pathID);
                        var script = am.GetTypeInstance(assetFile.file, scriptInfo);
                        string name;
                        if (scriptInfo.ReadName(assetFile.file, out name))
                            if (name == typeof(T).Name)
                                return Serialization.DeserializeObject<T>(am, asset.instance.GetBaseField(), assetFile);
                    }
                    else
                    {
                        return Serialization.DeserializeObject<T>(am, asset.instance.GetBaseField(), assetFile);
                    }
                }
            }
            return default(T);
        }

        public void Serialize(AssetsManager am, AssetTypeValueField assetTypeValueField, AssetsFileInstance fileInstance = null)
        {
            throw new NotImplementedException();
        }
    }
}
