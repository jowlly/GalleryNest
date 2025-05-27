using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using UMapx.Response;
using Photo = GalleryNestApp.Model.Photo;

namespace GalleryNestApp.Service
{
    public class PhotoService(HttpClient client, string url) : EntityService<Photo>(client, $"{url}/photo/meta")
    {
        public void LoadImageToWebView(WebView2CompositionControl webView, string photoId,bool contain = false)
        {
            var response = client.GetAsync($"{url}/photo/download?photoId={photoId}").Result;
            try
            {
                var html = $@"
                        <html>
                            <head>
                                <style>
                                    body {{
                                        margin: 0;
                                        padding: 0;
                                        overflow: hidden;
                                        background-color: transparent;
                                    }}
                                    .image-container {{
                                        width: 100%;
                                        height: 100%;
                                        overflow: hidden;
                                        position: relative; 
                                        transform: translateZ(0); 
                                    }}
                                    img {{
                                        width: 100%;
                                        height: 100%;
                                        object-fit: {((contain)? $@"contain":$@"cover")};
                                    }}
                                    video {{
                                        width: 100%;
                                        height: 100%;
                                        object-fit: {((contain) ? $@"contain" : $@"cover")};
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='image-container'>
                                    {(response.Content.Headers.ContentType.MediaType.Contains("video")
                                        ? $@"<video {((contain) ? $@"controls" : $@"")} muted >
                                                <source src='{url}/photo/download?photoId={photoId}'>
                                                </video>"
                                        : $@"<img src='{url}/photo/download?photoId={photoId}' alt='Media content'>"
                                     )}
                                </div>
                            </body>
                        </html>";

                webView.NavigateToString(html);
            }
            catch (Exception ex)
            {
                webView.NavigateToString($"<html><body>Error loading image: {ex.Message}</body></html>");
            }
        }

        public void LoadAlbumPreviewWebView(WebView2CompositionControl webView, string albumId)
        {
            var response = client.GetAsync($"{url}/photo/download/album/latest?albumId={albumId}").Result;
            try
            {
                var html = $@"
                        <html>
                            <head>
                                <style>
                                    body {{
                                        margin: 0;
                                        padding: 0;
                                        overflow: hidden;
                                        background-color: transparent;
                                    }}
                                    .image-container {{
                                        width: 100%;
                                        height: 100%;
                                        overflow: hidden;
                                        position: relative; 
                                        transform: translateZ(0); 
                                    }}
                                    img {{
                                        width: 100%;
                                        height: 100%;
                                        object-fit: cover;
                                    }}
                                    video {{
                                        width: 100%;
                                        height: 100%;
                                        object-fit: cover;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='image-container'>
                                    {(response.Content.Headers.ContentType.MediaType.Contains("video")
                                        ? $@"<video muted >
                                                <source src='{url}/photo/download/album/latest?albumId={albumId}'>
                                                </video>"
                                        : $@"<img src='{url}/photo/download/album/latest?albumId={albumId}' alt='Media content'>"
                                     )}
                                </div>
                            </body>
                        </html>";

                webView.NavigateToString(html);
            }
            catch (Exception ex)
            {
                webView.NavigateToString($"<html><body>Error loading album: {ex.Message}</body></html>");
            }
        }

        public void LoadSelectionPreviewWebView(WebView2CompositionControl webView, string selectionId)
        {
            try
            {
                var html = $@"
                        <html>
                            <head>
                                <style>
                                    body {{
                                        margin: 0;
                                        padding: 0;
                                        overflow: hidden;
                                        background-color: transparent;
                                    }}
                                    .image-container {{
                                        width: 100%;
                                        height: 100%;
                                        overflow: hidden;
                                        position: relative; 
                                        transform: translateZ(0); 
                                    }}
                                    img {{
                                        width: 100%;
                                        height: 100%;
                                        object-fit: cover;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='image-container'>
                                    <img src='{url}/photo/download/selection/latest?selectionId={selectionId}'/>
                                </div>
                            </body>
                        </html>";

                webView.NavigateToString(html);
            }
            catch (Exception ex)
            {
                webView.NavigateToString($"<html><body>Error loading album: {ex.Message}</body></html>");
            }
        }


        public void LoadPersonPreviewWebView(WebView2CompositionControl webView, string personId)
        {
            try
            {
                var html = $@"
                        <html>
                            <head>
                                <style>
                                    body {{
                                        margin: 0;
                                        padding: 0;
                                        overflow: hidden;
                                        background-color: transparent;
                                    }}
                                    .image-container {{
                                        width: 100%;
                                        height: 100%;
                                        overflow: hidden;
                                        position: relative; 
                                        transform: translateZ(0); 
                                    }}
                                    img {{
                                        width: 100%;
                                        height: 100%;
                                        object-fit: cover;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='image-container'>
                                    <img src='{url}/photo/download/person/latest?personId={personId}'/>
                                </div>
                            </body>
                        </html>";

                webView.NavigateToString(html);
            }
            catch (Exception ex)
            {
                webView.NavigateToString($"<html><body>Error loading album: {ex.Message}</body></html>");
            }
        }
        
        public async Task UploadFile(string filePath, List<int> albumIds)
        {
            var fileInfo = new FileInfo(filePath);
            var creationTime = GetFileCreationTime(fileInfo);

            using var fileStream = File.OpenRead(filePath);
            var fileName = Path.GetFileName(filePath);

            using var content = new MultipartFormDataContent
            {
                { new StreamContent(fileStream), "file", fileName },
                { new StringContent(creationTime.ToString("o")), "creationTime" }
            };

            var uriBuilder = new UriBuilder($"{url}/photo/upload");
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

            foreach (var id in albumIds)
            {
                query.Add("albumIds", id.ToString());
            }

            uriBuilder.Query = query.ToString();

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = uriBuilder.Uri,
                Content = content
            };

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Server error: {error}");
            }
        }

        private DateTime GetFileCreationTime(FileInfo fileInfo)
        {
            try
            {
                using var image = Image.FromFile(fileInfo.FullName);
                var propItem = image.GetPropertyItem(36867);
                if (propItem != null)
                {
                    var dateString = Encoding.UTF8.GetString(propItem.Value).Trim('\0');
                    if (DateTime.TryParseExact(dateString, "yyyy:MM:dd HH:mm:ss",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var exifDate))
                    {
                        return exifDate.ToUniversalTime();
                    }
                }
            }
            catch {}
            return new[] { fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc }
                .OrderBy(t => t)
                .First();
        }

        public async Task<IEnumerable<Photo>> LoadPhotosForAlbum(int albumId, int page, int pageSize)
        {
            var uriBuilder = new UriBuilder($"{url}/photo/meta/album");
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["albumId"] = albumId.ToString();
            query["pageNumber"] = page.ToString();
            query["pageSize"] = pageSize.ToString();
            uriBuilder.Query = query.ToString();

            var response = await client.GetStringAsync(uriBuilder.Uri.ToString());
            var result = JsonConvert.DeserializeObject<IEnumerable<Photo>>(response);
            return result ?? [];
        }

        public async Task<IEnumerable<Photo>> LoadFavouritePhotos(int currentPage, int pageSize)
        {
            var uriBuilder = new UriBuilder($"{url}/photo/meta/favourite");
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["pageNumber"] = currentPage.ToString();
            query["pageSize"] = pageSize.ToString();
            uriBuilder.Query = query.ToString();

            var response = await client.GetStringAsync(uriBuilder.Uri.ToString());
            var result = JsonConvert.DeserializeObject<IEnumerable<Photo>>(response);
            return result ?? [];
        }
        public async Task<IEnumerable<Photo>> LoadPhotosForSelection(int selectionId, int currentPage, int pageSize)
        {
            var uriBuilder = new UriBuilder($"{url}/photo/meta/selection");
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["selectionId"] = selectionId.ToString();
            query["pageNumber"] = currentPage.ToString();
            query["pageSize"] = pageSize.ToString();
            uriBuilder.Query = query.ToString();

            var response = await client.GetStringAsync(uriBuilder.Uri.ToString());
            var result = JsonConvert.DeserializeObject<IEnumerable<Photo>>(response);
            return result ?? [];
        }
        public async Task<IEnumerable<Photo>> LoadPhotosForPerson(string personId, int currentPage, int pageSize)
        {
            var uriBuilder = new UriBuilder($"{url}/photo/meta/person");
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["personId"] = personId.ToString();
            query["pageNumber"] = currentPage.ToString();
            query["pageSize"] = pageSize.ToString();
            uriBuilder.Query = query.ToString();

            var response = await client.GetStringAsync(uriBuilder.Uri.ToString());
            var result = JsonConvert.DeserializeObject<IEnumerable<Photo>>(response);
            return result ?? [];
        }

        public async Task<int> GetTotalPagesAsync(int pageSize)
        {
            var uriBuilder = new UriBuilder($"{url}/photo/meta/count");
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            uriBuilder.Query = query.ToString();

            var response = await client.GetStringAsync(uriBuilder.Uri.ToString());
            var result = JsonConvert.DeserializeObject<int>(response);
            return result;
        }

        internal async Task GetFavouritePagedAsync(int currentPage, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}