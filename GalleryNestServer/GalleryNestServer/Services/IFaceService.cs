using System.Drawing;

namespace GalleryNestServer.Services
{
    public interface IFaceService
    {
        float[]? GetFaceEmbedding(Image image);
    }
}
