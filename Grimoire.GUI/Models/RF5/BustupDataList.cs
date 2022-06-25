/*
 * Generated code file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Grimoire.GUI.Models.RF5
{
    public class BustupDataList : INotifyPropertyChanged
    {
        public DataTables[] Datas;
        public MouthTable[] DefaultMouthTables;

        public DataTables[] DatasProperty { get => Datas; set => Datas = value; }
        public MouthTable[] DefaultMouthTablesProperty { get => DefaultMouthTables; set => DefaultMouthTables = value; }

        public event PropertyChangedEventHandler? PropertyChanged;

        [Serializable]
        public class DataTable : INotifyPropertyChanged
        {
            public int Val1;
            public int Val2;
            public int LoadID;
            public int PoseNo;

            public int PoseNoProperty { get => PoseNo; set => PoseNo = value; }
            public int LoadIDProperty { get => LoadID; set => LoadID = value; }
            public int Val1Property { get => Val1; set => Val1 = value; }
            public int Val2Property { get => Val2; set => Val2 = value; }

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        [Serializable]
        public class DataTables : INotifyPropertyChanged
        {
            public List<DataTable> Data;

            public List<DataTable> DataProperty { get => Data; set => Data = value; }

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        [Serializable]
        public class MouthTable : INotifyPropertyChanged
        {
            public int[] BaseFaces;
            public int Spose1;
            public int Spose2;

            public int[] BaseFacesProperty { get => BaseFaces; set => BaseFaces = value; }
            public int Spose1Property { get => Spose1; set => Spose1 = value; }
            public int Spose2Property { get => Spose2; set => Spose2 = value; }

            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }

}