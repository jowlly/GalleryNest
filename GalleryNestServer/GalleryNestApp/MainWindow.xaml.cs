using GalleryNestApp.Service;
using GalleryNestApp.View;
using GalleryNestApp.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using HttpClient = System.Net.Http.HttpClient;

namespace GalleryNestApp
{
    public partial class MainWindow : Window
    {
        private readonly INavigationService _navigationService;
        private readonly ServiceProvider _serviceProvider;

        public MainWindow()
        {
            InitializeComponent();

            // Конфигурация DI-контейнера
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Получаем сервис навигации
            _navigationService = _serviceProvider.GetRequiredService<INavigationService>();

            // Начальная навигация
            _navigationService.NavigateTo<PhotoPage>();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Регистрация сервисов
            services.AddSingleton<HttpClient>();
            services.AddSingleton(provider =>
                new PhotoService(
                    provider.GetRequiredService<HttpClient>(),
                    "http://localhost:5285/api"
                ));
            services.AddSingleton(provider =>
                new DeviceService(
                    provider.GetRequiredService<HttpClient>(),
                    "http://localhost:5285/api"
                ));
            services.AddSingleton(provider =>
                new AlbumService(
                    provider.GetRequiredService<HttpClient>(),
                    "http://localhost:5285/api"
                ));
            // Регистрация ViewModel
            services.AddTransient<PhotoViewModel>();
            services.AddTransient<FullScreenPhotoViewModel>();
            services.AddTransient<DeviceViewModel>();
            services.AddTransient<AlbumViewModel>();
            services.AddTransient<FavouriteViewModel>();
            services.AddTransient<UpdateViewModel>();

            // Регистрация страниц
            services.AddTransient<PhotoPage>();
            services.AddTransient<FullScreenPhotoPage>();
            services.AddTransient<DevicePage>();
            services.AddTransient<AlbumPage>();
            services.AddTransient<FavouritesPage>();
            services.AddTransient<UpdatesPage>();

            // Регистрация NavigationService
            services.AddSingleton<INavigationService>(provider =>
                new NavigationService(Main, provider));
        }

        // Обработчики кнопок навигации
        private void PhotoButton_Click(object sender, RoutedEventArgs e)
            => _navigationService.NavigateTo<PhotoPage>();

        private void DevicesButton_Click(object sender, RoutedEventArgs e)
            => _navigationService.NavigateTo<DevicePage>();

        private void FavouriteButton_Click(object sender, RoutedEventArgs e)
            => _navigationService.NavigateTo<FavouritesPage>();

        private void AlbumButton_Click(object sender, RoutedEventArgs e)
            => _navigationService.NavigateTo<AlbumPage>();

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
            => _navigationService.NavigateTo<UpdatesPage>();
    }

}