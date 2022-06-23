using Grimoire.GUI.Core.Services;
using Grimoire.GUI.Models.RF5;

namespace Grimoire.GUI.ViewModels
{
    internal class CharactersWindowViewModel : ViewModelBase
    {
        private BustupDataList BustupDataTable { get; set; }
        private HumanDataTable HumanDataTable { get; set; }
        public CharactersWindowViewModel()
        {
        }
    }
}
