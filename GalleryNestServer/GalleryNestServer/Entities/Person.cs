using LiteDB;

namespace GalleryNestServer.Entities
{
    public class Person : IdentifiableEntity
    {
        public string Guid { get; set; }
        private List<float[]> _embeddings = new List<float[]>();
        public float[] AverageEmbedding { get; private set; }
        public string? Name { get; set; }

        public IReadOnlyList<float[]> Embeddings => _embeddings.AsReadOnly();

        public void AddEmbedding(float[] embedding)
        {
            _embeddings.Add(embedding);
            UpdateAverageEmbedding();
        }

        private void UpdateAverageEmbedding()
        {
            if (_embeddings.Count == 0)
            {
                AverageEmbedding = null;
                return;
            }

            float[] sum = new float[_embeddings[0].Length];
            foreach (var emb in _embeddings)
            {
                for (int i = 0; i < emb.Length; i++)
                {
                    sum[i] += emb[i];
                }
            }

            float magnitude = MathF.Sqrt(sum.Sum(x => x * x));
            for (int i = 0; i < sum.Length; i++)
            {
                sum[i] /= magnitude;
            }

            AverageEmbedding = sum;
        }
    }
}
