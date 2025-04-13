using GalleryNestServer.Data;
using GalleryNestServer.Entities;
using GalleryNestServer.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GalleryNestServer.Controllers
{
    [Route("api/photo")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly PhotoRepository _photoRepository;
        private readonly EntityRepository<Album> _albumRepository;
        private readonly IWebHostEnvironment _env;
        private string PhotoStoragePath;

        public PhotoController(PhotoRepository photoRepository, EntityRepository<Album> albumRepository, IWebHostEnvironment env)
        {

            _photoRepository = photoRepository;
            _albumRepository = albumRepository;
            _env = env;
            PhotoStoragePath = Path.Combine(_env.WebRootPath, "images");
        }

        [HttpGet("meta")]
        public ActionResult<IEnumerable<Photo>> GetAll()
        {
            var products = _photoRepository.GetAll();
            return Ok(products);
        }

        [HttpGet("meta/{photoId}")]
        public ActionResult<Photo> GetById([FromQuery] int photoId)
        {
            var photo = _photoRepository.GetById(photoId);
            return Ok(photo);
        }
        [HttpGet("meta/{albumId}")]
        public ActionResult<Photo> GetByAlbumId([FromQuery] int albumId)
        {
            var photo = _photoRepository.GetByAlbumId(albumId);
            return Ok(photo);
        }
        [HttpGet("meta/favourite")]
        public ActionResult<Photo> GetByFavourite()
        {
            var photo = _photoRepository.GetFavourite();
            return Ok(photo);
        }
        [HttpGet("meta/recent")]
        public ActionResult<Photo> GetByAlbumId()
        {
            var photo = _photoRepository.GetRecent();
            return Ok(photo);
        }


        [HttpPost("meta")]
        public IActionResult Set([FromBody] IEnumerable<Photo> entities)
        {
            var albumIds = _albumRepository.GetAll().Select(x => x.Id);

            if (!entities.All(x => albumIds.Contains(x.AlbumId)))
            {
                return BadRequest("Некорректный идентификатор альбома");
            }

            _photoRepository.Set(entities);
            return NoContent();
        }

        [HttpDelete("meta")]
        public IActionResult Delete(IEnumerable<int> ids)
        {
            _photoRepository.Delete(ids);
            return NoContent();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            string fileName = Path.GetFileName(file.FileName);
            string fileExtension = Path.GetExtension(file.FileName);

            string uploadPath = Path.Combine(PhotoStoragePath, fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(uploadPath));

            using (var fileStream = new FileStream(uploadPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            _photoRepository.Set([new Photo() { Path = uploadPath }]);

            return Ok(new { FilePath = uploadPath });
        }

        [HttpGet("download")]
        public IActionResult DownloadPhoto([FromQuery] int photoId)
        {
            var photo = _photoRepository.GetById(photoId);

            if (photo == null)
                return NotFound("Photo not found.");

            var mimeType = GetMimeType(photo.Path);

            return PhysicalFile(photo.Path, mimeType);
        }

        private string GetMimeType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

    }
}
