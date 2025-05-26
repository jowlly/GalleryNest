using LiteDB;

namespace GalleryNestServer.Entities
{
    public class IdentifiableEntity : Entity
    {
        [BsonId]
        public int Id { get; set; }
    }
}