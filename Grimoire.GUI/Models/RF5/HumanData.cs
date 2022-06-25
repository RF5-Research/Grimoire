/*
 * Generated code file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
 */

using Grimoire.GUI.Models.RF5.Define;
using Grimoire.GUI.Models.RF5.Loader.ID;
using System;
using System.ComponentModel;

namespace Grimoire.GUI.Models.RF5
{
    [Serializable]
    public class HumanData : INotifyPropertyChanged
    {
        public VariationNo Variation;
        public HairType Hair;
        public string AssetName;
        public string PrefabName;
        public Character DataId;

        public VariationNo VariationProperty { get => Variation; set => Variation = value; }
        public HairType HairProperty { get => Hair; set => Hair = value; }
        public string AssetNameProperty { get => AssetName; set => AssetName = value; }
        public string PrefabNameProperty { get => PrefabName; set => PrefabName = value; }
        public Character DataIdProperty { get => DataId; set => DataId = value; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}