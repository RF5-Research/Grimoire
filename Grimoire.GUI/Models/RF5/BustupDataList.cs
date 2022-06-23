/*
 * Generated code file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
 */

using System.Collections.Generic;
using System.ComponentModel;

namespace Grimoire.GUI.Models.RF5
{
    public class BustupDataList : INotifyPropertyChanged
	{
		public DataTables[] Datas { get; set; }
		public MouthTable[] DefaultMouthTables { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public class DataTable : INotifyPropertyChanged
		{
			public int Val1 { get; set; }
			public int Val2 { get; set; }
			public int PoseNo { get; set; }
			public int LoadID { get; set; }

            public event PropertyChangedEventHandler? PropertyChanged;
        }

		public class DataTables : INotifyPropertyChanged
		{
			public List<DataTable> Data { get; set; }

            public event PropertyChangedEventHandler? PropertyChanged;
        }

		public class MouthTable : INotifyPropertyChanged
		{
			public int[] BaseFaces { get; set; }
			public int Spose1 { get; set; }
			public int Spose2 { get; set; }

            public event PropertyChangedEventHandler? PropertyChanged;
        }
	}

}