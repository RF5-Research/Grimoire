﻿/*
 * Generated code file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
 */

using Grimoire.GUI.Models.RF5;
using System.Collections.Generic;
using System.ComponentModel;
using static Grimoire.Core.Serialization.Attributes;

[Serializable(Target.Property)]
public class HumanDataTable : INotifyPropertyChanged
{
    public List<HumanDataArray> HumanDatas { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
}
