using GalleryNestApp.Model;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;

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

        public async Task UploadFile(string filePath, int albumId = 1)
        {
            using var fileStream = File.OpenRead(filePath);
            var fileName = System.IO.Path.GetFileName(filePath);

            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(fileStream), "file", fileName);

            var response = await client.PostAsync($"{url}/photo/upload?albumId={albumId}", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Server error: {error}");
            }
        }

        public async Task<IEnumerable<Photo>> LoadPhotosForAlbum(int albumId)
        {
            var response = await client.GetStringAsync($"{url}/photo/meta?albumId={albumId}");
            return JsonConvert.DeserializeObject<Photo[]>(response) ?? [];
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