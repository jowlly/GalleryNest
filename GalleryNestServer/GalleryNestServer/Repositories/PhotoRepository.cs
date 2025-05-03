using GalleryNestServer.Data;
using GalleryNestServer.Entities;
using LiteDB;

namespace GalleryNestServer.Repositories
{
    public class PhotoRepository : EntityRepository<Photo>
    {
        public PhotoRepository(LiteDatabase database, string collectionName) : base(database, collectionName)
        {
            _collection.EnsureIndex(x => x.AlbumId);
        }
        public Photo? GetLatestByAlbumId(int albumId)
        {
            return _collection.Find(entity => entity.AlbumId == albumId)
                               .OrderByDescending(x => x.CreatedAt)
                               .FirstOrDefault();
        }

        public IEnumerable<Photo> GetByAlbumId(int albumId, int pageNumber, int pageSize)
        {
            return _collection.Find(entity => entity.AlbumId == albumId)
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
    }
}
