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
    /// Логика взаимодействия для AlbumPage.xaml
    /// </summary>
    public partial class AlbumPage : Page
    {
        private AlbumViewModel albumViewModel;

        public AlbumPage()
        {
            InitializeComponent();
        }

        public AlbumPage(AlbumViewModel albumViewModel)
        {
            this.albumViewModel = albumViewModel;
        }
    }
}
