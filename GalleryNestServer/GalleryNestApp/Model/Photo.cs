
namespace GalleryNestApp.Model
{
    public class Photo
    {
        public int Id { get; set; }
        public List<int> AlbumIds { get; set; }
        public List<int> SelectionIds { get; set; }
        public List<string> PersonIds { get; set; }
        public bool IsFavourite { get; set; }
        public string Path { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
    }
}