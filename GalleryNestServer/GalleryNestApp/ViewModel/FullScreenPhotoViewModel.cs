using GalleryNestApp.Service;
using GalleryNestApp.ViewModel.Core;

namespace GalleryNestApp.ViewModel
{
    public class FullScreenPhotoViewModel : ObservableObject
    {
        public PhotoService photoService;
        public FullScreenPhotoViewModel(PhotoService photoService)
        {
            this.photoService = photoService;
        }

    }
}
