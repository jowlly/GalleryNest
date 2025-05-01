using GalleryNestApp.Model;
using GalleryNestApp.ViewModel;
using Microsoft.Web.WebView2.Core;
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
    /// Логика взаимодействия для AlbumPage.xaml
    /// </summary>
    public partial class AlbumPage : Page
    {
        private AlbumViewModel albumViewModel;

        public AlbumPage(AlbumViewModel albumViewModel)
        {
            this.albumViewModel = albumViewModel;
            InitializeComponent();
            DataContext = this.albumViewModel;
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

            var albumId = Convert.ToString((webView.DataContext as Album)?.Id);
            if (string.IsNullOrEmpty(albumId)) return;

            try
            {
                albumViewModel.LoadAlbumToWebView(webView, albumId);
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
                    await albumViewModel.UploadFile(openFileDialog.FileNames.ToList());
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
