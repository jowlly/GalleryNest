using GalleryNestApp.Service;
using GalleryNestApp.ViewModel;
using GalleryNestServer.Entities;
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
    /// Логика взаимодействия для PersonPage.xaml
    /// </summary>
    public partial class PersonPage : Page
    {
        private PersonViewModel personViewModel;
        private WebView2Provider _webView2Provider;

        public WebView2Provider WebView2Provider { get => _webView2Provider; set => _webView2Provider = value; }

        public PersonPage(PersonViewModel selectionViewModel, WebView2Provider webView2Provider)
        {
            this.personViewModel = selectionViewModel;
            this._webView2Provider = webView2Provider;
            InitializeComponent();
            DataContext = this.personViewModel;
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

            var selectionId = Convert.ToString((webView.DataContext as Person)?.Guid);
            if (string.IsNullOrEmpty(selectionId)) return;

            try
            {
                personViewModel.LoadPersonToWebView(webView, selectionId);
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
                if (button != null && button.Command == personViewModel.DeletePersonCommand)
                    return;
            }

            if (sender is Grid grid && grid.DataContext is Person)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.NavigationService.NavigateTo<SelectionGalleryPage>((grid.DataContext as Person)!.Id);
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
