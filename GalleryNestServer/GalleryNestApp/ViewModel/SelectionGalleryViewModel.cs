﻿using GalleryNestApp.Model;
using GalleryNestApp.Service;
using GalleryNestApp.View;
using GalleryNestApp.ViewModel.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Wpf.Ui.Input;

namespace GalleryNestApp.ViewModel
{
    public class SelectionGalleryViewModel : ObservableObject, IParameterReceiver
    {
        private int _albumId;

        public int SelectionId { get => _albumId; set => _albumId = value; }

        public void ReceiveParameter(object parameter)
        {
            if (parameter is int albumId)
            {
                SelectionId = albumId;
                Task.Run(async () =>
                {
                    await LoadDataAsync();
                });

            }
        }

        #region Fields
        private PhotoService _photoService;
        public PhotoService PhotoService { get => _photoService; private set => _photoService = value; }

        private ObservableCollection<Photo> _photos = [];
        private ObservableCollection<int> _photoIds = [];
        private Photo? _selectedPhoto = null;
        private const int PageSize = 9;
        private readonly INavigationService _navigationService;
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

        public SelectionGalleryViewModel(PhotoService photoService, INavigationService navigationService)
        {
            _photoService = photoService;
            _navigationService = navigationService;
        }

        private async Task LoadDataAsync(bool reset = false, int pageSize = PageSize)
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                if (reset) CurrentPage = 1;

                var pagedResult = await PhotoService.LoadPhotosForSelection(SelectionId, CurrentPage, pageSize);

                if (reset) PhotoIds.Clear();
                foreach (var photo in from photo in pagedResult
                                      where !PhotoIds.Contains(photo.Id)
                                      select photo)
                {
                    PhotoIds.Add(photo.Id);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region Commands

        public ICommand PhotoClickCommand => new RelayCommand<object>(param =>
        {
            if (param is Photo)
                _navigationService.NavigateTo<PhotoShowPage>((param as Photo)!.Id);
        });

        public ICommand LoadNextPageCommand => new RelayCommand(async _ =>
        {
            if (CurrentPage < TotalPages && !IsLoading)
            {
                CurrentPage++;
                await LoadDataAsync();
            }
        });

        private RelayCommand? loadPhotoCommand = null;
        public RelayCommand LoadPhotoCommand => loadPhotoCommand ??= new RelayCommand(async obj =>
        {
            await LoadDataAsync(true);
        }
        );
        public ICommand LoadImageCommand => new RelayCommand<object>(param =>
        {
            if (param != null && (param is WebView2CompositionControl) && (param! as WebView2CompositionControl)!.DataContext is int photoId)
            {
                LoadImageToWebView((param as WebView2CompositionControl)!, photoId.ToString());
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
                await PhotoService.UploadFile(fileName, [SelectionId]);
            }
            await LoadDataAsync();
        }

    }
}