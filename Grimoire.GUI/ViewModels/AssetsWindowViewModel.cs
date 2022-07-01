using Grimoire.GUI.Core.Services;
using Grimoire.GUI.Models;
using Grimoire.Unity.Addressables.ResourceManager.ResourceLocations;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Grimoire.GUI.ViewModels
{
    public class AssetsWindowViewModel : ViewModelBase
    {
        private class Item
        {
            public IResourceLocation Location { get; set; }
            public string Text => $"[{Location.PrimaryKey}] {Location.InternalId}";
            public Item(IResourceLocation location)
            {
                Location = location;
            }
        }

        private Item? SelectedItem { get; set; }

        private List<Item> Assets { get; }
        private List<Item> Items { get; set; }

        private string[] Filters
        {
            get
            {
                return new string[]
                {
                    "Key",
                    "Name"
                };
            }
        }
        private int _filterSelectedIndex = 0;
        private int FilterSelectedIndex { get => _filterSelectedIndex; set => this.RaiseAndSetIfChanged(ref _filterSelectedIndex, value); }

        private string? _searchText;
        private string? SearchText { get => _searchText; set => this.RaiseAndSetIfChanged(ref _searchText, value); }

        public AssetsWindowViewModel()
        {
            this.WhenAnyValue(x => x.FilterSelectedIndex)
                .Throttle(TimeSpan.FromMilliseconds(400))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(Search!);

            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(400))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(Search!);

            Assets = new List<Item>();
            if (Addressables.ResourceLocationMap != null)
            {
                if (Addressables.ResourceLocationMap.Locations != null)
                {
                    var addedKeys = new List<int>();
                    var locations = new ObservableCollection<Item>(Addressables.ResourceLocationMap.Locations.Values
                        .ToList()
                        .Select(x => new Item(x.First())))
                        .ToList();

                    foreach (var location in locations)
                    {
                        //Remove dup keys that were added for some reason 
                        if (location.Location.HasDependencies && !addedKeys.Contains(location.Location.DependencyHashCode))
                        {
                            addedKeys.Add(location.Location.DependencyHashCode);
                            Assets.Add(location);
                        }
                    }
                    Items = new List<Item>(Assets);
                }
            }
        }
        private void Search(int _)
        {
            SearchAsset();
        }
        private void Search(string _)
        {
            SearchAsset();
        }

        private void SearchAsset()
        {
            if (!string.IsNullOrEmpty(SearchText))
            {
                var items = FilterSelectedIndex == 0 ?
                    Assets.Where(x => x.Location.PrimaryKey.Contains(SearchText)) :
                    Assets.Where(x => x.Location.InternalId.Contains(SearchText));
                Items = items.ToList();
            }
            else
            {
                Items = Assets;
            }
        }
    }
}
