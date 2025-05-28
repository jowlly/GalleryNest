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
using TextBlock = System.Windows.Controls.TextBlock;
using TextBox = System.Windows.Controls.TextBox;

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

        public static readonly DependencyProperty SaveAlbumCommandProperty =
           DependencyProperty.Register("SaveAlbumCommand", typeof(ICommand), typeof(PhotoGalleryControl));
        public ICommand SaveAlbumCommand
        {
            get => (ICommand)GetValue(SaveAlbumCommandProperty);
            set => SetValue(SaveAlbumCommandProperty, value);
        }


        public ICommand LoadImageCommand
        {
            get => (ICommand)GetValue(LoadImageCommandProperty);
            set => SetValue(LoadImageCommandProperty, value);
        }
        public static readonly DependencyProperty LoadLastAlbumImageCommandProperty =
            DependencyProperty.Register("LoadLastAlbumImageCommand", typeof(ICommand), typeof(PhotoGalleryControl));

        public ICommand LoadLastAlbumImageCommand
        {
            get => (ICommand)GetValue(LoadLastAlbumImageCommandProperty);
            set => SetValue(LoadLastAlbumImageCommandProperty, value);
        }
        public static readonly DependencyProperty LoadLastCategoryImageCommandProperty =
            DependencyProperty.Register("LoadLastCategoryImageCommand", typeof(ICommand), typeof(PhotoGalleryControl));

        public ICommand LoadLastCategoryImageCommand
        {
            get => (ICommand)GetValue(LoadLastCategoryImageCommandProperty);
            set => SetValue(LoadLastCategoryImageCommandProperty, value);
        }
        public static readonly DependencyProperty LoadLastPersonImageCommandProperty =
            DependencyProperty.Register("LoadLastPersonImageCommand", typeof(ICommand), typeof(PhotoGalleryControl));

        public ICommand LoadLastPersonImageCommand
        {
            get => (ICommand)GetValue(LoadLastPersonImageCommandProperty);
            set => SetValue(LoadLastPersonImageCommandProperty, value);
        }

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


        public static readonly DependencyProperty AlbumsSourceProperty =
            DependencyProperty.Register(
                "AlbumsSource",
                typeof(IEnumerable),
                typeof(PhotoGalleryControl));
        public static readonly DependencyProperty CategoriesSourceProperty =
            DependencyProperty.Register(
                "CategoriesSource",
                typeof(IEnumerable),
                typeof(PhotoGalleryControl));
        public static readonly DependencyProperty PeopleSourceProperty =
            DependencyProperty.Register(
                "PeopleSource",
                typeof(IEnumerable),
                typeof(PhotoGalleryControl));

        public static readonly DependencyProperty AddToFavoritesCommandProperty =
            DependencyProperty.Register(
                "AddToFavoritesCommand",
                typeof(ICommand),
                typeof(PhotoGalleryControl));

        public static readonly DependencyProperty AddToAlbumCommandProperty =
            DependencyProperty.Register(
                "AddToAlbumCommand",
                typeof(ICommand),
                typeof(PhotoGalleryControl));

        public static readonly DependencyProperty AddToCategoryCommandProperty =
            DependencyProperty.Register(
                "AddToCategoryCommand",
                typeof(ICommand),
                typeof(PhotoGalleryControl));

        public static readonly DependencyProperty AddToPersonCommandProperty =
            DependencyProperty.Register(
                "AddToPersonCommand",
                typeof(ICommand),
                typeof(PhotoGalleryControl));


        public IEnumerable AlbumsSource
        {
            get => (IEnumerable)GetValue(AlbumsSourceProperty);
            set => SetValue(AlbumsSourceProperty, value);
        }

        public ICommand AddToFavoritesCommand
        {
            get => (ICommand)GetValue(AddToFavoritesCommandProperty);
            set => SetValue(AddToFavoritesCommandProperty, value);
        }

        public ICommand AddToPersonCommand
        {
            get => (ICommand)GetValue(AddToPersonCommandProperty);
            set => SetValue(AddToPersonCommandProperty, value);
        }

        public ICommand AddToAlbumCommand
        {
            get => (ICommand)GetValue(AddToAlbumCommandProperty);
            set => SetValue(AddToAlbumCommandProperty, value);
        }

        public ICommand AddToCategoryCommand
        {
            get => (ICommand)GetValue(AddToCategoryCommandProperty);
            set => SetValue(AddToCategoryCommandProperty, value);
        }

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

        public IEnumerable CategoriesSource
        {
            get => (IEnumerable)GetValue(CategoriesSourceProperty);
            set => SetValue(CategoriesSourceProperty, value);
        }

        public IEnumerable PeopleSource
        {
            get => (IEnumerable)GetValue(PeopleSourceProperty);
            set => SetValue(PeopleSourceProperty, value);
        }
        public ICommand DeleteCommand
        {
            get => (ICommand)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
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
                var checkbox = FindVisualParent<CheckBox>(source);
                if (button != null || checkbox != null) return;
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

        private async void WebAlbumView_Loaded(object sender, RoutedEventArgs e)
        {
            var webView = sender as WebView2CompositionControl;
            if (webView == null || WebView2EnvironmentProvider == null) return;

            ToggleLoadingIndicator(webView, true);

            try
            {
                var env = await WebView2EnvironmentProvider.GetEnvironmentAsync();
                await webView.EnsureCoreWebView2Async(env);

                if (LoadLastAlbumImageCommand?.CanExecute(webView) == true)
                    LoadLastAlbumImageCommand.Execute(webView);
            }
            catch (Exception ex)
            {
                ToggleLoadingIndicator(webView, false);
                webView.NavigateToString($"<html><body>Error: {ex.Message}</body></html>");
            }
        }

        private async void WebCategoryView_Loaded(object sender, RoutedEventArgs e)
        {
            var webView = sender as WebView2CompositionControl;
            if (webView == null || WebView2EnvironmentProvider == null) return;

            ToggleLoadingIndicator(webView, true);

            try
            {
                var env = await WebView2EnvironmentProvider.GetEnvironmentAsync();
                await webView.EnsureCoreWebView2Async(env);

                if (LoadLastCategoryImageCommand?.CanExecute(webView) == true)
                    LoadLastCategoryImageCommand.Execute(webView);
            }
            catch (Exception ex)
            {
                ToggleLoadingIndicator(webView, false);
                webView.NavigateToString($"<html><body>Error: {ex.Message}</body></html>");
            }
        }

        private async void WebPersonView_Loaded(object sender, RoutedEventArgs e)
        {
            var webView = sender as WebView2CompositionControl;
            if (webView == null || WebView2EnvironmentProvider == null) return;

            ToggleLoadingIndicator(webView, true);

            try
            {
                var env = await WebView2EnvironmentProvider.GetEnvironmentAsync();
                await webView.EnsureCoreWebView2Async(env);

                if (LoadLastPersonImageCommand?.CanExecute(webView) == true)
                    LoadLastPersonImageCommand.Execute(webView);
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


            var albumInfo = mainGrid.Children
                .OfType<StackPanel>()
                .FirstOrDefault();

            var menuButton = parentGrid.Children
                .OfType<Button>()
                .FirstOrDefault(b => b.Name == "MenuButton");



            if (menuButton != null)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    menuButton.Visibility = !show && true
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }, DispatcherPriority.ContextIdle);
            }
            if (indicator != null && albumInfo != null)
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

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var stackPanel = FindVisualParent<StackPanel>(button);
                if (stackPanel == null) return;
                var textBlock = stackPanel.FindName("AlbumNameText") as TextBlock;
                var editButton = stackPanel.FindName("EditButton") as Button;
                var editPanel = stackPanel.FindName("EditPanel") as StackPanel;
                var editTextBox = editPanel?.FindName("EditTextBox") as TextBox;

                if (textBlock != null && editPanel != null && editTextBox != null)
                {
                    _originalName = textBlock.Text;

                    textBlock.Visibility = Visibility.Collapsed;
                    editButton.Visibility = Visibility.Collapsed;
                    editPanel.Visibility = Visibility.Visible;

                    editTextBox.Focus();
                    editTextBox.SelectAll();
                }
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveAlbumCommand?.CanExecute(sender as DependencyObject) == true)
                SaveAlbumCommand.Execute((sender as System.Windows.Controls.Button).DataContext);
            ExitEditMode(sender as DependencyObject);
        }

        private void EditTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExitEditMode(sender as DependencyObject);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CancelEdit(sender as DependencyObject);
                e.Handled = true;
            }
        }

        private void ExitEditMode(DependencyObject element)
        {
            var stackPanel = FindVisualParent<StackPanel>(element);
            if (stackPanel == null) return;

            var textBlock = stackPanel.FindName("AlbumNameText") as TextBlock;
            var editButton = stackPanel.FindName("EditButton") as Button;
            var editPanel = stackPanel.FindName("EditPanel") as StackPanel;

            if (textBlock != null && editButton != null && editPanel != null)
            {
                textBlock.Visibility = Visibility.Visible;
                editButton.Visibility = Visibility.Visible;
                editPanel.Visibility = Visibility.Collapsed;
            }
        }
        private void CancelEdit(DependencyObject element)
        {
            var stackPanel = FindVisualParent<StackPanel>(element);
            if (stackPanel == null) return;

            var editPanel = stackPanel.FindName("EditPanel") as StackPanel;
            var editTextBox = editPanel?.FindName("EditTextBox") as TextBox;

            if (editTextBox != null)
            {
                editTextBox.Text = _originalName;
            }

            ExitEditMode(element);
        }

        private string _originalName;
    }
}
