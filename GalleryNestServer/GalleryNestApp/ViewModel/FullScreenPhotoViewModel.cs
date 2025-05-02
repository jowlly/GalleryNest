using GalleryNestApp.Service;
using GalleryNestApp.ViewModel.Core;

namespace GalleryNestApp.ViewModel
{
    public class FullScreenPhotoViewModel : ObservableObject, IParameterReceiver
    {
        public PhotoService photoService;
        public FullScreenPhotoViewModel(PhotoService photoService)
        {
            this.photoService = photoService;
        }
        public void ReceiveParameter(object parameter)
        {
            if (parameter is int albumId)
            {
                AlbumId = albumId;
                LoadPhotos();
            }
        }
    }
}
