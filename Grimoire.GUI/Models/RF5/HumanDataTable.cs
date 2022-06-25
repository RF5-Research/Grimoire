/*
 * Generated code file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
 */

using Grimoire.GUI.Models.RF5;
using System;
using System.Collections.Generic;
using System.ComponentModel;

[Serializable]
public class HumanDataTable : INotifyPropertyChanged
{
    public List<HumanDataArray> HumanDatas;

    public List<HumanDataArray> HumanDatasProperty { get => HumanDatas; set => HumanDatas = value; }

    public event PropertyChangedEventHandler? PropertyChanged;
}

