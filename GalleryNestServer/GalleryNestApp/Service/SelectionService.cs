using GalleryNestApp.Model;
using System.Net.Http;

namespace GalleryNestApp.Service
{
    public class SelectionService(HttpClient client, string url) : EntityService<Selection>(client, $"{url}/selection")
    {
    }
}