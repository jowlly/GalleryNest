namespace GalleryNestServer.Entities
{
    public class Photo : IdentifiableEntity
    {
        public int AlbumId { get; set; }
        public string Path { get; set; } = string.Empty;
        public bool IsFavourite { get; set; } = false;
        public DateTime CreationTime { get; set; }
    }
}
