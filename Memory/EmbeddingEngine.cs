using Azure.Core;
using Discord;
using Memory.Converse;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Memory
{
    public class EmbeddingEngine : CognitiveHttpClient
    {
        private string _embedUri = @"https://api.openai.com/v1/embeddings";
        private string _key = File.ReadAllText(Environment.CurrentDirectory + "\\key.openAI");


        public async Task<QueryEmbedding> EmbedQuery(string message)
        {
             var request = EmbedMessage(message);
            var embedding = await Respond(request);
            var embedded = new QueryEmbedding(embedding, message, "Question", "Recall memory and context");
            return embedded;
        }
        public async Task<EmbeddedMemory> GetEmbedding(Conversation conversation, string topic, string context)
        {
            var request = Embed(conversation);
            var embedding = await Respond(request);
            var embedded = new EmbeddedMemory(embedding, conversation, topic, context);
            return embedded;
        }
        //public async Task<EmbeddedMemory> GetEmbedding(IMessage message, string topic, string context)
        //{
        //    var request = Embed(message.Content);
        //    var embedding = await Respond(request);
        //    var embedded = new EmbeddedMemory(embedding, message.Content, topic, context);
        //    return embedded;
        //}

        public HttpRequestMessage EmbedMessage(string inputMessage, string modelChoice = "text-embedding-ada-002")
        {
            var input = new
            {
                input = inputMessage,
                model = modelChoice
            };
            var json = JsonConvert.SerializeObject(input);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new UriBuilder(_embedUri).Uri,
                Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json /* or "application/json" in older versions */),
            };
            SetBearerToken(_key);
            return request;
        }
        public HttpRequestMessage Embed(Conversation input, string model = "text-embedding-ada-002")
        {
            var memories = input.Memories.Select(mem => mem);
            // Concatenate Author: to memories text
            var authorMemories = memories.Select(mem => $"{mem.Author}: {mem.Text}");
            var concat = string.Join("\n*", authorMemories);
            var json = JsonConvert.SerializeObject(new
            {
                input = concat,
                model = model
            });
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new UriBuilder(_embedUri).Uri,
                Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json /* or "application/json" in older versions */),
            };
            SetBearerToken(_key);
            return request;
        }
        public async Task<GptEmbedding> Respond(HttpRequestMessage request)
        {
            HttpResponseMessage response = await DefaultResponder(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var deserialize = JsonConvert.DeserializeObject<GptEmbedding>(responseBody);
            return deserialize;
        }
    }
}
