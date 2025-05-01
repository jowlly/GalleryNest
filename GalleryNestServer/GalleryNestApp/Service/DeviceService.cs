using GalleryNestApp.Model;
using System.Net.Http;

namespace GalleryNestApp.Service
{
    public class DeviceService(HttpClient client, string url) : EntityService<Device>(client, $"{url}/device")
    {
        public async Task<string> LoadQR() => await _httpClient.GetStringAsync($"{_url}/qr");

    }
}