using GalleryNestServer.Data;
using GalleryNestServer.Entities;
using LiteDB;

namespace GalleryNestServer.Repositories
{
    public class PhotoRepository : EntityRepository<Photo>
    {
        public PhotoRepository(LiteDatabase database, string collectionName) : base(database, collectionName)
        {
            _collection.EnsureIndex(x => x.AlbumIds);
            _collection.EnsureIndex(x => x.SelectionIds);
            _collection.EnsureIndex(x => x.PersonIds);
            _collection.EnsureIndex(x => x.Guid);
        }
        public new List<Photo> GetPaged(int pageNumber, int pageSize)
        {
            return _collection.FindAll()
                              .OrderByDescending(x => x.CreationTime)
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize).ToList();
        }

        public Photo? GetLatestByAlbumId(int albumId)
        {
            return _collection.Find(entity => entity.AlbumIds.Contains(albumId))
                               .OrderByDescending(x => x.CreatedAt)
                               .FirstOrDefault();
        }
        public Photo GetByGuid(string guid)
        {
            return _collection.FindOne(x => x.Guid == guid);
        }

        public IEnumerable<Photo> GetByAlbumId(int selectionId, int pageNumber, int pageSize)
        {
            return _collection.Find(entity => entity.AlbumIds.Contains(selectionId))
                              .OrderByDescending(x => x.CreatedAt)
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize);
        }

        public Photo? GetLatestBySelectionId(int selectionId)
        {
            return _collection.Find(entity => entity.SelectionIds.Contains(selectionId))
                               .OrderByDescending(x => x.CreatedAt)
                               .FirstOrDefault();
        }

        public IEnumerable<Photo> GetBySelectionId(int selectionId, int pageNumber, int pageSize)
        {
            return _collection.Find(entity => entity.SelectionIds.Contains(selectionId))
                              .OrderByDescending(x => x.CreatedAt)
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize);
        }

        public IEnumerable<Photo> GetFavourite(int pageNumber, int pageSize)
        {
            return _collection.Find(entity => entity.IsFavourite)
                              .OrderByDescending(x => x.CreatedAt)
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize);
        }

        public IEnumerable<Photo> GetRecent(int pageNumber, int pageSize)
        {
            return _collection.FindAll()
                              .OrderByDescending(x => x.CreatedAt)
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize);
        }

        public IEnumerable<Photo> GetByPersonGuid(string personId, int pageNumber, int pageSize)
        {
            return _collection.Find(entity => entity.PersonIds.Contains(personId))
                              .OrderByDescending(x => x.CreatedAt)
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize);
        }

        public Photo? GetLatestByPersonGuid(string personId)
        {
            var all = _collection.FindAll();
            return _collection.Find(
                entity => entity.PersonIds.Contains(personId))
                               .OrderByDescending(x => x.CreatedAt)
                               .FirstOrDefault();
        }

        internal object GetCount()
        {
            return _collection.Count();
        }
    }
}
