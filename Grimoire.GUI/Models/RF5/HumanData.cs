﻿/*
 * Generated code file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
 */

using Grimoire.GUI.Models.RF5.Define;
using Grimoire.GUI.Models.RF5.Loader.ID;
using System.ComponentModel;
using static Grimoire.Core.Serialization.Attributes;

namespace Grimoire.GUI.Models.RF5
{
    [Serializable(Target.Property)]
    public class HumanData : INotifyPropertyChanged
    {
        public VariationNo Variation { get; set; }
        public HairType Hair { get; set; }
        public string AssetName { get; set; }
        public string PrefabName { get; set; }
        public Character DataId { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}