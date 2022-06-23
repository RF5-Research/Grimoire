/*
 * Generated code file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
 */

using Grimoire.GUI.Models.RF5.Define;
using System.Collections.Generic;
using System.ComponentModel;

namespace Grimoire.GUI.Models.RF5
{
    public class HumanDataArray : INotifyPropertyChanged
	{
		public CharID CharId { get; set; }
		public List<HumanData> HumanDatas { get; set; }
		public Loader.ID.Character[] BaseId { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}