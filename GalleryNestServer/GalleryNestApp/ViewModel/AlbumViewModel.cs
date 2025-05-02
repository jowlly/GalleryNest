using GalleryNestApp.Model;
using GalleryNestApp.Service;
using GalleryNestApp.ViewModel.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Wpf.Ui.Input;

namespace GalleryNestApp.ViewModel
{
    public class AlbumViewModel : ObservableObject
    {
        #region Fields
        private AlbumService _albumService;
        private ObservableCollection<Album> _albums = [];
        private ObservableCollection<int> _photoIds = [];
        private string _albumName = string.Empty;
        private Album? _selectedAlbum = null;
        private PhotoService _photoService;
        public PhotoService PhotoService { get => _photoService; private set => _photoService = value; }
        #endregion

        #region Properties
        public ObservableCollection<Album> Albums
        {
            get => _albums;
            set
            {
                _albums = value;
                OnPropertyChanged(nameof(Albums));
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

        public string AlbumName
        {
            get => _albumName;
            set
            {
                _albumName = value;
                OnPropertyChanged(nameof(AlbumName));
            }
        }
        public Album SelectedAlbum
        {
            get => _selectedAlbum ?? new Album();
            set
            {
                _selectedAlbum = value;
                OnPropertyChanged(nameof(SelectedAlbum));
            }
        }
        #endregion

        public AlbumViewModel(AlbumService albumService, PhotoService photoService)
        {
            _albumService = albumService;
            _photoService = photoService;
            Albums = [.. Task.Run(() => _albumService.GetAllAsync()).Result];
        }

        #region Commands
        public ICommand LoadImageCommand => new RelayCommand<object>(param =>
        {
            if (param != null && (param is WebView2CompositionControl) && (param! as WebView2CompositionControl)!.DataContext is Album album)
            {
                LoadImageToWebView((param as WebView2CompositionControl)!, album.Id.ToString());
            }
        });

        public void LoadImageToWebView(WebView2CompositionControl webView, string albumId)
        {
            PhotoService.LoadAlbumPreviewWebView(webView, albumId);
        }

        private RelayCommand? loadAlbumsCommand = null;
        public RelayCommand LoadAlbumsCommand => loadAlbumsCommand ??= new RelayCommand(obj =>
        {
            Task.Run(async () =>
            {
                Albums = [.. await _albumService.GetAllAsync()];
            }).Wait();

        }
        );

        private RelayCommand? loadPhotosForAlbumCommand = null;
        public RelayCommand LoadPhotosForAlbumCommand => loadPhotosForAlbumCommand ??= new RelayCommand(obj =>
        {
            Task.Run(async () =>
            {
                PhotoIds = [.. (await _photoService.LoadPhotosForAlbum(SelectedAlbum.Id)).Select(x => x.Id)];
            }).Wait();

        }
        );

        private RelayCommand? addAlbumCommand = null;
        public RelayCommand AddAlbumCommand => addAlbumCommand ??= new RelayCommand(obj =>
            {
                Task.Run(async () =>
                {
                    await _albumService.AddAsync(new Album()
                    {
                        Name = AlbumName
                    });

                    Albums = [.. await _albumService.GetAllAsync()];
                }).Wait();

            }
        );

        private RelayCommand? editAlbumCommand = null;
        public RelayCommand EditAlbumCommand => editAlbumCommand ??= new RelayCommand(obj =>
        {
            Task.Run(async () =>
            {
                await _albumService.EditAsync(
                        new Album()
                        {
                            Id = 0,
                            Name = AlbumName
                        });

                Albums = [.. await _albumService.GetAllAsync()];
            }).Wait();
        }
        );

        private RelayCommand? deleteAlbumCommand = null;
        public RelayCommand DeleteAlbumCommand => deleteAlbumCommand ??= new RelayCommand(obj =>
        {
            Task.Run(async () =>
            {
                await _albumService.DeleteAsync(
                    [SelectedAlbum.Id]);

                Albums = [.. await _albumService.GetAllAsync()];
            }).Wait();
        }
        );

        #endregion

        public void LoadAlbumToWebView(WebView2CompositionControl webView, string albumId)
        {
            PhotoService.LoadAlbumPreviewWebView(webView, albumId);
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
