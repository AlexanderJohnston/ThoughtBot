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
        private Tokenizer _tokenizer = new();
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
        
        // Order sections by similarity and then return as a list of tuples.
        public async Task<List<(double, (string, string))>> OrderMemorySectionsByQuerySimilarity(string query, Dictionary<(string, string), double[]> contexts)
        {
            var queryEmbedding = await _engine.GetEmbedding(query);
            var vectorArray = queryEmbedding.Embedding.Data.Select(data => data.Embedding).First();
            
            var memorySimilarities = contexts.Select(x => (VectorSimilarity(vectorArray, x.Value), x.Key)).OrderByDescending(x => x.Item1).ToList();

            return memorySimilarities;
        }
        public async Task<string> ConstructPrompt(string question, List<EmbeddedMemory> memories)
        {
            const int MAX_SECTION_LEN = 500;
            const string SEPARATOR = "\n* ";
            int separatorLen = _tokenizer.Tokenize(SEPARATOR).Count();
            
            var contextEmbeddings = new Dictionary<(string, string), double[]>();
            //Populate the dictionary with the EmbeddedMemory list using the Topic and Context for the tuple key, and use the first GptEmbedding.Data.Embedding array for the value.
            foreach (var memory in memories)
            {
                contextEmbeddings.Add((memory.Topic, memory.Context), memory.Embedding.Data.Select(data => data.Embedding).First());
            }

            var mostRelevantDocumentSections = await OrderMemorySectionsByQuerySimilarity(question, contextEmbeddings);

            var chosenSections = new List<string>();
            var chosenSectionsLen = 0;
            var chosenSectionsIndexes = new List<string>();

            foreach (var key in mostRelevantDocumentSections)
            {
                // Add contexts until we run out of space.        
                var documentSection = memories.First(x => x.Topic == key.Item2.Item1 && x.Context == key.Item2.Item2).Context;
                var tokens = _tokenizer.Tokenize(documentSection);
                chosenSectionsLen += tokens.Count() + separatorLen;
                if (chosenSectionsLen > MAX_SECTION_LEN)
                {
                    break;
                }

                chosenSections.Add(SEPARATOR + documentSection.Content.Replace("\n", " "));
                chosenSectionsIndexes.Add(sectionIndex.ToString());
            }

            // Useful diagnostic information
            Console.WriteLine($"Selected {chosenSections.Count} document sections:");
            Console.WriteLine(string.Join("\n", chosenSectionsIndexes));

            const string header = @"Answer the question as truthfully as possible using the provided context, and if the answer is not contained within the text below, say ""I don't know.""

Context:
";

            return header + string.Join("", chosenSections) + "\n\n Q: " + question + "\n A:";
        }
    }
}
