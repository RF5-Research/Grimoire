/*
 * Generated code file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
 */

using Grimoire.GUI.Models.RF5.Define;
using Grimoire.GUI.Models.RF5.Loader.ID;
using System.Collections.Generic;
using System.ComponentModel;
using static Grimoire.Core.Serialization.Attributes;

namespace Grimoire.GUI.Models.RF5
{
    [Serializable(Target.Property)]
    public class HumanDataArray : INotifyPropertyChanged
    {
        public CharID CharId { get; set; }
        public List<HumanData> HumanDatas { get; set; }
        public Character[] BaseId { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}