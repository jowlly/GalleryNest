using GalleryNestApp.Model;
using GalleryNestApp.Service;
using GalleryNestApp.ViewModel.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GalleryNestApp.ViewModel
{
    public class PhotoViewModel : ObservableObject
    {
        #region Fields
        private PhotoService _photoService;
        private ObservableCollection<Photo> _photos = [];
        private ObservableCollection<int> _photoIds = [];
        private Photo? _selectedPhoto = null;
        #endregion

        #region Properties
        public ObservableCollection<Photo> Photos
        {
            get => _photos;
            set
            {
                _photos = value;
                OnPropertyChanged(nameof(Photos));
            }
        }
        public ObservableCollection<int> PhotoIds
        {
            get => _photoIds;
            set
            {
                _photoIds = value;
                OnPropertyChanged(nameof(PhotoIds));
            }
        }

        public Photo SelectedPhoto
        {
            get => _selectedPhoto ?? new Photo();
            set
            {
                _selectedPhoto = value;
                OnPropertyChanged(nameof(SelectedPhoto));
            }
        }
        #endregion

        public PhotoViewModel(PhotoService photoService)
        {
            _photoService = photoService;
            Photos = [.. Task.Run(() => _photoService.GetAllAsync()).Result];
            PhotoIds = [.. Photos.Select(x => x.Id)];
        }

        #region Commands
        private RelayCommand? addPhotoCommand = null;
        public RelayCommand AddPhotoCommand => addPhotoCommand ??= new RelayCommand(obj =>
        {
            Task.Run(async () =>
            {
                await _photoService.AddAsync(new Photo()
                {
                    Id = 0,
                });

                PhotoIds = [.. (await _photoService.GetAllAsync()).Select(x => x.Id)];
            }).Wait();

        }
        );

        private RelayCommand? editPhotoCommand = null;
        public RelayCommand EditPhotoCommand => editPhotoCommand ??= new RelayCommand(obj =>
        {
            Task.Run(async () =>
            {
                await _photoService.EditAsync(
                        new Photo()
                        {
                            Id = 0,
                        });

                PhotoIds = [.. (await _photoService.GetAllAsync()).Select(x => x.Id)];
            }).Wait();
        }
        );

        private RelayCommand? deletePhotoCommand = null;
        public RelayCommand DeletePhotoCommand => deletePhotoCommand ??= new RelayCommand(obj =>
        {
            Task.Run(async () =>
            {
                await _photoService.DeleteAsync(
                    [SelectedPhoto.Id]);

                PhotoIds = [.. (await _photoService.GetAllAsync()).Select(x => x.Id)];
            }).Wait();
        }
        );
        #endregion

        public void LoadImageToWebView(WebView2 webView, string photoId)
        {
            _photoService.LoadImageToWebView(webView, photoId);
        }

        public async Task UploadFile(List<string> fileNames)
        {
            fileNames.ForEach(async x=>await _photoService.UploadFile(x));
            PhotoIds = [.. (await _photoService.GetAllAsync()).Select(x => x.Id)];
        }
    }
}
