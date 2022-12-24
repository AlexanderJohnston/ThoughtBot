using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Web;

namespace Realization.Skill
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
                new MediaTypeWithQualityHeaderValue("application/json"));
            //_client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "19b4bb7d2b5348919647946c54b7ba00");
        }

        public UriBuilder CognitiveServicesUri(string message)
        {
            var uriTemplate = BuildUriTemplate("bb7e751e-9ab5-49a4-abc5-1dc36709b56f", "staging");
            var builder = new UriBuilder(uriTemplate);
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["subscription-key"] = "fee758b0a7fe4eb9b7ca9adca180f4ae";
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