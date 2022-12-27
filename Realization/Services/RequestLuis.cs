using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace Realization.Services
{
    public class RequestLuis
    {
        private string _uriTemplate = @"https://westus.api.cognitive.microsoft.com/luis/api/v2.0/apps/{0}/versions/{1}/example";

        private readonly HttpClient _client = new HttpClient();
        public string BuildUriTemplate(string appId, string versionId) => string.Format(_uriTemplate, appId, versionId);

        public async Task<HttpResponseMessage> BuildRequest(string bodyJson)
        {
            var uriBuilder = CognitiveServicesUri();
            SetClientHeaders();
            return await _client.PostAsync(uriBuilder.Uri, new StringContent(bodyJson, Encoding.UTF8, "application/json"));
        }

        private void SetClientHeaders()
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "19b4bb7d2b5348919647946c54b7ba00");
        }

        private UriBuilder CognitiveServicesUri()
        {
            var uriTemplate = BuildUriTemplate("ccf14592-8bca-4d36-925b-8152d2aedddc", "0.1");
            var builder = new UriBuilder(uriTemplate);
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["subscription-key"] = "fee758b0a7fe4eb9b7ca9adca180f4ae";

            builder.Query = query.ToString();
            return builder;
        }
    }
}
