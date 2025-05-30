﻿using GalleryNestApp.Service;
using GalleryNestApp.View;
using GalleryNestApp.ViewModel.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Wpf.Ui.Input;
using Album = GalleryNestApp.Model.Album;

namespace GalleryNestApp.ViewModel
{
    public class AlbumViewModel : ObservableObject
    {
        #region Fields
        private AlbumService _albumService;
        private ObservableCollection<Album> _selectedAlbums= [];
        private ObservableCollection<Album> _albums = [];
        private ObservableCollection<int> _photoIds = [];
        private string _albumName = string.Empty;
        private Album? _selectedAlbum = null;
        private PhotoService _photoService;
        private readonly INavigationService _navigationService;

        public PhotoService PhotoService { get => _photoService; private set => _photoService = value; }
        private const int PageSize = 9;
        private int _currentPage = 1;
        private int _totalPages = 10;
        private bool _isLoading = false;
        #endregion

        #region Properties
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged(nameof(TotalPages));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
        public ObservableCollection<Album> Albums
        {
            get => _albums;
            set
            {
                _albums = value;
                OnPropertyChanged(nameof(Albums));
            }
        }
        public ObservableCollection<Album> SelectedAlbums
        {
            get => _selectedAlbums;
            set
            {
                _selectedAlbums = value;
                OnPropertyChanged(nameof(SelectedAlbums));
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

        public AlbumViewModel(AlbumService albumService, PhotoService photoService, INavigationService navigationService)
        {
            _albumService = albumService;
            _photoService = photoService;
            _navigationService = navigationService;
            LoadDataAsync();
        }

        private void Album_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Album.IsSelected))
            {
                var album = (Album)sender;
                if (album.IsSelected)
                {
                    if (!SelectedAlbums.Contains(album))
                        SelectedAlbums.Add(album);
                }
                else
                {
                    if (SelectedAlbums.Contains(album))
                        SelectedAlbums.Remove(album);
                }
            }
        }

        private async Task LoadDataAsync(bool reset = false, int pageSize = PageSize)
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                if (reset) CurrentPage = 1;

                var pagedResult = await _albumService.GetPagedAsync(CurrentPage, pageSize);

                if (reset) Albums.Clear();
                foreach (var album in from album in pagedResult
                                      where !Albums.Select(x => x.Id).ToList().Contains(album.Id)
                                      select album)
                {
                    album.PropertyChanged += Album_PropertyChanged;
                    Albums.Add(album);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region Commands
        public ICommand LoadNextPageCommand => new RelayCommand(async _ =>
        {
            if (CurrentPage < TotalPages && !IsLoading)
            {
                CurrentPage++;
                await LoadDataAsync();
            }
        });

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

        public ICommand OpenAlbumCommand => new RelayCommand<object>(param =>
        {
            if (param is Album)
                _navigationService.NavigateTo<AlbumGalleryPage>((param as Album)!);
        });

        private RelayCommand? undoSelectionCommand = null;
        public RelayCommand UndoSelectionCommand => undoSelectionCommand ??= new RelayCommand(async obj =>
        {
            var selectedCopy = SelectedAlbums.ToList();

            foreach (var photo in selectedCopy)
            {
                photo.IsSelected = false;
            }

            SelectedAlbums.Clear();
        }
        );
        private RelayCommand? loadAlbumsCommand = null;
        public RelayCommand LoadAlbumsCommand => loadAlbumsCommand ??= new RelayCommand(async obj =>
        {
            await LoadDataAsync(true);
        }
        );


        private RelayCommand? addAlbumCommand = null;
        public RelayCommand AddAlbumCommand => addAlbumCommand ??= new RelayCommand(async obj =>
            {
                await _albumService.AddAsync(new Album()
                {
                    Name = AlbumName
                });
                await LoadDataAsync();
            }
        );

        private RelayCommand? editAlbumCommand = null;
        public RelayCommand EditAlbumCommand => editAlbumCommand ??= new RelayCommand(async obj =>
        {
            await _albumService.EditAsync(
                        [new Album()
                        {
                            Id = 0,
                            Name = AlbumName
                        }]);

            await LoadDataAsync();
        }
        );

        private RelayCommand? saveAlbumCommand = null;
        public RelayCommand SaveAlbumCommand => saveAlbumCommand ??= new RelayCommand(async obj =>
        {
            await _albumService.EditAsync(
                        [obj as Album]);

            await LoadDataAsync();
        }
        );

        private RelayCommand? deleteAlbumCommand = null;
        public RelayCommand DeleteAlbumCommand => deleteAlbumCommand ??= new RelayCommand(async obj =>
        {
            var toDelete = SelectedAlbums.ToList();
            await _albumService.DeleteAsync(toDelete.Select(x => x.Id).ToList());
            foreach (var photo in toDelete)
            {
                if (SelectedAlbums.Contains(photo))
                    SelectedAlbums.Remove(photo);
            }
            LoadDataAsync();
        }
        );

        #endregion

        public void LoadAlbumToWebView(WebView2CompositionControl webView, string albumId)
        {
            PhotoService.LoadAlbumPreviewWebView(webView, albumId);
        }

    }
}
