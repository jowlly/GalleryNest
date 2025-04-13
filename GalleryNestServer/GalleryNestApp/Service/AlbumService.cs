using GalleryNestApp.Model;
using GalleryNestServer.Entities;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Windows;
using Album = GalleryNestApp.Model.Album;

namespace GalleryNestApp.Service
{
    public class AlbumService(HttpClient client, string url) : EntityService<Album>(client, $"{url}/album")
    {
    }
}