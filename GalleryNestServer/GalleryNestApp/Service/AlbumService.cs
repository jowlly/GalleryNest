using System.Net.Http;
using Album = GalleryNestApp.Model.Album;

namespace GalleryNestApp.Service
{
    public class AlbumService(HttpClient client, string url) : EntityService<Album>(client, $"{url}/album")
    {
    }
}