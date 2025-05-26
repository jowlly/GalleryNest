using GalleryNestServer.Data;
using GalleryNestServer.Entities;
using LiteDB;

namespace GalleryNestServer.Repositories
{
    public class PersonRepository : EntityRepository<Person>
    {
        public PersonRepository(LiteDatabase database, string collectionName) : base(database, collectionName)
        {
            _collection.EnsureIndex(x => x.Guid);
        }
        public Person? GetByEmbedding(List<float[]> inputEmbeddings, float similarityThreshold = 0.6f)
        {
            var normalizedInputs = inputEmbeddings.Select(Normalize).ToList();

            return _collection.FindAll()
                .OrderByDescending(person =>
                    normalizedInputs.Max(input =>
                        CalculateSimilarity(person.AverageEmbedding, input)))
                .FirstOrDefault(person =>
                    normalizedInputs.Any(input =>
                        CalculateSimilarity(person.AverageEmbedding, input) >= similarityThreshold));
        }

        private float CalculateSimilarity(float[] a, float[] b)
        {
            float dot = 0;
            for (int i = 0; i < a.Length; i++)
                dot += a[i] * b[i];
            return dot;
        }

        private float[] Normalize(float[] vector)
        {
            float magnitude = MathF.Sqrt(vector.Sum(x => x * x));
            return vector.Select(x => x / magnitude).ToArray();
        }
    }
}
