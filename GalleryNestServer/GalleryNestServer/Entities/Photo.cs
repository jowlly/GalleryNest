namespace GalleryNestServer.Entities
{
    public class Photo : IdentifiableEntity
    {
        public string Guid { get; set; }
        public List<int> AlbumIds { get; set; }
        public List<int> SelectionIds { get; set; }
        public List<string> PersonIds { get; set; }
        public string Path { get; set; } = string.Empty;
        public bool IsFavourite { get; set; } = false;
        public DateTime CreationTime { get; set; }
    }
}
