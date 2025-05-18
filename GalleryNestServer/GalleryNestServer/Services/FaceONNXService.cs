using FaceONNX;
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

        public float[]? GetFaceEmbedding(Image image)
        {
            FaceDetectionResult[] faces = _faceDetector.Forward((Bitmap)image);
            if (faces.Length == 0) return null;
            var firstFace = faces.OrderByDescending(f => f.Score).First();
            var embedding = _faceEmbedder.Forward((Bitmap)image);

            return embedding;
        }

    }
}
