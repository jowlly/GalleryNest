using GalleryNestApp.Model;
using GalleryNestApp.Service;
using GalleryNestApp.View;
using GalleryNestApp.ViewModel.Core;
using GalleryNestServer.Entities;
using Microsoft.Web.WebView2.Wpf;
using NuGet.Packaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using Wpf.Ui.Input;
using Album = GalleryNestApp.Model.Album;
using Person = GalleryNestApp.Model.Person;
using Photo = GalleryNestApp.Model.Photo;
using Selection = GalleryNestApp.Model.Selection;

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
        private ObservableCollection<Photo> _selectedPhotos = [];
        private ObservableCollection<Album> _albums = [];
        private ObservableCollection<Selection> _categories = [];
        private ObservableCollection<Person> _persons = [];
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
        public ObservableCollection<Photo> SelectedPhotos
        {
            get => _selectedPhotos;
            set
            {
                _selectedPhotos = value;
                OnPropertyChanged(nameof(SelectedPhotos));
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

        public PhotoViewModel(PhotoService photoService, AlbumService albumService, SelectionService selectionService, PersonService personService, INavigationService navigationService)
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
        private void Photo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Photo.IsSelected))
            {
                var photo = (Photo)sender;
                if (photo.IsSelected)
                {
                    if (!SelectedPhotos.Contains(photo))
                        SelectedPhotos.Add(photo);
                }
                else
                {
                    if (SelectedPhotos.Contains(photo))
                        SelectedPhotos.Remove(photo);
                }
            }
        }
        private async Task LoadDataAsync(bool reset = false, int pageSize = PageSize)
        {
            if (IsLoading) return;
            IsLoading = true;

            Albums = [.. (await AlbumService.GetAllAsync()).ToList()];
            Categories = [.. (await CategoriesService.GetAllAsync()).ToList()];
            People = [.. (await PersonService.GetAllAsync()).ToList()];
            try
            {
                if (reset)
                {
                    Photos.Clear();
                    CurrentPage = 1;
                }

                var pagedResult = await PhotoService.GetPagedAsync(CurrentPage, pageSize);
                var newPhotos = pagedResult.Except(Photos).ToList();
                foreach (var photo in newPhotos)
                {
                    photo.PropertyChanged += Photo_PropertyChanged;
                }

                Photos.AddRange(newPhotos);
                //PhotoIds = [.. Photos.Select(x => x.Id).ToList()];

                TotalPages = await PhotoService.GetTotalPagesAsync(pageSize);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateDataAsync()
        {
            if (IsLoading) return;
            IsLoading = true;

            Albums = [.. (await AlbumService.GetAllAsync()).ToList()];
            Categories = [.. (await CategoriesService.GetAllAsync()).ToList()];
            People = [.. (await PersonService.GetAllAsync()).ToList()];
            try
            {
                var pagedResult = await PhotoService.GetByIds(PhotoIds);
                Photos = [.. pagedResult];
                PhotoIds = [.. Photos.Select(x => x.Id).ToList()];
                GroupedPhotos.Refresh();
                TotalPages = await PhotoService.GetTotalPagesAsync(PageSize);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdatePhotos(List<int> photoIds)
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                var result = await PhotoService.GetByIds(photoIds);
                if (result.Count() <= 0) Photos.RemoveAt(Photos.ToList().FindIndex(x => photoIds.Contains(x.Id)));
                else
                {
                    result.ToList().ForEach(edit => Photos[Photos.ToList().FindIndex(x => x.Id == edit.Id)] = edit);
                    var deleted = photoIds.Except(result.Select(x => x.Id));
                    deleted.ToList().ForEach(x => Photos.RemoveAt(x));
                }
                GroupedPhotos.Refresh();
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
        public ICommand UpdatePhotosCommand => new RelayCommand(async _ =>
        {
            UpdateDataAsync();
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

            UpdateDataAsync();
        }
        );


        private RelayCommand? addToAlbumCommand = null;
        public RelayCommand AddToAlbumCommand => addToAlbumCommand ??= new RelayCommand(async obj =>
        {
            var albumId = (obj as Album).Id;
            var toEdit = SelectedPhotos.ToList();
            toEdit.ForEach(x => x.AlbumIds.Add(albumId));

            await PhotoService.EditAsync(toEdit);
            foreach (var photo in toEdit)
            {
                if (SelectedPhotos.Contains(photo))
                    SelectedPhotos.Remove(photo);
            }
            UpdatePhotos(toEdit.Select(x => x.Id).ToList());

        }
        );
        private RelayCommand? addToCategoryCommand = null;
        public RelayCommand AddToCategoryCommand => addToCategoryCommand ??= new RelayCommand(async obj =>
        {
            var categoryId = (obj as Selection).Id;

            var toEdit = SelectedPhotos.ToList();
            toEdit.ForEach(x => x.SelectionIds.Add(categoryId));

            await PhotoService.EditAsync(toEdit);
            foreach (var photo in toEdit)
            {
                if (SelectedPhotos.Contains(photo))
                    SelectedPhotos.Remove(photo);
            }
            UpdatePhotos(toEdit.Select(x => x.Id).ToList());
        }
        );
        private RelayCommand? addToFavoritesCommand = null;
        public RelayCommand AddToFavoritesCommand => addToFavoritesCommand ??= new RelayCommand(async obj =>
        {

            var toEdit = SelectedPhotos.ToList();
            toEdit.ForEach(x => x.IsFavourite=!x.IsFavourite);

            await PhotoService.EditAsync(toEdit);
            foreach (var photo in toEdit)
            {
                if (SelectedPhotos.Contains(photo))
                    SelectedPhotos.Remove(photo);
            }
            UpdatePhotos(toEdit.Select(x => x.Id).ToList());
        }
        );

        private RelayCommand? addToPersonCommand = null;
        public RelayCommand AddToPersonCommand => addToPersonCommand ??= new RelayCommand(async obj =>
        {
            var personId = (obj as Person).Guid;

            var toEdit = SelectedPhotos.ToList();
            toEdit.ForEach(x => x.PersonIds.Add(personId));

            await PhotoService.EditAsync(toEdit);
            foreach (var photo in toEdit)
            {
                if (SelectedPhotos.Contains(photo))
                    SelectedPhotos.Remove(photo);
            }
            UpdatePhotos(toEdit.Select(x => x.Id).ToList());
        }
        );


        private RelayCommand? editPhotoCommand = null;
        public RelayCommand EditPhotoCommand => editPhotoCommand ??= new RelayCommand(async obj =>
        {
            await PhotoService.EditAsync(
                        [new Photo()
                        {
                            Id = 0,
                        }]);
            UpdateDataAsync();
        }
        );

        private RelayCommand? deletePhotoCommand = null;
        public RelayCommand DeletePhotoCommand => deletePhotoCommand ??= new RelayCommand(async _ =>
        {
            var toDelete = SelectedPhotos.ToList();
            await PhotoService.DeleteAsync(toDelete.Select(x=>x.Id).ToList());
            foreach (var photo in toDelete)
            {
                if (SelectedPhotos.Contains(photo))
                    SelectedPhotos.Remove(photo);
            }
            UpdatePhotos(toDelete.Select(x=>x.Id).ToList());
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
                PhotoIds.Add(await PhotoService.UploadFile(fileName, [1]));
            }
            UpdateDataAsync();
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
