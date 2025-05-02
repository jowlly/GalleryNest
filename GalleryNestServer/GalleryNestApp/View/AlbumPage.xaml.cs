using GalleryNestApp.Model;
using GalleryNestApp.Service;
using GalleryNestApp.ViewModel;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GalleryNestApp.View
{
    /// <summary>
    /// Логика взаимодействия для AlbumPage.xaml
    /// </summary>
    public partial class AlbumPage : Page
    {
        private AlbumViewModel albumViewModel;
        private WebView2Provider _webView2Provider;

        public WebView2Provider WebView2Provider { get => _webView2Provider; set => _webView2Provider = value; }

        public AlbumPage(AlbumViewModel albumViewModel, WebView2Provider webView2Provider)
        {
            this.albumViewModel = albumViewModel;
            this._webView2Provider = webView2Provider;
            InitializeComponent();
            DataContext = this.albumViewModel;
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
            var dialog = new AddAlbumDialog();
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.AlbumName))
            {
                albumViewModel.AlbumName = dialog.AlbumName;
                if (albumViewModel.AddAlbumCommand.CanExecute(null))
                {
                    albumViewModel.AddAlbumCommand.Execute(null);
                }
            }
        }

        private void WebViewContainer_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                var button = FindVisualParent<Button>(source);
                if (button != null && button.Command == albumViewModel.DeleteAlbumCommand)
                    return;
            }

            if (sender is Grid grid && grid.DataContext is Album)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.NavigationService.NavigateTo<AlbumGalleryPage>((grid.DataContext as Album)!.Id);
            }
        }
        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }
    }
}
