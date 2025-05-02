using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace GalleryNestApp.Service
{
    public class NavigationService : INavigationService
    {
        private readonly Frame _frame;
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(Frame frame, IServiceProvider serviceProvider)
        {
            _frame = frame;
            _serviceProvider = serviceProvider;
        }

        public void NavigateTo<T>(object? parameter = null) where T : Page
            => NavigateTo(typeof(T),parameter);

        public void NavigateTo(Type pageType, object? parameter = null)
        {
            if (!typeof(Page).IsAssignableFrom(pageType))
                throw new ArgumentException("PageType must inherit from Page");

            var page = _serviceProvider.GetRequiredService(pageType) as Page;
            if (page == null) return;
            if (page.DataContext is IParameterReceiver viewModel)
            {
                viewModel.ReceiveParameter(parameter);
            }
            _frame.Content = page;
        }

        public void GoBack()
        {
            if (_frame.CanGoBack)
                _frame.GoBack();
        }
    }
}
