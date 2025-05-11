
namespace GalleryNestApp.Model
{
    public class Photo
    {
        public int Id { get; set; }
        public int AlbumId { get; set; }
        public string Path { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
    }
}