using System.Windows.Controls;

namespace GalleryNestApp.Service
{
    public interface INavigationService
    {
        void NavigateTo<T>() where T : Page;
        void NavigateTo(Type pageType);
        void GoBack();
    }
}
