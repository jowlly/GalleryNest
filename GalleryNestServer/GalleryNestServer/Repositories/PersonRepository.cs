using GalleryNestServer.Data;
using GalleryNestServer.Entities;
using LiteDB;

namespace GalleryNestServer.Repositories
{
    public class PersonRepository : EntityRepository<Person>
    {
        public PersonRepository(LiteDatabase database, string collectionName) : base(database, collectionName)
        {
            _collection.EnsureIndex(x => x.Embeddings);
        }
        public Person? GetByEmbedding(List<float[]> inputEmbeddings, float similarityThreshold = 0.75f)
        {
            return _collection.FindAll()
                .FirstOrDefault(person =>
                    person.Embeddings.Any(personEmbedding =>
                        inputEmbeddings.Any(inputEmbedding =>
                            IsCosineSimilar(personEmbedding, inputEmbedding, similarityThreshold))));
        }

        private bool IsCosineSimilar(float[] a, float[] b, float threshold)
        {
            if (a.Length != b.Length) return false;

            float dotProduct = 0f;
            float magnitudeA = 0f;
            float magnitudeB = 0f;

            for (int i = 0; i < a.Length; i++)
            {
                dotProduct += a[i] * b[i];
                magnitudeA += a[i] * a[i];
                magnitudeB += b[i] * b[i];
            }

            magnitudeA = MathF.Sqrt(magnitudeA);
            magnitudeB = MathF.Sqrt(magnitudeB);

            if (magnitudeA == 0 || magnitudeB == 0)
                return false;

            float similarity = dotProduct / (magnitudeA * magnitudeB);
            return similarity >= threshold;
        }
        private bool IsSimilar(float[] a, float[] b, float epsilon)
        {
            if (a.Length != b.Length) return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (Math.Abs(a[i] - b[i]) > epsilon)
                    return false;
            }
            return true;
        }
    }
}
