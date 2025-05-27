using GalleryNestApp.Model;
using GalleryNestApp.Service;
using GalleryNestApp.View;
using GalleryNestApp.ViewModel.Core;
using Microsoft.Web.WebView2.Wpf;
using NuGet.Packaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using Wpf.Ui.Input;
using Photo = GalleryNestApp.Model.Photo;

namespace GalleryNestApp.ViewModel
{
    public class PhotoViewModel : ObservableObject
    {
        #region Fields
        private PhotoService _photoService;
        public PhotoService PhotoService { get => _photoService; private set => _photoService = value; }
        private AlbumService _albumService;
        public AlbumService AlbumService { get => _albumService; private set => _albumService = value; }
        private SelectionService _categoriesService;
        public SelectionService CategoriesService { get => _categoriesService; private set => _categoriesService = value; }
        private PersonService _personService;
        public PersonService PersonService { get => _personService; private set => _personService = value; }

        private ObservableCollection<Photo> _photos = [];
        private ObservableCollection<Album> _albums = [];
        private ObservableCollection<Selection> _categories= [];
        private ObservableCollection<Person> _persons= [];
        private ObservableCollection<int> _photoIds = [];

        private ICollectionView _groupedPhotos;

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
        public ObservableCollection<Album> Albums
        {
            get => _albums;
            set
            {
                _albums = value;
                OnPropertyChanged(nameof(Albums));
            }
        }
        public ObservableCollection<Selection> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }
        public ObservableCollection<Person> People
        {
            get => _persons;
            set
            {
                _persons = value;
                OnPropertyChanged(nameof(People));
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

        public ICollectionView GroupedPhotos
        {
            get => _groupedPhotos;
            set
            {
                _groupedPhotos = value;
                OnPropertyChanged(nameof(GroupedPhotos));
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

        public PhotoViewModel(PhotoService photoService,AlbumService albumService,SelectionService selectionService,PersonService personService, INavigationService navigationService)
        {
            PhotoService = photoService;
            AlbumService = albumService;
            CategoriesService = selectionService;
            PersonService = personService;
            _navigationService = navigationService;
            InitGroups();
            LoadDataAsync();
        }

        private void InitGroups()
        {
            GroupedPhotos = CollectionViewSource.GetDefaultView(Photos);
            GroupedPhotos.GroupDescriptions.Add(new PropertyGroupDescription("CreationTime", new DateTimeToDateConverter()));
            GroupedPhotos.SortDescriptions.Add(new SortDescription("CreationTime", ListSortDirection.Descending));
        }

        private async Task LoadDataAsync(bool reset = false, int pageSize = PageSize)
        {
            if (IsLoading) return;
            IsLoading = true;

            Albums = [..(await AlbumService.GetAllAsync()).ToList()];
            Categories = [..(await CategoriesService.GetAllAsync()).ToList()];
            People = [..(await PersonService.GetAllAsync()).ToList()];
            try
            {
                if (reset)
                {
                    Photos.Clear();
                    CurrentPage = 1;
                }

                var pagedResult = await PhotoService.GetPagedAsync(CurrentPage, pageSize);
                var newPhotos = pagedResult.Except(Photos).ToList();

                Photos.AddRange(newPhotos);

                TotalPages = await PhotoService.GetTotalPagesAsync(pageSize);
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
                LoadDataAsync();
            }
        });

        private RelayCommand? loadPhotoCommand = null;
        public RelayCommand LoadPhotoCommand => loadPhotoCommand ??= new RelayCommand(async obj =>
        {
            LoadDataAsync(true);
        }
        );
        public ICommand LoadImageCommand => new RelayCommand<object>(param =>
        {
            if (param != null && (param is WebView2CompositionControl) && (param! as WebView2CompositionControl)!.DataContext is Photo photo)
            {
                LoadImageToWebView((param as WebView2CompositionControl)!, photo.Id.ToString());
            }
        });
        public ICommand LoadLastAlbumImageCommand => new RelayCommand<object>(param =>
        {
            if (param != null && (param is WebView2CompositionControl))
            {
                PhotoService.LoadAlbumPreviewWebView((param as WebView2CompositionControl)!, Convert.ToString(((param as WebView2CompositionControl)!.DataContext as Album).Id));
            }
        });

        public ICommand LoadLastCategoryImageCommand => new RelayCommand<object>(param =>
        {
            if (param != null && (param is WebView2CompositionControl))
            {
                PhotoService.LoadSelectionPreviewWebView((param as WebView2CompositionControl)!, Convert.ToString(((param as WebView2CompositionControl)!.DataContext as Selection).Id));
            }
        });

        public ICommand LoadLastPersonImageCommand => new RelayCommand<object>(param =>
        {
            if (param != null && (param is WebView2CompositionControl))
            {
                PhotoService.LoadPersonPreviewWebView((param as WebView2CompositionControl)!, Convert.ToString(((param as WebView2CompositionControl)!.DataContext as Person).Guid));
            }
        });


        private RelayCommand? addPhotoCommand = null;
        public RelayCommand AddPhotoCommand => addPhotoCommand ??= new RelayCommand(async obj =>
        {
            await PhotoService.AddAsync(new Photo()
            {
                Id = 0,
            });

            LoadDataAsync();
        }
        );


        private RelayCommand? addToAlbumCommand = null;
        public RelayCommand AddToAlbumCommand => addToAlbumCommand ??= new RelayCommand(async obj =>
        {
            var albumId = (obj as Album).Id;
            //await PhotoService.EditAsync((obj as Photo));

            LoadDataAsync();
        }
        );
        private RelayCommand? addToCategoryCommand = null;
        public RelayCommand AddToCategoryCommand => addToCategoryCommand ??= new RelayCommand(async obj =>
        {
            var categoryId = (obj as Selection).Id;
            //await PhotoService.EditAsync((obj as Photo));

            LoadDataAsync();
        }
        );
        private RelayCommand? addToFavoritesCommand = null;
        public RelayCommand AddToFavoritesCommand => addToFavoritesCommand ??= new RelayCommand(async obj =>
        {
            (obj as Photo).IsFavourite = !(obj as Photo).IsFavourite;
            await PhotoService.EditAsync((obj as Photo));

            LoadDataAsync();
        }
        );

        private RelayCommand? addToPersonCommand = null;
        public RelayCommand AddToPersonCommand => addToPersonCommand ??= new RelayCommand(async obj =>
        {
            var personId = (obj as Selection).Id;
            //await PhotoService.EditAsync((obj as Photo));

            LoadDataAsync();
        }
        );


        private RelayCommand? editPhotoCommand = null;
        public RelayCommand EditPhotoCommand => editPhotoCommand ??= new RelayCommand(async obj =>
        {
            await PhotoService.EditAsync(
                        new Photo()
                        {
                            Id = 0,
                        });
            LoadDataAsync();
        }
        );

        private RelayCommand? deletePhotoCommand = null;
        public RelayCommand DeletePhotoCommand => deletePhotoCommand ??= new RelayCommand(async obj =>
        {
            if (obj is Photo photo)
            {
                await PhotoService.DeleteAsync((new[] { photo.Id }).ToList());

                LoadDataAsync();
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
                await PhotoService.UploadFile(fileName, [1]);
            }
            LoadDataAsync();
        }
    }
    public class DateTimeToDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                return date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
