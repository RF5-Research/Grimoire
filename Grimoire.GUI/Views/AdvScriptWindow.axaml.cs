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
            var assetIndex = AddressablesService.LocateKey($"Eng/{(int)Master.ADVINDEXDATA}");
            var bundlePath = AddressablesService.GetAssetBundlePath(assetIndex);

            var am = new AssetsManager();
            var bundle = am.LoadBundleFile(bundlePath);
            var inst = am.LoadAssetsFileFromBundle(bundle, 0, true);
            var inf = inst.table.GetAssetInfo(typeof(AdvIndexData).Name, (int)AssetClassID.MonoBehaviour, caseSensitive: false);
            var baseField = am.GetTypeInstance(inst, inf).GetBaseField();
            var x = baseField.GetTemplateField();
            //var advIndexData = Core.Serialization.Deserialize<AdvIndexData>(baseField);
            //Core.Serialization.Serialize(advIndexData, baseField);
            //var test = Core.Serialization.Deserialize<AdvIndexData>(baseField);
        }
    }
}
