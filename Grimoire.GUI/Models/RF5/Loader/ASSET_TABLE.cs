/*
 * Generated code file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
 */

using System.ComponentModel;
using static Grimoire.Core.Serialization.Attributes;

namespace Grimoire.GUI.Models.RF5.Loader
{
    [Serializable(Target.Property)]
    public class ASSET_TABLE : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public bool Region { get; set; }
        public string Label { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
