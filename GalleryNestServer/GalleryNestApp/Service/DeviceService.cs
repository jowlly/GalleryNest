using GalleryNestApp.Model;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace GalleryNestApp.Service
{
    public class DeviceService(HttpClient client, string url) : EntityService<Album>(client, $"{url}/device")
    {

    }
}