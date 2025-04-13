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
        public object GetByAlbumId(int albumId)
        {
            return _collection.Find(entity => entity.AlbumId == albumId);
        }

        public object GetFavourite()
        {
            return _collection.Find(entity=>entity.IsFavourite);
        }

        public object GetRecent()
        {
            return _collection.FindAll().OrderBy(entity=>entity.CreatedAt);
        }
    }
}
