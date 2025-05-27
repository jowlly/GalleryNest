using GalleryNestApp.Service;
using GalleryNestApp.ViewModel;
using Microsoft.Web.WebView2.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace GalleryNestApp.View
{
    /// <summary>
    /// Логика взаимодействия для FavouritesPage.xaml
    /// </summary>
    public partial class FavouritesPage : Page
    {
        private FavouriteViewModel _favouriteViewModel;
        private WebView2Provider _webView2Provider;

        public WebView2Provider WebView2Provider { get => _webView2Provider; set => _webView2Provider = value; }


        public FavouritesPage(FavouriteViewModel favouriteViewModel, WebView2Provider webView2Provider)
        {
            this._favouriteViewModel = favouriteViewModel;
            this._webView2Provider = webView2Provider;
            InitializeComponent();
            DataContext = this._favouriteViewModel;
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
                _favouriteViewModel.LoadImageToWebView(webView, photoId);
            }
            catch (Exception ex)
            {
                webView.NavigateToString($"<html><body>Error: {ex.Message}</body></html>");
            }
        }
    }
}
