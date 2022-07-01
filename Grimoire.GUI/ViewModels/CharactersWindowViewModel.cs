using AssetsTools.NET.Extra;
using Avalonia.Media.Imaging;
using Grimoire.GUI.Core;
using Grimoire.GUI.Core.Services;
using Grimoire.GUI.Core.Texture;
using Grimoire.GUI.Models.RF5;
using Grimoire.GUI.Models.RF5.Define;
using Grimoire.GUI.Models.UnityEngine;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grimoire.GUI.ViewModels
{
    public class CharactersWindowViewModel : ViewModelBase
    {
        AdvBustupResManager? AdvBustupResManager;
        private CancellationTokenSource? cts;
        Bitmap? Image { get; set; }
        List<NPCID> List { get; set; }

        private int _selectedIndex;
        private int SelectedIndex { get => _selectedIndex; set => this.RaiseAndSetIfChanged(ref _selectedIndex, value); }

        public CharactersWindowViewModel()
        {
            List = Enum.GetValues(typeof(NPCID)).Cast<NPCID>().ToList();
            AdvBustupResManager = new();

            this.WhenAnyValue(x => x.SelectedIndex)
                .Throttle(TimeSpan.FromMilliseconds(400))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(LoadImage!);
        }

        public async void LoadImage(int _ = 0)
        {
            try
            {
                cts?.Cancel();
                cts = new CancellationTokenSource();

                var value = await GetImage();
                if (!cts.IsCancellationRequested)
                {
                    Image = value;
                }
            }
            //Not sure if this is necessary anymore
            catch (TaskCanceledException)
            {
                LoadImage();
            }
            // Caused by task cancel.
            catch (NullReferenceException)
            {
                LoadImage();
            }
            // Anything else
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                Image = null;
            }
        }

        private async Task<Bitmap?> GetImage()
        {
            var variationID = 100;
            var poseID = 0;
            var loaderID = AdvBustupResManager.GetLoaderID((NPCID)SelectedIndex, poseID / 10 % 10, variationID / 100);
            var go = await Loader.LoadIDAsync<GameObject>(loaderID);
            var am = Loader.AssetsManager;
            if (go != null)
            {
                var eyeMouthAnimate = go.GetComponent<EyeMouthAnimate>(Loader.AssetsManager, AssetClassID.MonoBehaviour);
                var path = Path.GetFileName(Path.GetDirectoryName(eyeMouthAnimate?.MainImage.m_Sprite.m_RD.texture.m_StreamData.path));
                var bundle = am.files.Find(x => x.name == path)?.parentBundle;
                var data = BundleHelper.LoadAssetDataFromBundle(bundle?.file, Path.GetFileName(eyeMouthAnimate?.MainImage.m_Sprite.m_RD.texture.m_StreamData.path));
                var texture = eyeMouthAnimate?.MainImage.m_Sprite.m_RD.texture;
                byte[] texDat = new byte[texture.m_StreamData.size];
                using (var ms = new MemoryStream(data))
                {
                    ms.Read(texDat, (int)texture.m_StreamData.offset, (int)texture.m_StreamData.size);
                }

                if (texDat != null && texDat.Length > 0)
                {
                    var decoded = Astc.DecodeASTC(texDat, texture.m_Width, texture.m_Height, 6, 6);
                    var bitmap = new System.Drawing.Bitmap(
                        texture.m_Width,
                        texture.m_Height,
                        texture.m_Width * 4,
                        PixelFormat.Format32bppArgb,
                        Marshal.UnsafeAddrOfPinnedArrayElement(decoded, 0));
                    bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

                    using MemoryStream memory = new MemoryStream();
                    bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    return new Bitmap(memory);
                }
            }
            return null;
        }
    }
}
