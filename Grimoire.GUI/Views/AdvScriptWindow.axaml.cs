using AssetsTools.NET.Extra;
using Avalonia.Controls;
using Grimoire.GUI.Models;
using Grimoire.RF5.Loader.ID;
using Grimoire.Unity.Addressables.ResourceLocators;
using PropertyChanged;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Grimoire.GUI.Views
{
    public partial class AdvScriptWindow : Window
    {
        public AdvScriptWindow()
        {
            InitializeComponent();
            var assetIndex = AddressablesManager.LocateKey($"Eng/{((int)Master.ADVINDEXDATA)}");
            var am = new AssetsManager();
            //Hardcoding for now
            var bundlePath = assetIndex.Dependencies[0].InternalId/*.Replace("", ProjectManager.AddressableAssetsPath)*/;
            var bun = am.LoadBundleFile(bundlePath);

            var inst = am.LoadAssetsFileFromBundle(bun, 0, true);
            var inf = inst.table.GetAssetInfo(Master.ADVINDEXDATA.ToString(), 114, caseSensitive: false);

            var baseField = am.GetTypeInstance(inst, inf).GetBaseField();
            var offsets = baseField.Get("offset").Get("Array").GetChildrenList().Select(x => x.GetValue().AsInt()).ToList();
        }
    }
}
