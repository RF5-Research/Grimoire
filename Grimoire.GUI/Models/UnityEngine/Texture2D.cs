namespace Grimoire.GUI.Models.UnityEngine
{
    public class Texture2D
    {
        public string m_Name;
        public int m_ForcedFallbackFormat;
        public bool m_DownscaleFallback;
        public int m_Width;
        public int m_Height;
        public int m_CompleteImageSize;
        public int m_TextureFormat;
        public int m_MipCount;
        public bool m_MipMap;
        public bool m_IsReadable;
        public bool m_ReadAllowed;
        public bool m_StreamingMipmaps;
        public int m_StreamingMipmapsPriority;
        public int m_ImageCount;
        public int m_TextureDimension;
        public GLTextureSettings m_TextureSettings;
        public int m_LightmapFormat;
        public int m_ColorSpace;
        //public byte[] pictureData;
        public StreamingInfo m_StreamData;

        public struct GLTextureSettings
        {
            public int m_FilterMode;
            public int m_Aniso;
            public float m_MipBias;
            public int m_WrapMode;
            public int m_WrapU;
            public int m_WrapV;
            public int m_WrapW;
        }

        public struct StreamingInfo
        {
            public uint offset;
            public uint size;
            public string path;
        }
    }
}
