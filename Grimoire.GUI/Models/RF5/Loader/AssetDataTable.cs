/*
 * Generated code file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Grimoire.GUI.Models.RF5.Loader
{
	public class AssetDataTable : INotifyPropertyChanged
	{
		public ASSET_TABLE[] AssetTables { get; set; }
		public Dictionary<int, ASSET_TABLE> DataTables { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
