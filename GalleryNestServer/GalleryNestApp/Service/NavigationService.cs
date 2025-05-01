using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void NavigateTo<T>() where T : Page
            => NavigateTo(typeof(T));

        public void NavigateTo(Type pageType)
        {
            if (!typeof(Page).IsAssignableFrom(pageType))
                throw new ArgumentException("PageType must inherit from Page");

            var page = _serviceProvider.GetRequiredService(pageType) as Page;
            _frame.Content = page;
        }

        public void GoBack()
        {
            if (_frame.CanGoBack)
                _frame.GoBack();
        }
    }
}
