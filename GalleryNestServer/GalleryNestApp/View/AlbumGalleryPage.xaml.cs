using GalleryNestApp.Service;
using GalleryNestApp.ViewModel;
using Microsoft.Web.WebView2.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace GalleryNestApp.View
{
    /// <summary>
    /// Логика взаимодействия для AlbumGalleryPage.xaml
    /// </summary>
    public partial class AlbumGalleryPage : Page
    {
        private AlbumGalleryViewModel _albumGalleryViewModel;
        private WebView2Provider _webView2Provider;

        public WebView2Provider WebView2Provider { get => _webView2Provider; set => _webView2Provider = value; }

        public AlbumGalleryPage(AlbumGalleryViewModel albumGalleryViewModel, WebView2Provider webView2Provider)
        {
            this._albumGalleryViewModel = albumGalleryViewModel;
            this._webView2Provider = webView2Provider;
            InitializeComponent();
            DataContext = this._albumGalleryViewModel;
        }

        private async void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            var webView = sender as WebView2CompositionControl;
            if (webView == null) return;

            if (webView.CoreWebView2 == null)
            {
                var env = await WebView2Provider.GetEnvironmentAsync();
                await webView.EnsureCoreWebView2Async(env);
            }

            var photoId = Convert.ToString(webView.DataContext);
            if (string.IsNullOrEmpty(photoId)) return;

            try
            {
                _albumGalleryViewModel.LoadImageToWebView(webView, photoId);
            }
            catch (Exception ex)
            {
                webView.NavigateToString($"<html><body>Error: {ex.Message}</body></html>");
            }
        }


        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All Files|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    await _albumGalleryViewModel.UploadFile(openFileDialog.FileNames.ToList());
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
