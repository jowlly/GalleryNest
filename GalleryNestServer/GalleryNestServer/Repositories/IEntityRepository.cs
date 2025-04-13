using GalleryNestServer.Entities;

namespace GalleryNestServer.Data
{
    public interface IEntityRepository<T> where T : IdentifiableEntity
    {
        public IEnumerable<T> GetAll();
        public T GetById(int id);
        public void Set(IEnumerable<T> photos);
        public void Delete(IEnumerable<int> id);
    }
}
