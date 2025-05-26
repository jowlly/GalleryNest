using GalleryNestApp.Service;
using GalleryNestApp.ViewModel;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Wpf.Ui.Controls;
using Button = Wpf.Ui.Controls.Button;

namespace GalleryNestApp.View
{
    public partial class PhotoGalleryControl : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(PhotoGalleryControl));

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register("DeleteCommand", typeof(ICommand), typeof(PhotoGalleryControl));

        public static readonly DependencyProperty LoadImageCommandProperty =
            DependencyProperty.Register("LoadImageCommand", typeof(ICommand), typeof(PhotoGalleryControl));

        public static readonly DependencyProperty WebView2EnvironmentProviderProperty =
       DependencyProperty.Register(
           "WebView2EnvironmentProvider",
           typeof(WebView2Provider),
           typeof(PhotoGalleryControl));

        public static readonly DependencyProperty ShowAlbumInfoProperty =
        DependencyProperty.Register(
            "ShowAlbumInfo",
            typeof(bool),
            typeof(PhotoGalleryControl),
            new PropertyMetadata(false));

        public static readonly DependencyProperty ShowDeleteButtonProperty =
        DependencyProperty.Register(
            "ShowDeleteButton",
            typeof(bool),
            typeof(PhotoGalleryControl),
            new PropertyMetadata(false));

        public static readonly DependencyProperty ItemClickCommandProperty =
            DependencyProperty.Register(
                "ItemClickCommand",
                typeof(ICommand),
                typeof(PhotoGalleryControl));

        public bool ShowAlbumInfo
        {
            get => (bool)GetValue(ShowAlbumInfoProperty);
            set => SetValue(ShowAlbumInfoProperty, value);
        }
        public bool ShowDeleteButton
        {
            get => (bool)GetValue(ShowDeleteButtonProperty);
            set => SetValue(ShowDeleteButtonProperty, value);
        }

        public ICommand ItemClickCommand
        {
            get => (ICommand)GetValue(ItemClickCommandProperty);
            set => SetValue(ItemClickCommandProperty, value);
        }

        public WebView2Provider WebView2EnvironmentProvider
        {
            get => (WebView2Provider)GetValue(WebView2EnvironmentProviderProperty);
            set => SetValue(WebView2EnvironmentProviderProperty, value);
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public ICommand DeleteCommand
        {
            get => (ICommand)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }

        public ICommand LoadImageCommand
        {
            get => (ICommand)GetValue(LoadImageCommandProperty);
            set => SetValue(LoadImageCommandProperty, value);
        }

        public PhotoGalleryControl()
        {
            InitializeComponent();
        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                var button = FindVisualParent<Button>(source);
                if (button != null) return;
            }

            if (sender is FrameworkElement element &&
                ItemClickCommand?.CanExecute(element.DataContext) == true)
            {
                ItemClickCommand.Execute(element.DataContext);
            }
        }

        private async void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            var webView = sender as WebView2CompositionControl;
            if (webView == null || WebView2EnvironmentProvider == null) return;

            ToggleLoadingIndicator(webView, true);

            try
            {
                var env = await WebView2EnvironmentProvider.GetEnvironmentAsync();
                await webView.EnsureCoreWebView2Async(env);

                if (LoadImageCommand?.CanExecute(webView) == true)
                    LoadImageCommand.Execute(webView);
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

            var deleteButton = parentGrid.Children
                .OfType<Button>()
                .FirstOrDefault();

            var albumInfo = mainGrid.Children
                .OfType<StackPanel>()
                .FirstOrDefault();

            if (indicator != null && deleteButton != null && albumInfo != null)
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
                    if (ShowDeleteButton)
                        deleteButton.Visibility = !show ? Visibility.Visible : Visibility.Collapsed;
                    if (ShowAlbumInfo)
                        albumInfo.Visibility = !show ? Visibility.Visible : Visibility.Collapsed;
                }, DispatcherPriority.ContextIdle);
            }
        }
        private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            return parentObject is T parent ? parent : FindVisualParent<T>(parentObject);
        }

        private DateTime _lastScrollTime = DateTime.MinValue;
        private double _lastVerticalOffset = 0;

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            var currentTime = DateTime.Now;

            // Bounce-эффект при достижении границ
            if (e.VerticalOffset == 0 || e.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                var transform = new TranslateTransform();
                scrollViewer.RenderTransform = transform;

                var animation = new DoubleAnimationUsingKeyFrames
                {
                    Duration = TimeSpan.FromSeconds(0.8),
                    KeyFrames = {
                        new EasingDoubleKeyFrame(
                            0,
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                            new ElasticEase {
                                Oscillations = 1,
                                Springiness = 5,
                                EasingMode = EasingMode.EaseOut
                            }),
                        new EasingDoubleKeyFrame(
                            e.VerticalChange > 0 ? -15 : 15,
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2))),
                        new EasingDoubleKeyFrame(
                            0,
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.8)))
                    }
                };

                transform.BeginAnimation(TranslateTransform.YProperty, animation);
            }
            if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight * 0.8 &&
                (currentTime - _lastScrollTime).TotalMilliseconds > 500 &&
                _lastVerticalOffset < scrollViewer.VerticalOffset)
            {
                _lastScrollTime = currentTime;
                Dispatcher.BeginInvoke(async () =>
                {
                    if (DataContext is PhotoViewModel)
                        (DataContext as PhotoViewModel)!.LoadNextPageCommand.Execute(null);
                    if (DataContext is AlbumGalleryViewModel)
                        (DataContext as AlbumGalleryViewModel)!.LoadNextPageCommand.Execute(null);
                    if (DataContext is AlbumViewModel)
                        (DataContext as AlbumViewModel)!.LoadNextPageCommand.Execute(null);
                    if (DataContext is SelectionGalleryViewModel)
                        (DataContext as SelectionGalleryViewModel)!.LoadNextPageCommand.Execute(null);
                    if (DataContext is SelectionsViewModel)
                        (DataContext as SelectionsViewModel)!.LoadNextPageCommand.Execute(null);
                    if (DataContext is PersonGalleryViewModel)
                        (DataContext as PersonGalleryViewModel)!.LoadNextPageCommand.Execute(null);
                    if (DataContext is PersonViewModel)
                        (DataContext as PersonViewModel)!.LoadNextPageCommand.Execute(null);
                    scrollViewer.ScrollToVerticalOffset(_lastVerticalOffset);
                }, DispatcherPriority.Background);
            }

            _lastVerticalOffset = scrollViewer.VerticalOffset;
        }
    }
}
