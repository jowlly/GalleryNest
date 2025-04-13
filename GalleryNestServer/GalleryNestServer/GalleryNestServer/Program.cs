using GalleryNestServer.Data;
using GalleryNestServer.Entities;
using LiteDB;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var connectionString = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Database", "data.db");
var directory = Path.GetDirectoryName(connectionString);
if (directory != null && !Directory.Exists(directory))
{
    Directory.CreateDirectory(directory);
}
var database = new LiteDatabase(connectionString);
builder.Services.AddSingleton(new EntityRepository<Photo>(database,"photos"));
builder.Services.AddSingleton(new EntityRepository<Album>(database,"albums"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseCors("AllowAllOrigins");

app.MapControllers();

app.Run();
