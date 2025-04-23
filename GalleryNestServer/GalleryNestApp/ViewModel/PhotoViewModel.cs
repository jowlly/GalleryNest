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
using Windows.Services.Maps;

namespace GalleryNestApp.ViewModel
{
    public class PhotoViewModel : ObservableObject
    {
        #region Fields
        private PhotoService _photoService;

        private ObservableCollection<Photo> _photos = [];
        private ObservableCollection<int> _photoIds = [];
        private Photo? _selectedPhoto = null; private const int PageSize = 12;
        private int _currentPage = 0;
        private bool _isLoading;

        #endregion

        #region Properties
        public PhotoService PhotoService { get => _photoService; private set => _photoService = value; }
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

        public bool IsLoading { get => _isLoading; }
        public int CurrentPage { get => _currentPage;}
        #endregion

        public PhotoViewModel(PhotoService photoService)
        {
            PhotoService = photoService;
            Photos = [.. Task.Run(() => PhotoService.GetAllAsync()).Result];
            Task.Run(LoadCurrentPage).Wait();
        }

        #region Commands
        private RelayCommand? loadPhotoCommand = null;
        public RelayCommand LoadPhotoCommand => loadPhotoCommand ??= new RelayCommand(obj =>
        {
            Task.Run(async () =>
            {
                Photos = [.. (await PhotoService.GetAllAsync())];
                await LoadCurrentPage();
            }).Wait();

        }
        );

        private RelayCommand? addPhotoCommand = null;
        public RelayCommand AddPhotoCommand => addPhotoCommand ??= new RelayCommand(obj =>
        {
            Task.Run(async () =>
            {
                await PhotoService.AddAsync(new Photo()
                {
                    Id = 0,
                });

                Photos = [.. (await PhotoService.GetAllAsync())];
                await LoadCurrentPage();
            }).Wait();

        }
        );

        private RelayCommand? editPhotoCommand = null;
        public RelayCommand EditPhotoCommand => editPhotoCommand ??= new RelayCommand(obj =>
        {
            Task.Run(async () =>
            {
                await PhotoService.EditAsync(
                        new Photo()
                        {
                            Id = 0,
                        });

                Photos = [.. (await PhotoService.GetAllAsync())];
                await LoadCurrentPage();
            }).Wait();
        }
        );

        private RelayCommand? deletePhotoCommand = null;
        public RelayCommand DeletePhotoCommand => deletePhotoCommand ??= new RelayCommand(async obj =>
        {
            if (obj is int photoId)
            {
                await PhotoService.DeleteAsync((new[] { photoId }).ToList());

                Photos = [.. (await PhotoService.GetAllAsync())];
                await LoadCurrentPage();
            }
        });



        #endregion

        public async Task LoadNextPage()
        {
            if (IsLoading) return;

            _isLoading = true;
            try
            {
                _currentPage++;
                await Task.Run(() =>
                {
                    PhotoIds = [.. Photos.Select(x => x.Id).Skip(CurrentPage * PageSize).Take(PageSize).ToList()];
                });
                
            }
            finally
            {
                _isLoading = false;
            }
        }
        public async Task LoadCurrentPage()
        {
            if (IsLoading) return;

            _isLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    PhotoIds = [.. Photos.Select(x => x.Id).Skip(CurrentPage * PageSize).Take(PageSize).ToList()];
                });
            }
            finally
            {
                _isLoading = false;
            }
        }
        public async Task LoadPrevPage()
        {
            if (IsLoading) return;

            _isLoading = true;
            try
            {
                _currentPage--;
                await Task.Run(() =>
                {
                    PhotoIds = [.. Photos.Select(x => x.Id).Skip(CurrentPage * PageSize).Take(PageSize).ToList()];
                }); 
            }
            finally
            {
                _isLoading = false;
            }
        }


        public void LoadImageToWebView(WebView2CompositionControl webView, string photoId)
        {
            PhotoService.LoadImageToWebView(webView, photoId);
        }

        public async Task UploadFile(List<string> fileNames)
        {
            fileNames.ForEach(async x=>await PhotoService.UploadFile(x));
            PhotoIds = [.. (await PhotoService.GetAllAsync()).Select(x => x.Id)];
        }
    }
}
