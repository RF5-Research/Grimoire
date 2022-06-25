/*
 * Generated code file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
 */

using Grimoire.GUI.Models.RF5.Define;
using Grimoire.GUI.Models.RF5.Loader.ID;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Grimoire.GUI.Models.RF5
{
    public class HumanDataArray : INotifyPropertyChanged
    {
        public CharID CharId;
        public List<HumanData> HumanDatas;
        public Character[] BaseId;

        public CharID CharIdProperty { get => CharId; set => CharId = value; }
        public List<HumanData> HumanDatasProperty { get => HumanDatas; set => HumanDatas = value; }
        public Character[] BaseIdProperty { get => BaseId; set => BaseId = value; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}