using System.Drawing;

namespace GalleryNestServer.Services
{
    public interface IFaceService
    {
        List<float[]> GetFaceEmbeddings(Image image);
    }
}
