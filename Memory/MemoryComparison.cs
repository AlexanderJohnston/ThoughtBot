﻿using Newtonsoft.Json;
using Memory.Converse;
using Serilog;
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
        const int MAX_SECTION_LEN = 1000;
        const string SEPARATOR = "\n* ";
        const string SECTION = "\n### Memory Section\n";
        private Tokenizer _tokenizer = new();
        private EmbeddingEngine _engine { get; set; }
        public MemoryComparison()
        {
            _engine = new EmbeddingEngine();
        }
        public static double VectorSimilarity(double[] x, double[] y)
        {
            return x.Zip(y, (a, b) => a * b).Sum();
        }
        
        // Order sections by similarity and then return as a list of tuples.
        public async Task<List<(double, (string, string))>> OrderMemorySectionsByQuerySimilarity(string query, Dictionary<(string, string), double[]> contexts)
        {
            var queryEmbedding = await _engine.EmbedQuery(query);
            var vectorArray = queryEmbedding.Embedding.Data.Select(data => data.Embedding).First();
            var memorySimilarities = contexts.Select(x => (VectorSimilarity(vectorArray, x.Value), x.Key)).OrderByDescending(x => x.Item1).ToList();
            return memorySimilarities;
        }

        ///<summary>
        ///Parse the conversation into a list of memory text separated by <see cref="SEPARATOR">.
        ///</summary> 
        private string ParseMemoryFromConversation(Conversation conversation)
        {
            var memories = conversation.Memories.Select(mem => mem);
            // Concatenate Author: to memories text
            var authorMemories = memories.Select(mem => $"{mem.Author}: {mem.Text}");
            var concat = string.Join(SEPARATOR, authorMemories);
            return concat;
        }

        public async Task<string> ConstructPrompt(string question, List<EmbeddedMemory> memories)
        {
            // Tokenize the separator and set maximum prompt length
            int separatorLen = _tokenizer.Tokenize(SEPARATOR).Count();

            // Prepare the past embedded memories for similarity comparison.
            Dictionary<(string, string), double[]> contextEmbeddings = GetContextualMemory(memories);

            // Find the most relevant memories to the question based on the past embedded contexts.
            var mostRelevantDocumentSections = await OrderMemorySectionsByQuerySimilarity(question, contextEmbeddings);

            // Construct the prompt by concatenating the most relevant memories.
            List<string> chosenSections, chosenSectionsIndexes;
            WeaveMemories(memories, separatorLen, mostRelevantDocumentSections, out chosenSections, out chosenSectionsIndexes);

            // Useful diagnostic information
            Diagnostics(chosenSections, chosenSectionsIndexes);

            // Header for the final prompt.
            const string header = @"Answer the question as truthfully as possible using the provided context, and if the answer is not contained within the text below, say ""I don't know.""

###Context 
";
            // Join all sections and prompt parts.
            return header + string.Join("", chosenSections) + "\n\n Q: " + question + "\n A:";
        }

        private static void Diagnostics(List<string> chosenSections, List<string> chosenSectionsIndexes)
        {
            Log.Verbose($"Selected {chosenSections.Count} document sections:");
            Log.Verbose(string.Join("\n", chosenSectionsIndexes));
        }

        private List<string> WeaveMemories(List<EmbeddedMemory> memories, int separatorLen, List<(double, (string, string))> mostRelevantDocumentSections, out List<string> chosenSections, out List<string> chosenSectionsIndexes)
        {
            chosenSections = new List<string>();
            var chosenSectionsLen = 0;
            chosenSectionsIndexes = new List<string>();
            foreach (var key in mostRelevantDocumentSections)
            {
                // Add contexts until we run out of space.
                // Make sure documentSection exists in memories with Any
                var documentSectionExists = memories.Any(x => x.Topic == key.Item2.Item1 && x.Context == key.Item2.Item2);
                if (documentSectionExists)
                {
                    var documentSection = memories.First(x => x.Topic == key.Item2.Item1 && x.Context == key.Item2.Item2);
                    var conversation = documentSection.Conversation.Memories.Select(memory => $"{memory.Author}: {memory.Text}").ToList();
                    var tokens = _tokenizer.Tokenize(string.Join(SEPARATOR, conversation));
                    chosenSectionsLen += tokens.Count() + separatorLen;
                    if (chosenSectionsLen > MAX_SECTION_LEN)
                    {
                        break;
                    }
                    var sections = documentSection.Conversation.Memories.Select(memory => memory.Text).ToList();
                    var concat = string.Join(SEPARATOR, sections);
                    //var section = SECTION + concat; // TODO Save idea for later when you feel like doing the token math.
                    chosenSections.Add(concat);
                    chosenSectionsIndexes.Add($"Topic: {documentSection.Topic}, Context: {documentSection.Context}, Section: {concat}");
                }
            }
            return chosenSections;
        }

        /// <summary>
        /// Converts embedded memories into the format that the dot product similarity function 
        /// <see cref="VectorSimilarity(double[], double[])"/> expects.
        /// </summary>
        /// <param name="memories">A list of previously embedded memories.</param>
        /// <returns></returns>
        private static Dictionary<(string, string), double[]> GetContextualMemory(List<EmbeddedMemory> memories)
        {
            // Populate the dictionary with the EmbeddedMemory list using the Topic and Context for the tuple key, and use the first GptEmbedding.Data.Embedding array for the value.
            var contextEmbeddings = new Dictionary<(string, string), double[]>();
            foreach (var memory in memories)
            {
                contextEmbeddings.Add((memory.Topic, memory.Context), memory.Embedding.Data.Select(data => data.Embedding).First());
            }

            return contextEmbeddings;
        }
    }
}
