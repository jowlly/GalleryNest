using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace GalleryNestApp.View
{
    public class FavouriteToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool isFavorite && !isFavorite)
                ? new SymbolIcon(SymbolRegular.StarOff16)
                : new SymbolIcon(SymbolRegular.Star16);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
