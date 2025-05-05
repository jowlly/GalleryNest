
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SkiaSharp;
using YoloDotNet;
using YoloDotNet.Models;

namespace GalleryNestServer.Services
{
    public class YoloCategoryDetector : IObjectDetector, IDisposable
    {
        private readonly Yolo _yolo;
        private readonly Yolo _yoloC;
        private readonly SemaphoreSlim _semaphore = new(2);
        private readonly Dictionary<string, List<string>> _categoryMapping;

        public YoloCategoryDetector(IConfiguration config)
        {
            var yoloConfig = new YoloOptions
            {
                OnnxModel = config["MLModels:YoloModelPath"],
                Cuda = false,
                ModelType = YoloDotNet.Enums.ModelType.ObjectDetection,
            };

            _yolo = new Yolo(yoloConfig);

            _yoloC = new Yolo(new YoloOptions
            {
                OnnxModel = config["MLModels:YoloCModelPath"],
                Cuda = false,
                ModelType = YoloDotNet.Enums.ModelType.Classification,
            });

            _categoryMapping = config.GetSection("MLModels:ClassMapping")
                .Get<Dictionary<string, List<string>>>()
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public async Task<Dictionary<string, float>> DetectCategoriesAsync(Stream imageStream)
        {
            await _semaphore.WaitAsync();
            try
            {
                using var imageSharp = await Image.LoadAsync(imageStream);
                using var skImage = ConvertToSkiaImage(imageSharp);
                var results = _yolo.RunObjectDetection(skImage);
                Console.WriteLine(_yoloC.OnnxModel.Labels);
                var clas = _yoloC.RunClassification(skImage);
                var dict = new Dictionary<string, float>();
                dict[clas.First().Label] = (float)clas.First().Confidence;
                return dict;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private SKImage ConvertToSkiaImage(Image imageSharp)
        {
            using var memoryStream = new MemoryStream();
            imageSharp.Save(memoryStream, new PngEncoder());
            memoryStream.Position = 0;

            return SKImage.FromEncodedData(memoryStream);
        }

        private Dictionary<string, float> CalculateCategoryScores(IEnumerable<Classification> results)
        {
            var categoryScores = new Dictionary<string, float>();

            foreach (var (category, labels) in _categoryMapping)
            {
                var maxConfidence = results
                    .Where(r => labels.Contains(r.Label, StringComparer.OrdinalIgnoreCase))
                    .Select(r => r.Confidence)
                    .DefaultIfEmpty(0)
                    .Max();

                categoryScores[category] = (float)maxConfidence;
            }

            if (categoryScores["человек"] > 0)
            {
                categoryScores["человек"] = Math.Min(categoryScores["человек"] * 1.5f, 1.0f);
            }

            return categoryScores;
        }

        public void Dispose()
        {
            _yolo?.Dispose();
            _semaphore?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
