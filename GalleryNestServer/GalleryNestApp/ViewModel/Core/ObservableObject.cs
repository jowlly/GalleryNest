using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GalleryNestApp.ViewModel.Core
{
    public class ObservableObject : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
