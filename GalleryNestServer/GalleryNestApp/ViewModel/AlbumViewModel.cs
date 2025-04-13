using GalleryNestApp.Model;
using GalleryNestApp.Service;
using GalleryNestApp.ViewModel.Core;
using System.Collections.ObjectModel;

namespace GalleryNestApp.ViewModel
{
    public class AlbumViewModel : ObservableObject
    {
        #region Fields
        private AlbumService _albumService;
        private ObservableCollection<Album> _albums = [];
        private string _albumName = string.Empty;
        private Album? _selectedAlbum = null;
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
            get => _selectedAlbum??new Album();
            set
            {
                _selectedAlbum = value;
                OnPropertyChanged(nameof(SelectedAlbum));
            }
        }
        #endregion

        public AlbumViewModel(AlbumService albumService)
        {
            _albumService = albumService;
            Albums = [.. Task.Run(() => _albumService.GetAllAsync()).Result];
        }

        #region Commands
        private RelayCommand? addAlbumCommand = null;
        public RelayCommand AddAlbumCommand => addAlbumCommand ??= new RelayCommand(obj =>
            {
                Task.Run(async () =>
                {
                    await _albumService.AddAsync(new Album()
                    {
                        Id = 0,
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

    }
}
