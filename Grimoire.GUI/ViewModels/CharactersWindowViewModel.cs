using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Grimoire.Core;
using Grimoire.GUI.Core.Services;
using Grimoire.GUI.Core.Texture;
using Grimoire.GUI.Models.RF5;
using Grimoire.GUI.Models.RF5.Define;
using Grimoire.GUI.Models.RF5.Loader.ID;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Grimoire.GUI.ViewModels
{
    internal class CharactersWindowViewModel : ViewModelBase
    {
        private Avalonia.Media.Imaging.Bitmap Bitmap { get; set; }
        private BustupDataList BustupDataTable { get; set; }
        private HumanDataTable HumanDataTable { get; set; }
        private List<NPCID> List { get; set; }

        int selectedIndex;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedIndex, value);
                LoadImage();
            }
        }
        public CharactersWindowViewModel()
        {
            List = Enum.GetValues(typeof(NPCID)).Cast<NPCID>().ToList();
            var am = new AssetsManager();
            LoaderService.LoadID(am, (int)Master.UIDATA_BUSTUPDATATABLE);
            (var bustupDataTableAsset, var assetFile) = LoaderService.FindSerializedAsset(am, nameof(BustupDataTable), AssetClassID.MonoBehaviour);
            BustupDataTable = LoaderService.GetDeserializedObject<BustupDataList>(am, bustupDataTableAsset, assetFile);
            if (BustupDataTable == null)
                throw new System.Exception($"Couldn't load `{nameof(BustupDataTable)}`");
            am.UnloadAll();

            LoadImage();
        }

        private void LoadImage()
        {
            var am = new AssetsManager();
            LoaderService.LoadID(am, BustupDataTable.Datas[SelectedIndex].Data[0].LoadID);
            (var eyeMouthAnimateAsset, var eyeMouthAnimateAssetFile) = LoaderService.FindSerializedAsset(am, "EyeMouthAnimate", AssetClassID.MonoBehaviour);
            if (eyeMouthAnimateAsset != null && eyeMouthAnimateAssetFile != null)
            {
                var eyeMouthAnimate = Serialization.DeserializeObject<EyeMouthAnimate>(am, eyeMouthAnimateAsset.GetBaseField(), eyeMouthAnimateAssetFile);
                var x = Path.GetDirectoryName(eyeMouthAnimate.MainImage.m_Sprite.m_RD.texture.m_StreamData.path);
                var path = Path.GetDirectoryName(eyeMouthAnimate.MainImage.m_Sprite.m_RD.texture.m_StreamData.path);
                path = Path.GetFileName(path);
                var bundle = am.files.Find(x => x.name == path).parentBundle;
                var data = BundleHelper.LoadAssetDataFromBundle(bundle.file, Path.GetFileName(eyeMouthAnimate.MainImage.m_Sprite.m_RD.texture.m_StreamData.path));
                var texture = eyeMouthAnimate.MainImage.m_Sprite.m_RD.texture;
                byte[] texDat = new byte[texture.m_StreamData.size];
                using (var ms = new MemoryStream(data))
                {
                    ms.Read(texDat, (int)texture.m_StreamData.offset, (int)texture.m_StreamData.size);
                }
                if (texDat != null && texDat.Length > 0)
                {
                    var decoded = Astc.DecodeASTC(texDat, texture.m_Width, texture.m_Height, 6, 6);
                    var canvas = new System.Drawing.Bitmap(texture.m_Width, texture.m_Height, texture.m_Width * 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb,
                        Marshal.UnsafeAddrOfPinnedArrayElement(decoded, 0));
                    canvas.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

                    using (MemoryStream memory = new MemoryStream())
                    {
                        canvas.Save(memory, ImageFormat.Png);
                        memory.Position = 0;

                        Bitmap = new Avalonia.Media.Imaging.Bitmap(memory);
                    }
                }
            }
        }
    }
}
