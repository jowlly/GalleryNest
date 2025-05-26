using GalleryNestApp.Model;
using System.Net.Http;

namespace GalleryNestApp.Service
{
    public class PersonService(HttpClient client, string url) : EntityService<Person>(client, $"{url}/person")
    {
    }
}