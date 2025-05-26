using GalleryNestApp.Service;
using GalleryNestApp.ViewModel;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GalleryNestApp.View
{
    /// <summary>
    /// Логика взаимодействия для PersonGalleryPage.xaml
    /// </summary>
    public partial class PersonGalleryPage : Page
    {
        private PersonGalleryViewModel _personGalleryViewModel;
        private WebView2Provider _webView2Provider;

        public WebView2Provider WebView2Provider { get => _webView2Provider; set => _webView2Provider = value; }

        public PersonGalleryPage(PersonGalleryViewModel albumGalleryViewModel, WebView2Provider webView2Provider)
        {
            this._personGalleryViewModel = albumGalleryViewModel;
            this._webView2Provider = webView2Provider;
            InitializeComponent();
            DataContext = this._personGalleryViewModel;
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
                _personGalleryViewModel.LoadImageToWebView(webView, photoId);
            }
            catch (Exception ex)
            {
                webView.NavigateToString($"<html><body>Error: {ex.Message}</body></html>");
            }
        }
    }
}
