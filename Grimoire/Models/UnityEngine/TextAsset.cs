using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System.Text;
using static Grimoire.Serialization.Attributes;

namespace Grimoire.Models.UnityEngine
{
    [Serializable(Target.Property)]
    public class TextAsset : ISerialization
    {
        public string m_Name { get; set; }
        public byte[] m_Script { get; set; }

        public void Deserialize(AssetsManager am, System.Type type, AssetTypeValueField assetTypeValueField, AssetsFileInstance fileInstance = null)
        {
            m_Name = assetTypeValueField.Get("m_Name").GetValue().AsString();
            m_Script = assetTypeValueField.Get("m_Script").GetValue().AsStringBytes();
        }

        public void Serialize(AssetsManager am, AssetTypeValueField assetTypeValueField, AssetsFileInstance fileInstance = null)
        {
            assetTypeValueField.Get("m_Name").GetValue().Set(m_Name);
            assetTypeValueField.Get("m_Script").GetValue().Set(m_Script);
        }
    }
}
