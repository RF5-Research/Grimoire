using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grimoire.GUI.Core
{
    public interface ISerialization
    {
        public void Deserialize(AssetsManager am, Type type, AssetTypeValueField assetTypeValueField, AssetsFileInstance? fileInstance = null);
    }
}
