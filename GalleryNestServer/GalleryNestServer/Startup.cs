using GalleryNestServer.Data;
using GalleryNestServer.Entities;
using GalleryNestServer.Repositories;
using GalleryNestServer.Services;
using LiteDB;

namespace GalleryNestServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Database", "data.db");
            var directory = Path.GetDirectoryName(connectionString);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var database = new LiteDatabase(connectionString);
            services.AddSingleton(new PhotoRepository(database, "photos"));
            services.AddSingleton(new EntityRepository<Album>(database, "albums"));
            services.AddSingleton(new PersonRepository(database, "persons"));
            services.AddSingleton(new EntityRepository<Selection>(database, "selections"));

            services.AddSingleton<IFaceService, FaceONNXService>();
            services.AddSingleton<IObjectDetector, YoloCategoryDetector>();

            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    policyBuilder =>
                    {
                        policyBuilder.AllowAnyOrigin()
                                     .AllowAnyMethod()
                                     .AllowAnyHeader();
                    });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseRouting();
            app.UseCors("AllowAllOrigins");
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
