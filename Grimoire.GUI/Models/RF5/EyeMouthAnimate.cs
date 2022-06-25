/*
 * Generated code file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
 */

using Grimoire.GUI.Models.UnityEngine;
using static Grimoire.Core.Serialization.Attributes;

namespace Grimoire.GUI.Models.RF5
{
    public class EyeMouthAnimate
	{
		private bool PlayEye;
		private bool PlayEyeBrows;
		private bool MouthSeted;
		private bool _PlayMouth;
		public Image MainImage;
		public Image NoFaceImage;
		public Image EyeImage;
		public Image EyeBrowsImage;
		public Image MouthImage;
		public Sprite[] BaseImage;
		[SerializeField]
		private Sprite[] EyeImgs;
		[SerializeField]
		private Sprite[] EyeBrowsImgs;
		[SerializeField]
		private Sprite[] MouthImgs;
		[SerializeField]
		private int MaxEyeId;
		[SerializeField]
		private int MaxEyeBrowsId;
		[SerializeField]
		private int MaxMouthId;
		[SerializeField]
		private EMAnimSet[] EyeSet;
		[SerializeField]
		private EMAnimSet[] EyeBrowsSet;
		[SerializeField]
		private EMAnimSet[] MouthSet;
		[SerializeField]
		private float EyeFrame;
		[SerializeField]
		private float EyeBrowsFrame;
		[SerializeField]
		private float MouthFrame;
		[SerializeField]
		private int EyeAnimPoint;
		[SerializeField]
		private int EyeBrowsAnimPoint;
		[SerializeField]
		private int MouthAnimPoint;
		[SerializeField]
		private float EyeWaitTimeMin;
		[SerializeField]
		private float EyeWaitTimeMax;
		private Image ParentImage;
		public SpriteAtlas spriteAtlas;
		private int EyeVariation;
		private int EyeBrowsVariation;
		private int MouthVariation;
		private BustupDataList.MouthTable mouthTable;
		private int defaultMouthVal;
		private bool fadeInCompleted;

		public struct EMAnimSet
		{

			public float frame;
			public int id;
		}
	}
}
