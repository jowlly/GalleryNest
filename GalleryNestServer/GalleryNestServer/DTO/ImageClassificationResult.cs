namespace GalleryNestServer.DTO
{
    public class ImageClassificationResult
    {
        public string FileName { get; set; }
        public Dictionary<string, float> Categories { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }

}
