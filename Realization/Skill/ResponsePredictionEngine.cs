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
    public class ResponsePredictionEngine : CognitiveHttpClient
    {
        private string _authorTemplate = @"https://westus.api.cognitive.microsoft.com/luis/authoring/v3.0/apps/{0}/versions/{1}/";
        private readonly string _cognitiveUriTemplate = @"https://realize.cognitiveservices.azure.com/language/:analyze-conversations?api-version=2022-10-01-preview";
        private readonly string _openUriTemplate = @"https://api.openai.com/v1/completions";

        public List<Intention> Intentions = new List<Intention>();

        public ResponsePredictionEngine(List<Intention> intentions)
        {
            Intentions = intentions;
        }

        public async Task<string> PredictTopicShift(string message, string currentTopic)
        {
            var templateForPrompt = Prompts.TopicShift;
            var prompt = string.Format(templateForPrompt, currentTopic, message);
            return await PredictResponse(prompt, "text-ada-001");
        }

        public async Task<string> LearnIntent(string message)
        {
            var templateForPrompt = Prompts.Intent;
            var prompt = string.Format(templateForPrompt, message);
            return await PredictResponse(prompt, "text-ada-001");
        }

        public async Task<string> PredictResponse(string message, string model = "text-ada-001")
        {
            var uriBuilder = BuildRequestToOpenAi();
            var request = OpenAIRequest(uriBuilder.Uri, 0.7f, 256, message, "text-ada-001");
            HttpResponseMessage response = await DefaultResponder(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var deserialize = JsonConvert.DeserializeObject<GptPredictionResponse>(responseBody);
            return deserialize.choices.FirstOrDefault().text;
        }

        private UriBuilder BuildRequestToOpenAi()
        {
            var uriBuilder = new UriBuilder(_openUriTemplate);
            // TODO cleanse tokens from source
            SetBearerToken("");
            return uriBuilder;
        }

        private HttpRequestMessage OpenAIRequest(Uri uri, float temperature, int maxTokens, string message, string aiModel = "text-ada-002")
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

        public async Task<FormulatedIntent> PredictAsync(string message)
        {
            var uriBuilder = CognitiveServicesUri(message);

            // Make the prediction and get the response.
            string predictionJson = await DefaultGet(uriBuilder);
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
