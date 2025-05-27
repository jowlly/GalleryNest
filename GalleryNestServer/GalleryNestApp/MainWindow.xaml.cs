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
        private const string BASE_URL = "http://localhost:5285/api";
        private readonly INavigationService navigationService;
        private readonly ServiceProvider _serviceProvider;

        public INavigationService NavigationService => navigationService;

        public MainWindow()
        {
            InitializeComponent();
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            navigationService = _serviceProvider.GetRequiredService<INavigationService>();

            navigationService.NavigateTo<PhotoPage>();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<WebView2Provider>();
            services.AddSingleton<HttpClient>();
            services.AddSingleton(provider =>
                new PhotoService(
                    provider.GetRequiredService<HttpClient>(),
                    BASE_URL
                ));
            services.AddSingleton(provider =>
                new DeviceService(
                    provider.GetRequiredService<HttpClient>(),
                    BASE_URL
                ));
            services.AddSingleton(provider =>
                new AlbumService(
                    provider.GetRequiredService<HttpClient>(),
                    BASE_URL
                ));
            services.AddSingleton(provider =>
                new SelectionService(
                    provider.GetRequiredService<HttpClient>(),
                    BASE_URL
                ));
            services.AddSingleton(provider =>
                new PersonService(
                    provider.GetRequiredService<HttpClient>(),
                    BASE_URL
                ));
            // Регистрация ViewModel
            services.AddTransient<PhotoViewModel>();
            services.AddTransient<AlbumGalleryViewModel>();
            services.AddTransient<DeviceViewModel>();
            services.AddTransient<FavouriteViewModel>();
            services.AddTransient<AlbumViewModel>();
            services.AddTransient<FavouriteViewModel>();
            services.AddTransient<SelectionsViewModel>();
            services.AddTransient<SelectionGalleryViewModel>();
            services.AddTransient<PersonViewModel>();
            services.AddTransient<PersonGalleryViewModel>();
            services.AddTransient<PhotoShowViewModel>();

            // Регистрация страниц
            services.AddTransient<PhotoPage>();
            services.AddTransient<AlbumGalleryPage>();
            services.AddTransient<DevicePage>();
            services.AddTransient<FavouritesPage>();
            services.AddTransient<AlbumPage>();
            services.AddTransient<SelectionsPage>();
            services.AddTransient<SelectionGalleryPage>();
            services.AddTransient<PersonPage>();
            services.AddTransient<PersonGalleryPage>();
            services.AddTransient<PhotoShowPage>();

            // Регистрация NavigationService
            services.AddSingleton<INavigationService>(provider =>
                new NavigationService(Main, provider));
        }

        // Обработчики кнопок навигации
        private void PhotoButton_Click(object sender, RoutedEventArgs e)
            => NavigationService.NavigateTo<PhotoPage>();

        private void DevicesButton_Click(object sender, RoutedEventArgs e)
            => NavigationService.NavigateTo<DevicePage>();

        private void FavouriteButton_Click(object sender, RoutedEventArgs e)
            => NavigationService.NavigateTo<FavouritesPage>();

        private void AlbumButton_Click(object sender, RoutedEventArgs e)
            => NavigationService.NavigateTo<AlbumPage>();

        private void SelectionsButton_Click(object sender, RoutedEventArgs e)
            => NavigationService.NavigateTo<SelectionsPage>();

        private void PersonsButton_Click(object sender, RoutedEventArgs e)
            => NavigationService.NavigateTo<PersonPage>();

    }

}