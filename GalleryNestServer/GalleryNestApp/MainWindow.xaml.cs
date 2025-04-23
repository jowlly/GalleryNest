using GalleryNestApp.Model;
using GalleryNestApp.Service;
using GalleryNestApp.View;
using GalleryNestApp.ViewModel;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Windows;

namespace GalleryNestApp
{
    public partial class MainWindow : Window
    {
        private const string baseUrl = "http://localhost:5285/api";
        private static HttpClient httpClient = new HttpClient();
        private PhotoService photoService = new PhotoService(httpClient,baseUrl);
        private DeviceService deviceService = new DeviceService(httpClient, baseUrl);
        private AlbumService albumService = new AlbumService(httpClient, baseUrl);

        private PhotoViewModel? photoViewModel = null;
        private UpdateViewModel? updateViewModel = null;
        private FavouriteViewModel? favouriteViewModel = null;
        private DeviceViewModel? deviceViewModel = null;
        private AlbumViewModel? albumViewModel = null;

        public MainWindow()
        {
            photoViewModel = new PhotoViewModel(photoService);
            updateViewModel = new UpdateViewModel(photoService);
            favouriteViewModel = new FavouriteViewModel(photoService);
            deviceViewModel = new DeviceViewModel(deviceService);
            albumViewModel = new AlbumViewModel(albumService);

        InitializeComponent();
        }

        private async void PhotoButton_Click(object sender, RoutedEventArgs e)
        {
            photoViewModel!.PhotoIds=[..(await photoViewModel.PhotoService.GetAllAsync()).Select(x=>x.Id)];
            Main.Content = new PhotoPage(photoViewModel!);
        }

        private void DevicesButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = new DevicePage(deviceViewModel!);
        }

        private void FavouriteButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = new FavouritesPage(favouriteViewModel!);
        }

        private void AlbumButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = new AlbumPage(albumViewModel!);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = new UpdatesPage(updateViewModel!);
        }
    }
}