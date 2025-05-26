using GalleryNestApp.Model;
using GalleryNestApp.Service;
using GalleryNestApp.View;
using GalleryNestApp.ViewModel.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Wpf.Ui.Input;

namespace GalleryNestApp.ViewModel
{
    public class SelectionsViewModel : ObservableObject
    {
        #region Fields
        private SelectionService _albumService;
        private ObservableCollection<Selection> _selections = [];
        private ObservableCollection<int> _photoIds = [];
        private string _selectionName = string.Empty;
        private Selection? _selectedSelection = null;
        private PhotoService _photoService;
        private readonly INavigationService _navigationService;

        public PhotoService PhotoService { get => _photoService; private set => _photoService = value; }
        private const int PageSize = 3;
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
        public ObservableCollection<Selection> Selections
        {
            get => _selections;
            set
            {
                _selections = value;
                OnPropertyChanged(nameof(Selections));
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

        public string SelectionName
        {
            get => _selectionName;
            set
            {
                _selectionName = value;
                OnPropertyChanged(nameof(SelectionName));
            }
        }
        public Selection SelectedSelection
        {
            get => _selectedSelection ?? new Selection();
            set
            {
                _selectedSelection = value;
                OnPropertyChanged(nameof(SelectedSelection));
            }
        }
        #endregion

        public SelectionsViewModel(SelectionService albumService, PhotoService photoService, INavigationService navigationService)
        {
            _albumService = albumService;
            _photoService = photoService;
            _navigationService = navigationService;
            Task.Run(async () =>
            {
                await LoadDataAsync(pageSize: 9);
                CurrentPage = 3;
            });
        }

        private async Task LoadDataAsync(bool reset = false, int pageSize = PageSize)
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                if (reset) CurrentPage = 1;

                var pagedResult = await _albumService.GetPagedAsync(CurrentPage, pageSize);

                if (reset) Selections.Clear();
                foreach (var album in from album in pagedResult
                                      where !Selections.Select(x => x.Id).ToList().Contains(album.Id)
                                      select album)
                {
                    Selections.Add(album);
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
            if (param != null && (param is WebView2CompositionControl) && (param! as WebView2CompositionControl)!.DataContext is Selection album)
            {
                LoadImageToWebView((param as WebView2CompositionControl)!, album.Id.ToString());
            }
        });

        public void LoadImageToWebView(WebView2CompositionControl webView, string selectionId)
        {
            PhotoService.LoadSelectionPreviewWebView(webView, selectionId);
        }

        public ICommand OpenSelectionCommand => new RelayCommand<object>(param =>
        {
            if (param is Selection)
                _navigationService.NavigateTo<SelectionGalleryPage>((param as Selection)!.Id);
        });


        private RelayCommand? loadSelectionsCommand = null;
        public RelayCommand LoadSelectionsCommand => loadSelectionsCommand ??= new RelayCommand(async obj =>
        {
            await LoadDataAsync(true);
        }
        );


        private RelayCommand? addSelectionCommand = null;
        public RelayCommand AddSelectionCommand => addSelectionCommand ??= new RelayCommand(async obj =>
        {
            await _albumService.AddAsync(new Selection()
            {
                Name = SelectionName
            });
            await LoadDataAsync();
        }
        );

        private RelayCommand? editSelectionCommand = null;
        public RelayCommand EditSelectionCommand => editSelectionCommand ??= new RelayCommand(async obj =>
        {
            await _albumService.EditAsync(
                        new Selection()
                        {
                            Id = 0,
                            Name = SelectionName
                        });

            await LoadDataAsync();
        }
        );

        private RelayCommand? deleteSelectionCommand = null;
        public RelayCommand DeleteSelectionCommand => deleteSelectionCommand ??= new RelayCommand(async obj =>
        {
            await _albumService.DeleteAsync(
                    [SelectedSelection.Id]);

            await LoadDataAsync();
        }
        );

        #endregion

        public void LoadSelectionToWebView(WebView2CompositionControl webView, string albumId)
        {
            PhotoService.LoadSelectionPreviewWebView(webView, albumId);
        }

    }
}