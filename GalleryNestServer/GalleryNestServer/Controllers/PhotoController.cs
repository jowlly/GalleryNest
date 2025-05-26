using GalleryNestServer.Data;
using GalleryNestServer.DTO;
using GalleryNestServer.Entities;
using GalleryNestServer.Repositories;
using GalleryNestServer.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Drawing;

namespace GalleryNestServer.Controllers
{
    [Route("api/photo")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly PhotoRepository _photoRepository;
        private readonly EntityRepository<Album> _albumRepository;
        private readonly EntityRepository<Selection> _selectionRepository;
        private readonly PersonRepository _personRepository;
        private readonly IWebHostEnvironment _env;
        private readonly IFaceService _faceService;
        private readonly IObjectDetector _detector;
        private string PhotoStoragePath;

        public PhotoController(PhotoRepository photoRepository, EntityRepository<Album> albumRepository, EntityRepository<Selection> selectionRepository, PersonRepository personRepository, IWebHostEnvironment env, IFaceService faceService, IObjectDetector detector)
        {

            _photoRepository = photoRepository;
            _albumRepository = albumRepository;
            _selectionRepository = selectionRepository;
            _personRepository = personRepository;
            _env = env;
            _faceService = faceService;
            _detector = detector;
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

        [HttpGet("meta/count")]
        public ActionResult<IEnumerable<Photo>> GetNumber()
        {
            var products = _photoRepository.GetCount();
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
        [HttpGet("meta/album/latest")]
        public ActionResult<Photo> GetLatestByAlbumId([FromQuery] int albumId)
        {
            var photo = _photoRepository.GetLatestByAlbumId(albumId);
            return Ok(photo);
        }

        [HttpGet("meta/selection")]
        public ActionResult<IEnumerable<Photo>> GetBySelectionId([FromQuery] int selectionId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var photos = _photoRepository.GetBySelectionId(selectionId, pageNumber, pageSize);
            return Ok(photos);
        }
        [HttpGet("meta/selection/latest")]
        public ActionResult<Photo> GetLatestBySelectionId([FromQuery] int selectionId)
        {
            var photo = _photoRepository.GetLatestBySelectionId(selectionId);
            return Ok(photo);
        }

        [HttpGet("meta/person")]
        public ActionResult<IEnumerable<Photo>> GetByPersonId([FromQuery] string personId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var photos = _photoRepository.GetByPersonGuid(personId, pageNumber, pageSize);
            return Ok(photos);
        }
        [HttpGet("meta/person/latest")]
        public ActionResult<Photo> GetLatestByPersonId([FromQuery] string personId)
        {
            var photo = _photoRepository.GetLatestByPersonGuid(personId);
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
            var selectionIds = _selectionRepository.GetAll().Select(x => x.Id);

            if (!entities.All(x => x.AlbumIds.Any(y => albumIds.Contains(y))))
            {
                return BadRequest("Некорректный идентификатор альбома");
            }
            if (!entities.All(x => x.SelectionIds.Any(y => selectionIds.Contains(y))))
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
        public async Task<IActionResult> UploadPhoto(
            [FromForm] IFormFile file,
            [FromForm] DateTime creationTime,
            [FromQuery] IEnumerable<int> albumIds)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (creationTime == default)
            {
                return BadRequest("Creation time is required.");
            }

            string fileName = Path.GetFileName(file.FileName);
            string uploadPath = Path.Combine(PhotoStoragePath, fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(uploadPath)!);

            using (var fileStream = new FileStream(uploadPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            var guid = Guid.NewGuid().ToString();
            _photoRepository.Set(
            [
                new Photo()
                {
                    Guid = guid,
                    Path = uploadPath,
                    AlbumIds = albumIds.ToList(),
                    PersonIds = [],
                    CreationTime = creationTime
                }
            ]);

            Task.Run(async () =>
            {
                try
                {
                    await using (var fileStream = new FileStream(uploadPath, FileMode.Open))
                    {
                        var res = await _detector.DetectCategoriesAsync(fileStream);
                        var result = new ImageClassificationResult
                        {
                            FileName = file.FileName,
                            Categories = res,
                            Success = true
                        };
                        if (result.Categories["Человек"] > 0.5)
                        {
                            var faceResult = new FaceProcessingResult
                            {
                                FileName = file.FileName,
                                Embeddings = new List<float[]>()
                            };

                            using var image = Image.FromStream(fileStream);
                            var embeddings = _faceService.GetFaceEmbeddings(image);

                            if (embeddings?.Count() > 0)
                            {
                                var photo = _photoRepository.GetByGuid(guid);
                                foreach (var embedding in embeddings)
                                {
                                    var normalizedEmbedding = Normalize(embedding);
                                    var person = _personRepository.GetByEmbedding([normalizedEmbedding]);

                                    if (person == null)
                                    {
                                        person = new Person { Guid = Guid.NewGuid().ToString() };
                                        person.AddEmbedding(normalizedEmbedding);
                                        _personRepository.Set([person]);
                                    }
                                    else
                                    {
                                        person.AddEmbedding(normalizedEmbedding);
                                        _personRepository.Set([person]);
                                    }

                                    photo.PersonIds.Add(person.Guid);
                                }
                                _photoRepository.Set([photo]);
                            }
                        }
                        if (result.Categories.Values.All(x => x < 0.5))
                        {
                            res = await _detector.ClassifyCategoriesAsync(fileStream);
                            result.Categories = res;
                        }
                        var selections = _selectionRepository.GetAll();
                        if (selections != null)
                        {
                            var photo = _photoRepository.GetByGuid(guid);
                            photo.SelectionIds = selections.Where(x => result.Categories.ContainsKey(x.Name) && result.Categories[x.Name] >= 0.5).Select(x => x.Id).ToList();
                            _photoRepository.Set([photo]);

                        }
                    }
                }
                catch (Exception ex)
                {
                }
            });

            return Ok(new { FilePath = uploadPath });
        }
        private float[] Normalize(float[] vector)
        {
            float magnitude = MathF.Sqrt(vector.Sum(x => x * x));
            return vector.Select(x => x / magnitude).ToArray();
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

        [HttpGet("download/album/latest")]
        public IActionResult DownloadLatestAlbumPhoto([FromQuery] int albumId)
        {
            var photo = _photoRepository.GetLatestByAlbumId(albumId);

            if (photo == null)
                return NotFound("Photo not found.");

            var mimeType = GetMimeType(photo.Path);

            return PhysicalFile(photo.Path, mimeType);
        }
        [HttpGet("download/selection/latest")]
        public IActionResult DownloadLatestSelectionPhoto([FromQuery] int selectionId)
        {
            var photo = _photoRepository.GetLatestBySelectionId(selectionId);

            if (photo == null)
                return NotFound("Photo not found.");

            var mimeType = GetMimeType(photo.Path);

            return PhysicalFile(photo.Path, mimeType);
        }
        [HttpGet("download/person/latest")]
        public IActionResult DownloadLatestPersonPhoto([FromQuery] string personId)
        {
            var photo = _photoRepository.GetLatestByPersonGuid(personId);

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
                ".mp4" => "video/mp4",
                ".mov" => "video/quicktime",
                ".avi" => "video/x-msvideo",
                ".mkv" => "video/x-matroska",
                _ => "application/octet-stream"
            };

        }
    }
}
