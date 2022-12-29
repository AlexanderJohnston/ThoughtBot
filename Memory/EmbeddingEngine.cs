using Azure.Core;
using Discord;
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

        public async Task<EmbeddedMemory> GetEmbedding(string message)
        {
            var request = Embed(message);
            var embedding = await Respond(request);
            var embedded = new EmbeddedMemory(embedding, message);
            return embedded;
        }
        public async Task<EmbeddedMemory> GetEmbedding(IMessage message)
        {
            var request = Embed(message.Content);
            var embedding = await Respond(request);
            var embedded = new EmbeddedMemory(embedding, message.Content);
            return embedded;
        }
        public HttpRequestMessage Embed(string input, string model = "text-embedding-ada-002")
        {
            var testJson = JsonConvert.SerializeObject(new
            {
                model,
                input
            });
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new UriBuilder(_embedUri).Uri,
                Content = new StringContent(testJson, Encoding.UTF8, MediaTypeNames.Application.Json /* or "application/json" in older versions */),
            };
            // TODO Cleanse tokens
            SetBearerToken("");
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
