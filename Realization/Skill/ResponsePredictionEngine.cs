using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Realization.Intent;
using ThotLibrary;
using Discord;

namespace Realization.Skill
{
    public class ResponsePredictionEngine
    {
        private string _uriTemplate = @"https://westus.api.cognitive.microsoft.com/luis/prediction/v3.0/apps/{0}/slots/{1}/predict";
        private string _authorTemplate = @"https://westus.api.cognitive.microsoft.com/luis/authoring/v3.0/apps/{0}/versions/{1}/";
        private string _openUriTemplate = @"https://api.openai.com/v1/completions";

        private readonly HttpClient _client = new HttpClient();

        public List<Intention> Intentions = new List<Intention>();

        public string BuildUriTemplate(string appId, string slot) => string.Format(_uriTemplate, appId, slot);

        public ResponsePredictionEngine(List<Intention> intentions)
        {
            Intentions = intentions;
        }

        public async Task<string> PredictTopicShift(string message, string currentTopic)
        {
            var templateForPrompt = Prompts.TopicShift;
            var prompt = string.Format(templateForPrompt, currentTopic, message);
            return await PredictResponse(prompt, "text-curie-001");
        }

        public async Task<string> LearnIntent(string message)
        {
            var templateForPrompt = Prompts.Intent;
            var prompt = string.Format(templateForPrompt, message);
            return await PredictResponse(prompt, "text-curie-001");
        }

        public UriBuilder BuildRequestToOpenAi()
        {
            var uriBuilder = new UriBuilder(_openUriTemplate);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "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");
            SetClientHeaders();
            return uriBuilder;
        }              

        public HttpRequestMessage BuildHttpMessage(Uri uri, float temperature, int maxTokens, string message, string aiModel = "text-davinci-002")
        {
            var testJson = JsonConvert.SerializeObject(new
            {
                model = aiModel,
                prompt = message,
                temperature = temperature,
                max_tokens = maxTokens
            });
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = uri,
                Content = new StringContent(testJson, Encoding.UTF8, MediaTypeNames.Application.Json /* or "application/json" in older versions */),
            };
            return request;
        }

        public async Task<string> PredictResponse(string message, string model = "text-davinci-002")
        {
            var uriBuilder = BuildRequestToOpenAi();
            var request = BuildHttpMessage(uriBuilder.Uri, 0.7f, 256, message, model);
            var response = await _client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var deserialize = JsonConvert.DeserializeObject<GptPredictionResponse>(responseBody);
            return deserialize.choices.FirstOrDefault().text;
        }

        public async Task<FormulatedIntent> PredictAsync(string message)
        {
            var uriBuilder = CognitiveServicesUri(message);
            SetClientHeaders();

            // Make the prediction and get the response.
            var predictionJson = await _client.GetStringAsync(uriBuilder.Uri);
            var response = JsonConvert.DeserializeObject<PredictionResponse>(predictionJson);

            // Unpack the top intention from the prediction.
            string intention = response.Prediction.TopIntent;
            var topPrediction = response.Prediction.Intents[intention];
            float score = topPrediction.Score;
            var entities = response.Prediction.Entities.ToList();
            Log.Verbose(string.Format("Predicted Intent: [ {0} ] Score: [ {1} ] Entities: [ {2} ]", intention, score, ""));

            // Return the intent if it was above the 65% confidence threshold.
            if (Intentions.Any(intent => intent.Name == intention) && score > 0.1F)
                return new FormulatedIntent() { Predicted = Intentions.First(intent => intent.Name == intention), Entities = entities };
            else
                return new FormulatedIntent() { Predicted = new None() };
        }

        private void SetClientHeaders()
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            //_client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "19b4bb7d2b5348919647946c54b7ba00");
        }

        private UriBuilder CognitiveServicesUri(string message)
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

        private UriBuilder CognitiveAuthorUri(string message)
        {
            return new UriBuilder();
        }
    }

    public class FormulatedIntent
    {
        public Intention Predicted;
        public List<KeyValuePair<string, string[]>> Entities;
    }

    public class PredictionResponse
    {
        public string Query;
        public Prediction Prediction;
    }

    public class GptPredictionResponse
    {
        public string id;
        [JsonProperty("object")]
        public string _object;
        public string model;
        public List<GptChoice> choices;
        public GptUsage usage;
    }

    public class GptUsage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }

    public class GptChoice
    {
        public string text;
        public int index;
        public string logprobs;
        public string finish_reason;
    }

    public class Prediction 
    {
        public string TopIntent;
        public Dictionary<string, Scores> Intents;
        public Dictionary<string,string[]> Entities;
    }

    public class Scores
    {
        public float Score;
    }
}
