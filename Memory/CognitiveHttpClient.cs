﻿using System.Net.Http.Headers;
using System.Net.Mime;
using System.Web;

namespace Memory
{
    public class CognitiveHttpClient
    {
        public HttpClient Client { get => _client; }
        private readonly HttpClient _client = new HttpClient();
        private string _uriTemplate = @"https://westus.api.cognitive.microsoft.com/luis/prediction/v3.0/apps/{0}/slots/{1}/predict";

        public async Task<string> DefaultGet(UriBuilder uriBuilder)
        {
            SetClientHeaders();
            return await _client.GetStringAsync(uriBuilder.Uri);
        }

        public async Task<HttpResponseMessage> DefaultResponder(HttpRequestMessage request)
        {
            SetClientHeaders();
            return await _client.SendAsync(request).ConfigureAwait(false);
        }

        public void SetBearerToken(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public string BuildUriTemplate(string appId, string slot) => string.Format(_uriTemplate, appId, slot);

        private void SetClientHeaders()
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            _client.Timeout = TimeSpan.FromSeconds(30);
        }

        public UriBuilder CognitiveServicesUri(string message)
        {
            var uriTemplate = BuildUriTemplate("YOUR_CLU_URI_HERE", "staging");
            var builder = new UriBuilder(uriTemplate);
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["subscription-key"] = "YOUR_CLU_KEY_HERE";
            query["verbose"] = "false";
            query["log"] = "false";
            query["show-all-intents"] = "true";
            query["query"] = message;

            builder.Query = query.ToString();
            return builder;
        }
        public UriBuilder CognitiveAuthorUri(string message)
        {
            return new UriBuilder();
        }
    }
}