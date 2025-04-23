using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GalleryNestApp.Service
{
    public class EntityService<T>(HttpClient client,string url)
    {
        protected readonly string _url = url;
        protected readonly HttpClient _httpClient = client;

        public async Task AddAsync(T album)
        {
            var content = new StringContent(JsonConvert.SerializeObject(album), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_url}", content);
        }

        public async Task EditAsync(T album)
        {
            var content = new StringContent(JsonConvert.SerializeObject(album), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_url}", content);
        }

        public async Task DeleteAsync(List<int> ids)
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(ids), Encoding.UTF8, "application/json"),
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"{_url}", UriKind.Relative)
            };
            await _httpClient.SendAsync(request);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var response = await _httpClient.GetStringAsync($"{_url}");
            return JsonConvert.DeserializeObject<T[]>(response) ?? [];
        }
    }
}
