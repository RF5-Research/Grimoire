using Avalonia.Controls;
using Grimoire.Unity.Addressables.ResourceLocators;
using PropertyChanged;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Grimoire.UI.Views
{
    [DoNotNotify]
    public partial class AdvScriptWindow : Window
    {

        public AdvScriptWindow()
        {
            InitializeComponent();
            using (var fs = new FileStream(@"C:\Users\Sloth\Downloads\Switch\Dumps\RF5\romfs\Data\StreamingAssets\aa\catalog.json", FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(fs))
            {
                var file = JsonSerializer.Deserialize<ContentCatalogData>(reader.ReadToEnd());
                var test = file.CreateLocator();

                //foreach (var t in test.Locations)
                //{
                //    foreach (var f in t.Value)
                //    {
                //        if (f.InternalId == "Assets/AddressableAssets/All/Prefab/Npc/NpcDatas.prefab")
                //        {
                //            var g = f.HasDependencies;
                //        }
                //    }
                //}
                System.Collections.Generic.IList<Unity.Addressables.ResourceManager.ResourceLocations.IResourceLocation> locs;
                test.Locate("All/15507", typeof(object), out locs);
                foreach (var location in locs)
                {
                    var d = location.ToString();
                }
            }
        }
    }
}
