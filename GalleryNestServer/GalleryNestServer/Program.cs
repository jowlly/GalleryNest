using GalleryNestServer;
using GalleryNestServer.Data;
using GalleryNestServer.Entities;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup();
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, app.Environment);
InitializeDataSources(app);
app.Run();

void InitializeDataSources(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<EntityRepository<Album>>().Set(
    [
        new Album(){ Id=0,Name="Default"}
    ]);
}