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

        [HttpGet("meta/exact")]
        public ActionResult<Photo> GetById([FromQuery] int photoId)
        {
            var photo = _photoRepository.GetById(photoId);
            return Ok(photo);
        }
        [HttpGet("meta")]
        public ActionResult<IEnumerable<Photo>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var products = _photoRepository.GetPaged(pageNumber, pageSize);
            return Ok(products);
        }

        [HttpGet("meta/album")]
        public ActionResult<IEnumerable<Photo>> GetByAlbumId([FromQuery] int albumId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var photos = _photoRepository.GetByAlbumId(albumId, pageNumber, pageSize);
            return Ok(photos);
        }
        [HttpGet("meta/latest")]
        public ActionResult<Photo> GetLatestByAlbumId([FromQuery] int albumId)
        {
            var photo = _photoRepository.GetLatestByAlbumId(albumId);
            return Ok(photo);
        }
        [HttpGet("meta/favourite")]
        public ActionResult<Photo> GetByFavourite([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var photos = _photoRepository.GetFavourite(pageNumber, pageSize);
            return Ok(photos);
        }

        [HttpGet("meta/recent")]
        public ActionResult<IEnumerable<Photo>> GetRecent([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var photos = _photoRepository.GetRecent(pageNumber, pageSize);
            return Ok(photos);
        }


        [HttpPost("meta")]
        public IActionResult Set([FromBody] IEnumerable<Photo> entities)
        {
            if (entities.Count() < 0) return BadRequest();
            var albumIds = _albumRepository.GetAll().Select(x => x.Id);

            if (!entities.All(x => albumIds.Contains(x.AlbumId)))
            {
                return BadRequest("Некорректный идентификатор альбома");
            }

            _photoRepository.Set(entities);
            return NoContent();
        }

        [HttpDelete("meta")]
        public IActionResult Delete([FromQuery] IEnumerable<int> ids)
        {
            if (ids.Count() < 0) return BadRequest();
            _photoRepository.Delete(ids);
            return NoContent();
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile file, [FromQuery] int albumId)
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

            _photoRepository.Set([new Photo() { Path = uploadPath, AlbumId = albumId }]);

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

        [HttpGet("download/latest")]
        public IActionResult DownloadLatestAlbumPhoto([FromQuery] int albumId)
        {
            var photo = _photoRepository.GetLatestByAlbumId(albumId);

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
