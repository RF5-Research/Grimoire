using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Grimoire.Serialization.Attributes;

namespace Grimoire.Models.UnityEngine
{
    [Serializable(Target.Property)]
    public class Sprite
    {
        public Rectf m_Rect { get; set; }
        public SpriteAtlas m_SpriteAtlas { get; set; }
        public SpriteRenderData m_RD { get; set; }
    }
}
