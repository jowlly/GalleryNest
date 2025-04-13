using GalleryNestServer.Entities;
using LiteDB;

namespace GalleryNestServer.Data
{
    public class EntityRepository<T> : IEntityRepository<T> where T : IdentifiableEntity
    {
        private readonly LiteDatabase _database;
        private readonly ILiteCollection<T> _collection;

        public EntityRepository(LiteDatabase database, string collectionName)
        {
            _database = database;
            _collection = _database.GetCollection<T>(collectionName);
        }

        public void Delete(IEnumerable<int> ids)
        {
            _collection.DeleteMany(entity => ids.Contains(entity.Id));
        }

        public IEnumerable<T> GetAll()
        {
            return _collection.FindAll();
        }

        public T GetById(int id)
        {
            return _collection.FindOne(entity => entity.Id == id);
        }

        public void Set(IEnumerable<T> photos)
        {
            _collection.Upsert(photos);
        }
    }
}
