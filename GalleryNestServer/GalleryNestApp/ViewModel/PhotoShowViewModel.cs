using GalleryNestApp.Service;
using GalleryNestApp.ViewModel.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpf.Ui.Input;

namespace GalleryNestApp.ViewModel
{
    public class PhotoShowViewModel:ObservableObject,IParameterReceiver
    {
        private int _photoId;

        public int PhotoId { get => _photoId; set => _photoId = value; }

        public void ReceiveParameter(object parameter)
        {
            if (parameter is int photoId)
            {
                PhotoId = photoId;

            }
        }

        private PhotoService _photoService;

        public PhotoService PhotoService { get => _photoService; private set => _photoService = value; }

        public PhotoShowViewModel(PhotoService photoService)
        {
            _photoService = photoService;
        }

        public ICommand LoadImageCommand => new RelayCommand<object>(param =>
        {
            if (param != null && (param is WebView2CompositionControl))
            {
                LoadImageToWebView((param as WebView2CompositionControl)!, _photoId.ToString());
            }
        });
        public void LoadImageToWebView(WebView2CompositionControl webView, string photoId)
        {
            PhotoService.LoadImageToWebView(webView, photoId,true);
        }
    }
}
