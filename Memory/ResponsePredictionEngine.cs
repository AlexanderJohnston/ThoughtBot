using System.Threading.Tasks;
using System.Web;
using ThotLibrary;
using Discord;
using Memory.Learning;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text;
using Serilog;
using Memory.Intent;
using Memory.Chat;

namespace Memory
{
    //Class representing the answer for a topic shift. The answer is a single string that needs to be broken up into three parts broken up by \n delimeters.
    //The first line is Decision: Yes or Decision: No
    //The second line is Topic: <topic>
    //The third line is Reasoning: <reasoning>
    public class TopicShiftAnswer
    {
        public string Decision { get; set; }
        public string Topic { get; set; }
        public string Reasoning { get; set; }
        public TopicShiftAnswer(string answer)
        {
            // Split the answer into three parts.
            var lines = answer.Split('\n');
            // These three parts might not be formatted correctly, if any of them are not then we will store the entire answer instead.
            // Otherwise, we will trim the formatting from the answer.
            // Check that all three lines exist and then test each one to see if it is formatted correctly.
            if (lines.Length == 3 && lines[0].StartsWith("Decision: ") && lines[1].StartsWith("Topic: ") && lines[2].StartsWith("Reasoning: "))
            {
                Decision = lines[0].Substring(10);
                Topic = lines[1].Substring(7);
                Reasoning = lines[2].Substring(10);
            }
            // If all three lines still exist, we can store all of them directly.
            else if(lines.Length == 3)
            {
                Decision = lines[0];
                Topic = lines[1];
                Reasoning = lines[2];
            }
            // Set the Decision to No and the other fields to empty.
            else
            {
                Decision = "No";
                Topic = string.Empty;
                Reasoning = string.Empty;
            }
        }
    }
    //Refactor Realization.Skill.ResponsePredictionEngine for the Memory namespace.
    public class ResponsePredictionEngine : CognitiveHttpClient
    {
        private string _authorTemplate = @"https://westus.api.cognitive.microsoft.com/luis/authoring/v3.0/apps/{0}/versions/{1}/";
        private readonly string _cognitiveUriTemplate = @"https://realize.cognitiveservices.azure.com/language/:analyze-conversations?api-version=2022-10-01-preview";
        private readonly string _openUriTemplate = @"https://api.openai.com/v1/completions";
        private readonly string _openUriChatTemplate = @"https://api.openai.com/v1/chat/completions";

        public string _token;

        public ResponsePredictionEngine(string token)
        {
            _token = token;
        }

        /// <summary>
        /// a method that predicts whether the topic of a conversation has shifted based on a message and the current topic.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="currentTopic"></param>
        /// <returns><see cref="TopicShiftAnswer"/> an object that contains a boolean value indicating whether the topic has shifted and a string value representing the new topic</returns>
        public async Task<TopicShiftAnswer> PredictComplexShift(string message, string currentTopic)
        {
            var response = await PredictTopicShift(message, currentTopic, 0.2f, 1000);
            return new TopicShiftAnswer(response);
        }

        public async Task<string> PredictTopicShift(string message, string currentTopic, float temperature = 0.8f, int tokens = 512)
        {
            var templateForPrompt = Prompts.TopicShift;
            var prompt = string.Format(templateForPrompt, currentTopic, message);
            return await PredictResponse(prompt, "text-davinci-003", temperature, tokens);
        }

        public async Task<string> LearnIntent(string message)
        {
            var templateForPrompt = Prompts.Intent;
            var prompt = string.Format(templateForPrompt, message);
            return await PredictResponse(prompt, "text-ada-001");
        }

        public async Task<string> PredictResponse(string message, string model = "text-davinci-003", float temperature = 0.8f, int tokens = 512)
        {
            var uriBuilder = BuildRequestToOpenAi();
            var request = OpenAIRequest(uriBuilder.Uri, temperature, tokens, message, model);
            HttpResponseMessage response = await DefaultResponder(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var deserialize = JsonConvert.DeserializeObject<GptPredictionResponse>(responseBody);
            return deserialize.choices.FirstOrDefault().text;
        }

        public async Task<string> PredictResponse(List<GptChatMessage> chatHistory, string model = "text-davinci-003", float temperature = 0.8f, int tokens = 512)
        {
            var uriBuilder = BuildChatRequestToOpenAi();
            var request = OpenAIRequest(uriBuilder.Uri, temperature, tokens, chatHistory, model);
            HttpResponseMessage response = await DefaultResponder(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var deserialize = JsonConvert.DeserializeObject<GptChatResponse>(responseBody);
            return deserialize.Choices.FirstOrDefault().Message.Content;
        }

        private UriBuilder BuildRequestToOpenAi()
        {
            var uriBuilder = new UriBuilder(_openUriTemplate);
            SetBearerToken(_token);
            return uriBuilder;
        }

        private UriBuilder BuildChatRequestToOpenAi()
        {
            var uriBuilder = new UriBuilder(_openUriChatTemplate);
            SetBearerToken(_token);
            return uriBuilder;
        }

        private HttpRequestMessage OpenAIRequest(Uri uri, float temp, int maxTokens, string message, string aiModel = "text-ada-002")
        {
            var testJson = JsonConvert.SerializeObject(new
            {
                model = aiModel,
                prompt = message,
                temperature = temp,
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

        private HttpRequestMessage OpenAIRequest(Uri uri, float temp, int maxTokens, List<GptChatMessage> chatHistory, string aiModel = "text-ada-002")
        {
            var testJson = JsonConvert.SerializeObject(new
            {
                model = aiModel,
                messages = chatHistory,
                temperature = temp,
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
        [JsonProperty("prompt_tokens")]
        public int PromptTokens;
        [JsonProperty("completion_tokens")]
        public int CompletionTokens;
        [JsonProperty("total_tokens")]
        public int TotalTokens;
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
        public Dictionary<string, string[]> Entities;
    }

    public class Scores
    {
        public float Score;
    }
}
