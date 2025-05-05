namespace GalleryNestServer.DTO
{
    public class FaceProcessingResult
    {
        public string FileName { get; set; }
        public bool IsHuman { get; set; }
        public List<float[]> Embeddings { get; set; }
        public bool Success { get; set; } = true;
        public string Error { get; set; }
    }
}
