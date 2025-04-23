using GalleryNestApp.Service;
using Microsoft.Web.WebView2.Wpf;

namespace GalleryNestApp.ViewModel
{
    public class FavouriteViewModel
    {
        private PhotoService _photoService;

        public FavouriteViewModel(PhotoService photoService)
        {
            this._photoService = photoService;
        }

        public void LoadImageToWebView(WebView2CompositionControl webView, string photoId)
        {
            _photoService.LoadImageToWebView(webView, photoId);
        }

        public async Task UploadFile(string fileName)
        {
            await _photoService.UploadFile(fileName);
        }
    }
}