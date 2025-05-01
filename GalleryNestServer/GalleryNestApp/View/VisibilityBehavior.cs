using GalleryNestApp.ViewModel;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace GalleryNestApp.Behaviors
{
    public class VisibilityBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            AssociatedObject.IsVisibleChanged += OnVisibilityChanged;
        }

        private async void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (AssociatedObject.IsVisible &&
                AssociatedObject.DataContext is string photoId)
            {
                var container = AssociatedObject.FindName("WebViewContainer") as Grid;
                var webView = container?.FindName("PhotoWebView") as WebView2CompositionControl;

                if (webView != null && webView.CoreWebView2 == null)
                {
                    var itemsControl = ItemsControl.ItemsControlFromItemContainer(AssociatedObject);
                    var viewModel = itemsControl?.DataContext as PhotoViewModel;

                    if (viewModel != null)
                    {
                        await webView.EnsureCoreWebView2Async();
                        viewModel.LoadImageToWebView(webView, photoId);
                    }
                }
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.IsVisibleChanged -= OnVisibilityChanged;
        }
    }
}
