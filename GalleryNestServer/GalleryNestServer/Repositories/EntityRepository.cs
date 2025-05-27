using GalleryNestServer.Entities;
using LiteDB;

namespace GalleryNestServer.Data
{
    public class EntityRepository<T> : IEntityRepository<T> where T : IdentifiableEntity
    {
        protected readonly LiteDatabase _database;
        protected readonly ILiteCollection<T> _collection;
        private static readonly object _dbLock = new object();

        public EntityRepository(LiteDatabase database, string collectionName)
        {
            _database = database;
            _collection = _database.GetCollection<T>(collectionName);
            _collection.EnsureIndex(x => x.Id);
            _collection.EnsureIndex(x => x.CreatedAt);
        }
        public IEnumerable<T> GetPaged(int pageNumber, int pageSize)
        {
            return _collection.FindAll()
                              .OrderByDescending(x => x.CreatedAt)
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize);
        }

        public void Delete(IEnumerable<int> ids)
        {

            lock (_dbLock)
            {
                _collection.DeleteMany(entity => ids.Contains(entity.Id) && entity.Id != 1);
            }
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

        public IEnumerable<T> GetByIds(IEnumerable<int> ids)
        {
            try
            {
                return _collection.Find(entity => ids.Contains(entity.Id));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public void Set(IEnumerable<T> photos)
        {
            lock (_dbLock)
            {
                _collection.Upsert(photos.Select(x => { x.CreatedAt = DateTime.Now; return x; }).ToList());
            }
        }


    }
}
