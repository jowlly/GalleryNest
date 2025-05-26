using GalleryNestApp.Model;
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
    public partial class SelectionsPage : Page
    {
        private SelectionsViewModel selectionViewModel;
        private WebView2Provider _webView2Provider;

        public WebView2Provider WebView2Provider { get => _webView2Provider; set => _webView2Provider = value; }

        public SelectionsPage(SelectionsViewModel selectionViewModel, WebView2Provider webView2Provider)
        {
            this.selectionViewModel = selectionViewModel;
            this._webView2Provider = webView2Provider;
            InitializeComponent();
            DataContext = this.selectionViewModel;
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

            var selectionId = Convert.ToString((webView.DataContext as Selection)?.Id);
            if (string.IsNullOrEmpty(selectionId)) return;

            try
            {
                selectionViewModel.LoadSelectionToWebView(webView, selectionId);
            }
            catch (Exception ex)
            {
                webView.NavigateToString($"<html><body>Error: {ex.Message}</body></html>");
            }

        }

        private void WebViewContainer_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                var button = FindVisualParent<Button>(source);
                if (button != null && button.Command == selectionViewModel.DeleteSelectionCommand)
                    return;
            }

            if (sender is Grid grid && grid.DataContext is Selection)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.NavigationService.NavigateTo<SelectionGalleryPage>((grid.DataContext as Selection)!.Id);
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
