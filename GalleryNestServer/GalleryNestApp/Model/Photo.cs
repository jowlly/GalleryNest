
using GalleryNestApp.ViewModel.Core;

namespace GalleryNestApp.Model
{
    public class Photo : ObservableObject
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public List<int> AlbumIds { get; set; }
        public List<int> SelectionIds { get; set; }
        public List<string> PersonIds { get; set; }
        public bool IsFavourite { get; set; }
        public string Path { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
    }
}