using GalleryNestApp.Service;
using GalleryNestApp.ViewModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
