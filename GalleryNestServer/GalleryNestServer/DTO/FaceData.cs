using System.Drawing;

namespace GalleryNestServer.DTO
{
    public class FaceData
    {
        public float[] Embedding { get; set; }
        public Rectangle BoundingBox { get; set; }
    }
}
