using GalleryNestServer.Data;
using GalleryNestServer.Entities;

namespace GalleryNestServer
{
    public class AspNetServer
    {
        private static WebApplication? _app;

        public static void Init()
        {

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = AppContext.BaseDirectory,
                Args = Array.Empty<string>(),

                WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot")
            });

            builder.Services.AddControllers()
                .AddApplicationPart(typeof(AspNetServer).Assembly);

            builder.WebHost.UseUrls("http://0.0.0.0:5285");

            var startup = new Startup();
            startup.ConfigureServices(builder.Services);

            var app = builder.Build();
            startup.Configure(app, app.Environment);
            InitializeDataSources(app);
            app.RunAsync();
        }

        public static async Task StopAsync()
        {
            if (_app != null)
                await _app.StopAsync();
        }

        private static void InitializeDataSources(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<EntityRepository<Album>>().Set(
            [
                new Album(){Id=1,Name="Без альбома"}
            ]);

            scope.ServiceProvider.GetRequiredService<EntityRepository<Selection>>().Set(
                [
                    new Selection(){Id=1,Name="Питомцы"},
                    new Selection(){Id=2,Name="Животные"},
                    new Selection(){Id=3,Name="Природа"},
                    new Selection(){Id=4,Name="Еда"},
                    new Selection(){Id=4,Name="Еда"},
                    new Selection(){Id=5,Name="Город"},
                    new Selection(){Id=6,Name="Спорт"},
                ]);
        }
    }
}
