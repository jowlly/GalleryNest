using GalleryNestServer.Entities;
using LiteDB;

namespace GalleryNestServer.Data
{
    public class EntityRepository<T> : IEntityRepository<T> where T : IdentifiableEntity
    {
        protected readonly LiteDatabase _database;
        protected readonly ILiteCollection<T> _collection;

        public EntityRepository(LiteDatabase database, string collectionName)
        {
            _database = database;
            _collection = _database.GetCollection<T>(collectionName);
            _collection.EnsureIndex(x => x.Id);
            _collection.EnsureIndex(x => x.CreatedAt);
        }

        public void Delete(IEnumerable<int> ids)
        {

            _collection.DeleteMany(entity => ids.Contains(entity.Id) && entity.Id != 1);
        }

        public IEnumerable<T> GetAll()
        {
            return _collection.FindAll();
        }

        public T GetById(int id)
        {
            try
            {
                return _collection.FindOne(entity => entity.Id == id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public void Set(IEnumerable<T> photos)
        {
            _collection.Upsert(photos.Select(x => { x.CreatedAt = DateTime.Now; return x; }).ToList());
        }


    }
}
