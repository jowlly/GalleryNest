using GalleryNestApp.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace GalleryNestApp.View
{
    /// <summary>
    /// Логика взаимодействия для DevicePage.xaml
    /// </summary>
    public partial class DevicePage : Page
    {
        private DeviceViewModel deviceViewModel;

        public DevicePage(DeviceViewModel deviceViewModel)
        {
            InitializeComponent();
            this.deviceViewModel = deviceViewModel;
            DataContext = this.deviceViewModel;
        }


        private async void QRWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await QRWebView.EnsureCoreWebView2Async();
            await LoadQRAsync(await deviceViewModel.deviceService.LoadQR());
        }

        public async Task LoadQRAsync(string svgContent)
        {
            await QRWebView.EnsureCoreWebView2Async();
            QRWebView.NavigateToString(svgContent);
        }
    }
}
