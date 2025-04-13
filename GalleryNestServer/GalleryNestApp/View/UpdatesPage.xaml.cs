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
    /// Логика взаимодействия для UpdatesPage.xaml
    /// </summary>
    public partial class UpdatesPage : Page
    {
        private UpdateViewModel updateViewModel;

        public UpdatesPage(UpdateViewModel updateViewModel)
        {
            this.updateViewModel = updateViewModel;
            InitializeComponent();
            DataContext = this.updateViewModel;
        }

        private async void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            var webView = sender as WebView2;
            if (webView == null) return;

            var photoId = Convert.ToString(webView.DataContext);
            if (string.IsNullOrEmpty(photoId)) return;

            try
            {
                await webView.EnsureCoreWebView2Async();
                updateViewModel.LoadImageToWebView(webView, photoId);
            }
            catch (Exception ex)
            {
                webView.NavigateToString($"<html><body>Error: {ex.Message}</body></html>");
            }
        }
    }
}
