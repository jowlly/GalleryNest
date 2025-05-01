using GalleryNestApp.Model;
using GalleryNestApp.Service;
using GalleryNestApp.ViewModel.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Wpf.Ui.Input;

namespace GalleryNestApp.ViewModel
{
    public class PhotoViewModel : ObservableObject
    {
        #region Fields
        private PhotoService _photoService;
        public PhotoService PhotoService { get => _photoService; private set => _photoService = value; }

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
        private RelayCommand? loadDataCommand;
        #endregion

        public PhotoViewModel(PhotoService photoService)
        {
            PhotoService = photoService;
            loadDataCommand = new RelayCommand(async _ => await LoadDataAsync());
            loadDataCommand.Execute(null);
        }

        private async Task LoadDataAsync()
        {
            var photos = await PhotoService.GetAllAsync();
            PhotoIds = [.. photos.Select(x => x.Id)];
        }

        #region Commands
        private RelayCommand? loadPhotoCommand = null;
        public RelayCommand LoadPhotoCommand => loadPhotoCommand ??= new RelayCommand(obj =>
        {
            Task.Run(async () =>
            {
                PhotoIds = [.. (await PhotoService.GetAllAsync()).Select(x => x.Id)];
            }).Wait();


        }
        );
        public ICommand LoadImageCommand => new RelayCommand<object>(param =>
        {
            if (param != null && (param is WebView2CompositionControl) && (param! as WebView2CompositionControl)!.DataContext is int photoId)
            {
                LoadImageToWebView((param as WebView2CompositionControl)!, photoId.ToString());
            }
        });
        private RelayCommand? addPhotoCommand = null;
        public RelayCommand AddPhotoCommand => addPhotoCommand ??= new RelayCommand(obj =>
        {
            Task.Run(async () =>
            {
                await PhotoService.AddAsync(new Photo()
                {
                    Id = 0,
                    AlbumId = 1
                });

                PhotoIds = [.. (await PhotoService.GetAllAsync()).Select(x => x.Id)];
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

                PhotoIds = [.. (await PhotoService.GetAllAsync()).Select(x => x.Id)];
            }).Wait();
        }
        );

        private RelayCommand? deletePhotoCommand = null;
        public RelayCommand DeletePhotoCommand => deletePhotoCommand ??= new RelayCommand(async obj =>
        {
            if (obj is int photoId)
            {
                await PhotoService.DeleteAsync((new[] { photoId }).ToList());

                PhotoIds = [.. (await PhotoService.GetAllAsync()).Select(x => x.Id)];
            }
        });

        #endregion



        public void LoadImageToWebView(WebView2CompositionControl webView, string photoId)
        {
            PhotoService.LoadImageToWebView(webView, photoId);
        }

        public async Task UploadFile(List<string> fileNames)
        {
            foreach (var fileName in fileNames)
            {
                await PhotoService.UploadFile(fileName);
            }
            var updatedPhotos = await PhotoService.GetAllAsync();

            PhotoIds = [.. (await PhotoService.GetAllAsync()).Select(x => x.Id)];
        }
    }
}
