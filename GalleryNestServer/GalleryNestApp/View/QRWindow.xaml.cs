using System.Windows;

namespace GalleryNestApp
{
    /// <summary>
    /// Логика взаимодействия для QRWindow.xaml
    /// </summary>
    public partial class QRWindow : Window
    {
        public QRWindow()
        {
            InitializeComponent();
            Loaded += QRWindow_Loaded;
        }

        private async void QRWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await QRWebView.EnsureCoreWebView2Async();
        }

        public async Task LoadQRAsync(string svgContent)
        {
            await QRWebView.EnsureCoreWebView2Async();
            QRWebView.NavigateToString(svgContent);
        }
    }
}
