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

        public async Task<Intention> PredictAsync(string message)
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
            Log.Verbose(string.Format("Predicted Intent: [ {0} ] Score: [ {1} ]", intention, score));

            // Return the intent if it was above the 65% confidence threshold.
            if (Intentions.Any(intent => intent.Name == intention) && score > .65F)
                return Intentions.First(intent => intent.Name == intention);
            else
                return new None();
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
            var uriTemplate = BuildUriTemplate("f14ae124-3882-496b-bd05-ba01a35a4df5", "staging");
            var builder = new UriBuilder(uriTemplate);
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["subscription-key"] = "19b4bb7d2b5348919647946c54b7ba00";
            query["verbose"] = "false";
            query["log"] = "false";
            query["show-all-intents"] = "false";
            query["query"] = message;

            builder.Query = query.ToString();
            return builder;
        }
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
    }

    public class Scores
    {
        public float Score;
    }
}
