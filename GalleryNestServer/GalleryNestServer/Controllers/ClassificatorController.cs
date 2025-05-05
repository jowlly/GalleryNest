using GalleryNestServer.DTO;
using GalleryNestServer.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Drawing;

namespace GalleryNestServer.Controllers
{
    [ApiController]
    [Route("api/classifier")]
    public class ClassificationController : ControllerBase
    {
        private readonly IFaceService _faceService;
        private readonly IObjectDetector _detector;

        public ClassificationController(IFaceService faceService, IObjectDetector detector)
        {
            _faceService = faceService;
            _detector = detector;
        }

        [HttpPost("detect")]
        public async Task<IActionResult> DetectObjects(IEnumerable<IFormFile> files)
        {
            var results = new ConcurrentBag<ImageClassificationResult>();
            var tasks = files.Select(async file =>
            {
                await using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                var res = await _detector.DetectCategoriesAsync(stream);
                results.Add(new ImageClassificationResult
                {
                    FileName = file.FileName,
                    Categories = res,
                    Success = true
                });
            });

            await Task.WhenAll(tasks);
            return Ok(results);
        }



        [HttpPost("face")]
        public async Task<IActionResult> GetFaceEmbedding(IEnumerable<IFormFile> files)
        {
            var results = new ConcurrentBag<FaceProcessingResult>();
            var tasks = files.Select(async file =>
            {
                try
                {
                    await using var stream = new MemoryStream();
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    var detection = await _detector.DetectCategoriesAsync(stream);
                    //var hasPeople = detection["люди"] < 0.5f;
                    var hasPeople = true;

                    stream.Position = 0;
                    using var image = Image.FromStream(stream);

                    var faceResult = new FaceProcessingResult
                    {
                        FileName = file.FileName,
                        IsHuman = hasPeople,
                        Embeddings = new List<float[]>()
                    };

                    if (hasPeople)
                    {
                        var embeddings = _faceService.GetFaceEmbedding(image);
                        if (embeddings != null)
                        {
                            faceResult.Embeddings.Add(embeddings);
                        }
                    }

                    results.Add(faceResult);
                }
                catch (Exception ex)
                {
                    results.Add(new FaceProcessingResult
                    {
                        FileName = file?.FileName ?? "unknown",
                        Error = ex.Message,
                        Success = false
                    });
                }
            });

            await Task.WhenAll(tasks);
            return Ok(results);
        }
    }

}
