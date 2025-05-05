namespace GalleryNestServer.Services
{
    public interface IObjectDetector
    {
        Task<Dictionary<string, float>> DetectCategoriesAsync(Stream imageStream);
    }
}
