﻿using Grimoire.GUI.Core.Services;
using Grimoire.GUI.Models;
using Grimoire.Unity.Addressables.ResourceManager.ResourceLocations;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections;
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

        private string[] Filters =>  new string[]
        {
            "Key",
            "Name"
        };

        [Reactive] private int FilterSelectedIndex { get; set; } = 0;
        [Reactive] private string SearchText { get; set; }

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
                        var hashCode = location.Location.InternalId.GetHashCode();
                        if (location.Location.HasDependencies && !addedKeys.Contains(hashCode))
                        {
                            addedKeys.Add(hashCode);
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
                    Assets.Where(x => x.Location.PrimaryKey.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) :
                    Assets.Where(x => x.Location.InternalId.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                Items = items.ToList();
            }
            else
            {
                Items = Assets;
            }
        }
    }
}
