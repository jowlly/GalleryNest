using GalleryNestApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GalleryNestApp.View
{
    /// <summary>
    /// Логика взаимодействия для AlbumGalleryPage.xaml
    /// </summary>
    public partial class AlbumGalleryPage : Page
    {
        private AlbumGalleryViewModel albumGalleryViewModel;
        public AlbumGalleryPage(AlbumGalleryViewModel viewModel)
        {
            this.albumGalleryViewModel = viewModel;
            InitializeComponent();
            DataContext = this.albumViewModel;
            InitializeAsync();
        }
    }
}
