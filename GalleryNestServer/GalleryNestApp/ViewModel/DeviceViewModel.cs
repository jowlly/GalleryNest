using GalleryNestApp.Service;

namespace GalleryNestApp.ViewModel
{
    public class DeviceViewModel
    {
        private DeviceService deviceService;

        public DeviceViewModel(DeviceService deviceService)
        {
            this.deviceService = deviceService;
        }
    }
}