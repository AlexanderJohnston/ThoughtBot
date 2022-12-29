using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory
{
    /// <summary>
    /// At the time of question-answering, to answer the user's query we compute the query embedding of the question.
    /// and use it to find the most similar document sections. Since this is a small example, we store and search the 
    /// embeddings locally. If you have a larger dataset, consider using a vector search engine like Pinecone or Weaviate 
    /// to power the search.
    /// </summary>
    public class MemoryComparison
    {
        private Memories _memories { get; set; }
        private EmbeddingEngine _engine { get; set; }
        public MemoryComparison(Memories memories)
        {
            _memories = memories;
            _engine = new EmbeddingEngine();
        }
        public static double VectorSimilarity(double[] x, double[] y)
        {
            return x.Zip(y, (a, b) => a * b).Sum();
        }
        
        public async Task<List<(double, (string, string))>> OrderMemorySectionsByQuerySimilarity(string query, Dictionary<(string, string), double[]> contexts)
        {
            var queryEmbedding = await _engine.GetEmbedding(query);
            var vectorArray = queryEmbedding.Embedding.Data.Select(data => data.Embedding).First();
            
            var memorySimilarities = contexts.Select(x => (VectorSimilarity(vectorArray, x.Value), x.Key)).OrderByDescending(x => x.Item1).ToList();

            return memorySimilarities;
        }
    }
}
