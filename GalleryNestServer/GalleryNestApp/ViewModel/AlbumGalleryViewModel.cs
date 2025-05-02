using GalleryNestApp.Service;
using GalleryNestApp.ViewModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalleryNestApp.ViewModel
{
    public class AlbumGalleryViewModel:ObservableObject,IParameterReceiver
    {
        private int _albumId;

        public int AlbumId { get => _albumId; set => _albumId = value; }

        public void ReceiveParameter(object parameter)
        {
            if (parameter is int albumId)
            {
                AlbumId = albumId;
            }
        }
    }
}
