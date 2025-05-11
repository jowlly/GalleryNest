using GalleryNestApp.Model;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;

namespace GalleryNestApp.Service
{
    public class PhotoService(HttpClient client, string url) : EntityService<Photo>(client, $"{url}/photo/meta")
    {
        public void LoadImageToWebView(WebView2CompositionControl webView, string photoId)
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
                                    <img src='{url}/photo/download?photoId={photoId}'/>
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
                                    <img src='{url}/photo/download/latest?albumId={albumId}'/>
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

        //public async Task UploadFile(string filePath, int albumId = 1)
        //{
        //    using var fileStream = File.OpenRead(filePath);
        //    var fileName = System.IO.Path.GetFileName(filePath);

        //    using var content = new MultipartFormDataContent();
        //    content.Add(new StreamContent(fileStream), "file", fileName);

        //    var response = await client.PostAsync($"{url}/photo/upload?albumId={albumId}", content);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var error = await response.Content.ReadAsStringAsync();
        //        throw new Exception($"Server error: {error}");
        //    }
        //}

        public async Task UploadFile(string filePath, int albumId = 1)
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

            var response = await client.PostAsync($"{url}/photo/upload?albumId={albumId}", content);

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

        private async Task<IEnumerable<Photo>> LoadFavouritePhotos()
        {
            var response = await client.GetStringAsync($"{url}/photo/meta/favourite");
            return JsonConvert.DeserializeObject<Photo[]>(response) ?? [];
        }
        private async Task<IEnumerable<Photo>> LoadUpdatePhotos()
        {
            var response = await client.GetStringAsync($"{url}/photo/meta/recent");
            return JsonConvert.DeserializeObject<Photo[]>(response) ?? [];
        }

    }
}