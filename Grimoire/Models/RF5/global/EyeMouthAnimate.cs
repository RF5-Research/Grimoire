/*
 * Generated code file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
 */

using Grimoire.Models.UnityEngine;
using static Grimoire.Serialization.Attributes;

namespace Grimoire.Models.RF5
{
	[Serializable(Target.Property)]
	public class EyeMouthAnimate
	{
		private bool PlayEye { get; set; }
		private bool PlayEyeBrows { get; set; }
		private bool MouthSeted { get; set; }
		private bool _PlayMouth { get; set; }
		public Image MainImage { get; set; }
		public Image NoFaceImage { get; set; }
		public Image EyeImage { get; set; }
		public Image EyeBrowsImage { get; set; }
		public Image MouthImage { get; set; }
		public Sprite[] BaseImage { get; set; }
		[SerializeField]
		private Sprite[] EyeImgs { get; set; }
		[SerializeField]
		private Sprite[] EyeBrowsImgs { get; set; }
		[SerializeField]
		private Sprite[] MouthImgs { get; set; }
		[SerializeField]
		private int MaxEyeId { get; set; }
		[SerializeField]
		private int MaxEyeBrowsId { get; set; }
		[SerializeField]
		private int MaxMouthId { get; set; }
		[SerializeField]
		private EMAnimSet[] EyeSet { get; set; }
		[SerializeField]
		private EMAnimSet[] EyeBrowsSet { get; set; }
		[SerializeField]
		private EMAnimSet[] MouthSet { get; set; }
		[SerializeField]
		private float EyeFrame { get; set; }
		[SerializeField]
		private float EyeBrowsFrame { get; set; }
		[SerializeField]
		private float MouthFrame { get; set; }
		[SerializeField]
		private int EyeAnimPoint { get; set; }
		[SerializeField]
		private int EyeBrowsAnimPoint { get; set; }
		[SerializeField]
		private int MouthAnimPoint { get; set; }
		[SerializeField]
		private float EyeWaitTimeMin { get; set; }
		[SerializeField]
		private float EyeWaitTimeMax { get; set; }
		private Image ParentImage { get; set; }
		public SpriteAtlas spriteAtlas { get; set; }
		private int EyeVariation { get; set; }
		private int EyeBrowsVariation { get; set; }
		private int MouthVariation { get; set; }
		private BustupDataList.MouthTable mouthTable { get; set; }
		private int defaultMouthVal { get; set; }
		private bool fadeInCompleted { get; set; }

		[Serializable(Target.Property)]
		public struct EMAnimSet
		{
			public float frame { get; set; }
			public int id { get; set; }
		}
	}
}
