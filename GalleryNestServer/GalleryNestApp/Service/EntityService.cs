using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace GalleryNestApp.Service
{
    public class EntityService<T>(HttpClient client, string url)
    {
        protected readonly string _url = url;
        protected readonly HttpClient _httpClient = client;

        public async Task AddAsync(T album)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new List<T> { album }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_url}", content);
        }

        public async Task EditAsync(IEnumerable<T> album)
        {
            var json = JsonConvert.SerializeObject(album);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var uriBuilder = new UriBuilder($"{_url}");
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = content,
                RequestUri = uriBuilder.Uri
            };
            var response = await _httpClient.SendAsync(request);

        }

        public async Task DeleteAsync(List<int> ids)
        {
            var uriBuilder = new UriBuilder($"{_url}");
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

            foreach (var id in ids)
            {
                query.Add("ids", id.ToString());
            }

            uriBuilder.Query = query.ToString();

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = uriBuilder.Uri
            };

            await _httpClient.SendAsync(request);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var response = await _httpClient.GetStringAsync($"{_url}");
            return JsonConvert.DeserializeObject<T[]>(response) ?? [];
        }

        public async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize)
        {
            var uriBuilder = new UriBuilder($"{_url}");
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["pageNumber"] = page.ToString();
            query["pageSize"] = pageSize.ToString();
            uriBuilder.Query = query.ToString();

            var response = await _httpClient.GetStringAsync(uriBuilder.Uri.ToString());
            var result = JsonConvert.DeserializeObject<IEnumerable<T>>(response);
            return result ?? [];
        }

    }
}
