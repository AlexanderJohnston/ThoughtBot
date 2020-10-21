using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ThotBot.Intent;

namespace ThotBot.Skill
{
    public class ResponsePredictionEngine
    {
        private string _uriTemplate = @"https://westus.api.cognitive.microsoft.com/luis/prediction/v3.0/apps/{0}/slots/{1}/predict";

        private readonly HttpClient _client = new HttpClient();

        public List<Intention> Intentions = new List<Intention>();

        public string BuildUriTemplate(string appId, string slot) => string.Format(_uriTemplate, appId, slot);

        public ResponsePredictionEngine(List<Intention> intentions)
        {
            Intentions = intentions;
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
            if (Intentions.Any(intent => intent.Name == intention) && score > .65F)
                return new FormulatedIntent() { Predicted = Intentions.First(intent => intent.Name == intention), Entities = entities };
            else
                return new FormulatedIntent() { Predicted = new None() };
        }

        private void SetClientHeaders()
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "19b4bb7d2b5348919647946c54b7ba00");
        }

        private UriBuilder CognitiveServicesUri(string message)
        {
            var uriTemplate = BuildUriTemplate("ccf14592-8bca-4d36-925b-8152d2aedddc", "staging");
            var builder = new UriBuilder(uriTemplate);
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["subscription-key"] = "fee758b0a7fe4eb9b7ca9adca180f4ae";
            query["verbose"] = "false";
            query["log"] = "false";
            query["show-all-intents"] = "false";
            query["query"] = message;

            builder.Query = query.ToString();
            return builder;
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
