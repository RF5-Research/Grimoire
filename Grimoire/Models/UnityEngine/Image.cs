using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grimoire.Models.UnityEngine
{
    public class Image
    {
        //public GameObject m_GameObject;
        public byte m_Enabled;
        //public MonoScript m_Script;
        public string m_Name;
        //public Material m_Material;
        //public ColorRGBA m_Color;
        //public byte m_RaycastTarget;
        //public byte m_Maskable;
        //public CullStateChangedEvent CullStateChangedEvent;
        public Sprite m_Sprite;
        public int m_Type;
        public byte m_PreserveAspect;
        public byte m_FillCenter;
        public int m_FillMethod;
        public float m_FillAmount;
        public byte m_FillClockwise;
        public int m_FillOrigin;
        public byte m_UseSpriteMesh;
        public float m_PixelsPerUnitMultiplier;
    }
}
