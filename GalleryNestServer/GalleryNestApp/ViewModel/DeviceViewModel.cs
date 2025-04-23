using GalleryNestApp.Service;
using GalleryNestApp.ViewModel.Core;

namespace GalleryNestApp.ViewModel
{
    public class DeviceViewModel : ObservableObject
    {
        public DeviceService deviceService;
        public DeviceViewModel(DeviceService deviceService)
        {
            this.deviceService = deviceService;
        }

    }
}