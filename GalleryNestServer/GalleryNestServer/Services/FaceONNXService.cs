using FaceONNX;
using System.Diagnostics;
using System.Drawing;

namespace GalleryNestServer.Services
{
    public class FaceONNXService : IFaceService
    {
        private readonly FaceDetector _faceDetector;
        private readonly FaceEmbedder _faceEmbedder;

        public FaceONNXService()
        {
            _faceDetector = new FaceDetector();
            _faceEmbedder = new FaceEmbedder();
        }

        public List<float[]> GetFaceEmbeddings(Image image)
        {
            var result = new List<float[]>();

            var bitmap = image as Bitmap ?? new Bitmap(image);

            FaceDetectionResult[] faces = _faceDetector.Forward(bitmap);

            if (faces.Length == 0)
                return result;

            foreach (var face in faces.OrderByDescending(f => f.Score))
            {
                try
                {
                    var faceBox = face.Box;
                    using var croppedFace = new Bitmap(faceBox.Width, faceBox.Height);

                    using (var g = Graphics.FromImage(croppedFace))
                    {
                        g.DrawImage(
                            bitmap,
                            new Rectangle(0, 0, faceBox.Width, faceBox.Height),
                            faceBox,
                            GraphicsUnit.Pixel
                        );
                    }

                    var embedding = _faceEmbedder.Forward(croppedFace);

                    if (embedding != null && embedding.Length > 0)
                    {
                        result.Add(embedding);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing face: {ex.Message}");
                }
            }

            return result;
        }

    }
}
