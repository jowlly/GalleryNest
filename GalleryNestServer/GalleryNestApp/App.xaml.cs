using GalleryNestServer;
using System.Windows;

namespace GalleryNestApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AspNetServer.Init();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AspNetServer.StopAsync();
            base.OnExit(e);
        }
    }

}
