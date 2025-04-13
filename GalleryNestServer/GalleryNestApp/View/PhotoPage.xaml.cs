using GalleryNestApp.Model;
using GalleryNestApp.ViewModel;
using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
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
using static QRCoder.PayloadGenerator;
using Wpf.Ui.Controls;

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
