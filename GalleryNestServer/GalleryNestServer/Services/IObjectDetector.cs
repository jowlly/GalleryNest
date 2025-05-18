namespace GalleryNestServer.Services
{
    public interface IObjectDetector
    {
        Task<Dictionary<string, float>> DetectCategoriesAsync(Stream imageStream);
        Task<Dictionary<string, float>> ClassifyCategoriesAsync(Stream imageStream);
    }
}
