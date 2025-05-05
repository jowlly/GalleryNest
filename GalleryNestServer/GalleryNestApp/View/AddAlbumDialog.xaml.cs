using System.Windows;

namespace GalleryNestApp.View
{
    public partial class AddAlbumDialog : Window
    {
        public string AlbumName { get; private set; }

        public AddAlbumDialog()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AlbumName = AlbumNameTextBox.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
