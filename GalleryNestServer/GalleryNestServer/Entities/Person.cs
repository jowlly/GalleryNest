namespace GalleryNestServer.Entities
{
    public class Person : IdentifiableEntity
    {
        public string Guid { get; set; }
        public List<float[]> Embeddings { get; set; }
        public string? Name { get; set; }
    }
}
