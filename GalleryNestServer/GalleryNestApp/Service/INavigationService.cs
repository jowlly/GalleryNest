using System.Windows.Controls;

namespace GalleryNestApp.Service
{
    public interface INavigationService
    {
        void NavigateTo<T>(object? parameter = null) where T : Page;
        void NavigateTo(Type pageType, object parameter);
        void GoBack();
    }
}
