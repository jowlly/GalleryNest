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
    public class PersonViewModel : ObservableObject
    {
        #region Fields
        private PersonService _personService;
        private ObservableCollection<Person> _persons = [];
        private ObservableCollection<int> _photoIds = [];
        private string _selectionName = string.Empty;
        private Person? _selectedPerson = null;
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
        public ObservableCollection<Person> Persons
        {
            get => _persons;
            set
            {
                _persons = value;
                OnPropertyChanged(nameof(Persons));
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

        public string PersonName
        {
            get => _selectionName;
            set
            {
                _selectionName = value;
                OnPropertyChanged(nameof(PersonName));
            }
        }
        public Person SelectedPerson
        {
            get => _selectedPerson ?? new Person();
            set
            {
                _selectedPerson = value;
                OnPropertyChanged(nameof(SelectedPerson));
            }
        }
        #endregion

        public PersonViewModel(PersonService albumService, PhotoService photoService, INavigationService navigationService)
        {
            _personService = albumService;
            _photoService = photoService;
            _navigationService = navigationService;
            LoadDataAsync();
        }

        private async Task LoadDataAsync(bool reset = false, int pageSize = PageSize)
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                if (reset) CurrentPage = 1;

                var pagedResult = await _personService.GetPagedAsync(CurrentPage, pageSize);

                if (reset) Persons.Clear();
                foreach (var album in from album in pagedResult
                                      where !Persons.Select(x => x.Id).ToList().Contains(album.Id)
                                      select album)
                {
                    Persons.Add(album);
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
            if (param != null && (param is WebView2CompositionControl) && (param! as WebView2CompositionControl)!.DataContext is Person album)
            {
                LoadImageToWebView((param as WebView2CompositionControl)!, album.Guid.ToString());
            }
        });

        public void LoadImageToWebView(WebView2CompositionControl webView, string selectionId)
        {
            PhotoService.LoadPersonPreviewWebView(webView, selectionId);
        }

        public ICommand OpenPersonCommand => new RelayCommand<object>(param =>
        {
            if (param is Person)
                _navigationService.NavigateTo<PersonGalleryPage>((param as Person)!.Guid);
        });


        private RelayCommand? loadPersonCommand = null;
        public RelayCommand LoadPersonCommand => loadPersonCommand ??= new RelayCommand(async obj =>
        {
            await LoadDataAsync(true);
        }
        );


        private RelayCommand? deletePersonCommand = null;
        public RelayCommand DeletePersonCommand => deletePersonCommand ??= new RelayCommand(async obj =>
        {
            await _personService.DeleteAsync(
                    [SelectedPerson.Id]);

            await LoadDataAsync();
        }
        );

        #endregion

        public void LoadPersonToWebView(WebView2CompositionControl webView, string personId)
        {
            PhotoService.LoadPersonPreviewWebView(webView, personId);
        }

    }
}