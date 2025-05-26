using GalleryNestApp.Service;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace GalleryNestApp.View
{
    /// <summary>
    /// Логика взаимодействия для PhotoShowPage.xaml
    /// </summary>
    public partial class PhotoShowPage : Page
    {
        private WebView2Provider _webView2Provider;
        private PhotoShowViewModel _viewModel;

        public WebView2Provider WebView2Provider { get => _webView2Provider; set => _webView2Provider = value; }

        public PhotoShowPage(PhotoShowViewModel viewModel,WebView2Provider webView2Provider)
        {
            InitializeComponent();
            _webView2Provider = webView2Provider;
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private async void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            var webView = sender as WebView2CompositionControl;
            if (webView == null || WebView2Provider == null) return;

            ToggleLoadingIndicator(webView, true);

            try
            {
                var env = await WebView2Provider.GetEnvironmentAsync();
                await webView.EnsureCoreWebView2Async(env);

                if (_viewModel.LoadImageCommand?.CanExecute(webView) == true)
                    _viewModel.LoadImageCommand.Execute(webView);
            }
            catch (Exception ex)
            {
                ToggleLoadingIndicator(webView, false);
                webView.NavigateToString($"<html><body>Error: {ex.Message}</body></html>");
            }
        }



        private void WebView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is WebView2CompositionControl webView)
            {
                ToggleLoadingIndicator(webView, false);
                webView.CoreWebView2?.Stop();
                webView.Dispose();
            }
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var webView = sender as WebView2CompositionControl;
            ToggleLoadingIndicator(webView, false);

            if (!e.IsSuccess && webView != null)
            {
                webView.NavigateToString($"<html><body>Error: {e.WebErrorStatus}</body></html>");
            }
        }

        private void ToggleLoadingIndicator(WebView2CompositionControl webView, bool show)
        {
            if (webView == null) return;
            var parentGrid = FindVisualParent<Grid>(webView);
            if (parentGrid == null) return;
            var mainGrid = FindVisualParent<Grid>(parentGrid);

            var indicator = parentGrid.Children
                .OfType<ProgressRing>()
                .FirstOrDefault();

            if (indicator != null)
            {
                var animation = new DoubleAnimation
                {
                    To = show ? 1 : 0,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                indicator.BeginAnimation(UIElement.OpacityProperty, animation);

                Dispatcher.BeginInvoke(() =>
                {
                    indicator.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                }, DispatcherPriority.ContextIdle);
            }
        }
        private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            return parentObject is T parent ? parent : FindVisualParent<T>(parentObject);
        }
    }
}
