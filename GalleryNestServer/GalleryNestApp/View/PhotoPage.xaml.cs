using GalleryNestApp.ViewModel;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;

namespace GalleryNestApp
{
    /// <summary>
    /// Логика взаимодействия для PhotoPage.xaml
    /// </summary>
    public partial class PhotoPage : Page
    {
        private PhotoViewModel photoViewModel;

        public PhotoPage(PhotoViewModel photoViewModel)
        {
            this.photoViewModel = photoViewModel;
            InitializeComponent();
            DataContext = this.photoViewModel;
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await CoreWebView2Environment.CreateAsync();
        }
        
        private async void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            var webView = sender as WebView2CompositionControl;
            if (webView == null) return;

            if (webView.CoreWebView2 == null)
            {
                var env = await CoreWebView2Environment.CreateAsync();
                await webView.EnsureCoreWebView2Async(env);
            }

            var photoId = Convert.ToString(webView.DataContext);
            if (string.IsNullOrEmpty(photoId)) return;

            try
            {
                photoViewModel.LoadImageToWebView(webView, photoId);
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
                    await photoViewModel.UploadFile(openFileDialog.FileNames.ToList());
                }
                catch (Exception ex)
                {
                }
            }
        }
        
    }
}
