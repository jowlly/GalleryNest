using GalleryNestServer.Data;
using GalleryNestServer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace GalleryNestServer.Controllers
{
    [Route("api/photo")]
    [ApiController]
    public class PhotoController(EntityRepository<Photo> photoRepository, EntityRepository<Album> albumRepository) : ControllerBase
    {
        private readonly EntityRepository<Photo> _photoRepository = photoRepository;
        private readonly EntityRepository<Album> _albumRepository = albumRepository;
        private const string PhotoStoragePath = "Data\\Images";

        [HttpGet("meta")]
        public ActionResult<IEnumerable<Photo>> GetAll()
        {
            var products = _photoRepository.GetAll();
            return Ok(products);
        }

        [HttpGet("meta/{photoId}")]
        public ActionResult<Photo> GetById([FromQuery] int photoId)
        {
            var product = _photoRepository.GetById(photoId);
            return Ok(product);
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

            var fileExtension = Path.GetExtension(photo.Path);
            var mimeType = "application/octet-stream";

            switch (fileExtension.ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                    mimeType = "image/jpeg";
                    break;
                case ".png":
                    mimeType = "image/png";
                    break;
                case ".gif":
                    mimeType = "image/gif";
                    break;
            }

            var fileBytes = System.IO.File.ReadAllBytes(photo.Path);
            return File(fileBytes, mimeType, $"{photoId}{fileExtension}");
        }

    }
}
