using GalleryNestApp.Service;
using GalleryNestApp.ViewModel;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace GalleryNestApp
{
    /// <summary>
    /// Логика взаимодействия для PhotoPage.xaml
    /// </summary>
    public partial class PhotoPage : Page
    {
        private PhotoViewModel _photoViewModel;
        private WebView2Provider _webView2Provider;

        public WebView2Provider WebView2Provider { get => _webView2Provider; set => _webView2Provider = value; }

        public PhotoPage(PhotoViewModel photoViewModel, WebView2Provider webView2Provider)
        {
            this._photoViewModel = photoViewModel;
            this._webView2Provider = webView2Provider;
            InitializeComponent();
            DataContext = this._photoViewModel;
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
                _photoViewModel.LoadImageToWebView(webView, photoId);
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
                    await _photoViewModel.UploadFile(openFileDialog.FileNames.ToList());
                }
                catch (Exception ex)
                {
                }
            }
        }

        private async void WebAlbumView_Loaded(object sender, RoutedEventArgs e)
        {
            var webView = sender as WebView2CompositionControl;
            if (webView == null || WebView2Provider == null) return;


            try
            {
                var env = await WebView2Provider.GetEnvironmentAsync();
                await webView.EnsureCoreWebView2Async(env);

                if (_photoViewModel.LoadLastAlbumImageCommand?.CanExecute(webView) == true)
                    _photoViewModel.LoadLastAlbumImageCommand.Execute(webView);
            }
            catch (Exception ex)
            {
                webView.NavigateToString($"<html><body>Error: {ex.Message}</body></html>");
            }
        }

        private async void WebCategoryView_Loaded(object sender, RoutedEventArgs e)
        {
            var webView = sender as WebView2CompositionControl;
            if (webView == null || WebView2Provider == null) return;


            try
            {
                var env = await WebView2Provider.GetEnvironmentAsync();
                await webView.EnsureCoreWebView2Async(env);

                if (_photoViewModel.LoadLastCategoryImageCommand?.CanExecute(webView) == true)
                    _photoViewModel.LoadLastCategoryImageCommand.Execute(webView);
            }
            catch (Exception ex)
            {
                webView.NavigateToString($"<html><body>Error: {ex.Message}</body></html>");
            }
        }

        private async void WebPersonView_Loaded(object sender, RoutedEventArgs e)
        {
            var webView = sender as WebView2CompositionControl;
            if (webView == null || WebView2Provider == null) return;


            try
            {
                var env = await WebView2Provider.GetEnvironmentAsync();
                await webView.EnsureCoreWebView2Async(env);

                if (_photoViewModel.LoadLastPersonImageCommand?.CanExecute(webView) == true)
                    _photoViewModel.LoadLastPersonImageCommand.Execute(webView);
            }
            catch (Exception ex)
            {
                webView.NavigateToString($"<html><body>Error: {ex.Message}</body></html>");
            }
        }

        private void WebView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is WebView2CompositionControl webView)
            {
                try
                {
                    webView.CoreWebView2?.Stop();
                    webView.Dispose();
                }
                catch { }
            }
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var webView = sender as WebView2CompositionControl;

            if (!e.IsSuccess && webView != null)
            {
                webView.NavigateToString($"<html><body>Error: {e.WebErrorStatus}</body></html>");
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button button)
            {
                var contextMenu = button.ContextMenu;
                if (contextMenu != null)
                {
                    contextMenu.PlacementTarget = button;
                    contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                    contextMenu.IsOpen = true;
                }
            }
        }
    }
}
