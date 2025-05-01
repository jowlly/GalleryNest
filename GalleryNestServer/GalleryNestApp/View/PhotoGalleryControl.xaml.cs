using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections;
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
    public partial class PhotoGalleryControl : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(PhotoGalleryControl));

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register("DeleteCommand", typeof(ICommand), typeof(PhotoGalleryControl));

        public static readonly DependencyProperty LoadImageCommandProperty =
            DependencyProperty.Register("LoadImageCommand", typeof(ICommand), typeof(PhotoGalleryControl));

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
            Loaded += async (s, e) =>
                await CoreWebView2Environment.CreateAsync();
        }

        private async void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            var webView = sender as WebView2CompositionControl;
            if (webView?.CoreWebView2 == null)
                await webView.EnsureCoreWebView2Async();

            if (LoadImageCommand?.CanExecute(webView) == true)
                LoadImageCommand.Execute(webView);
        }
    }
}
