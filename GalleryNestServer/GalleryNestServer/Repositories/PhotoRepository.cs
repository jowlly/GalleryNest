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
        public IEnumerable<Photo> GetByAlbumId(int albumId)
        {
            return _collection.Find(entity => entity.AlbumId == albumId);
        }
        public Photo? GetLatestByAlbumId(int albumId)
        {
            return _collection.Find(entity => entity.AlbumId == albumId)
                               .OrderByDescending(x => x.CreatedAt)
                               .FirstOrDefault();
        }

        public IEnumerable<Photo> GetFavourite()
        {
            return _collection.Find(entity=>entity.IsFavourite);
        }

        public IEnumerable<Photo> GetRecent()
        {
            return _collection.FindAll().OrderBy(entity=>entity.CreatedAt);
        }
    }
}
