/*
 * Generated code file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
 */

using Grimoire.GUI.Models.RF5.Define;
using System.ComponentModel;

namespace Grimoire.GUI.Models.RF5
{
	public class HumanData : INotifyPropertyChanged
	{
		public VariationNo Variation { get; set; }
		public HairType Hair { get; set; }
		public string AssetName { get; set; }
		public string PrefabName { get; set; }
		public Loader.ID.Character DataId { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}