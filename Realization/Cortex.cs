using Azure;
using Azure.AI.TextAnalytics;
using Azure.Core;
using AzureLUIS;
using Discord;
using Newtonsoft.Json;
using System.Text.Json;
using ThotLibrary;
using Memory;
using Memory.Intent;
using Discord.WebSocket;
using Memory.Converse;

namespace Realization
{
    public class Cortex
    {
        public string ProjectName = "Realize";
        public string DeploymentName = "FirstRealization";
        public List<Intention> Intentions;
        private GetAClue _cognition;
        private string _openAIToken;

        public Cortex(List<Intention> intentions, GetAClue cognition, string token)
        {
            _openAIToken = token;
            Intentions = intentions;
            _cognition = cognition;
        }

        public async Task<EmbeddedMemory> EmbedMemory(Conversation conversation, SocketMessage messageParam, string topic, string context)
        {
            var embedEngine = new EmbeddingEngine();
            var embed = await embedEngine.GetEmbedding(conversation, topic, context);
            var model = JsonConvert.SerializeObject(embed.Embedding.Model);
            var usage = JsonConvert.SerializeObject(embed.Embedding.Usage);
            var obj = JsonConvert.SerializeObject(embed.Embedding.Object);
            var data = JsonConvert.SerializeObject(embed.Embedding.Data[0].Embedding.Take(10));
            var template = @"model: {0}
usage: {1},
object: {2},
data: {3}";
            var output = string.Format(template, model, usage, obj, data);
            await messageParam.Channel.SendMessageAsync(output);
            return embed;
        }


        /// <summary>
        /// The PredictTopicShift method is a method of the Cortex object. It appears to be using a prediction system, represented 
        /// by the ResponsePredictionEngine object, to determine if the topic of a conversation has shifted. 
        /// The ResponsePredictionEngine object is initialized with a list of intentions, represented by the Intentions field.
        /// </summary>
        /// <param name="message">an object representing the message being analyzed</param>
        /// <param name="currentTopic">a string representing the current topic of the conversation</param>
        /// <param name="customMessage">a string containing a custom message to be used in place </param>
        /// <returns><see cref="TopicShiftAnswer"/>an object that contains a boolean value indicating whether the topic has shifted and a string value representing the new topic</returns>
        public async Task<TopicShiftAnswer> PredictTopicShift(IMessage message, string currentTopic, string customMessage = "")
        {
            if (customMessage == string.Empty)
            {
                var responseEngine = new ResponsePredictionEngine(_openAIToken);
                //var response = await responseEngine.PredictTopicShift(message.Content, currentTopic);
                var response = await responseEngine.PredictComplexShift(message.Content, currentTopic);

                return response;
            }
            else
            {
                var responseEngine = new ResponsePredictionEngine(_openAIToken);
                //var response = await responseEngine.PredictTopicShift(customMessage, currentTopic);
                var response = await responseEngine.PredictComplexShift(customMessage, currentTopic);
                return response;
            }
        }

        public async Task<string> PredictGptIntent(IMessage message)
        {
            var responseEngine = new ResponsePredictionEngine(_openAIToken);
            var response = await responseEngine.LearnIntent(message.Content);
            return response;
        }

        public async Task<string> PredictResponse(IMessage message, string content)
        {
            var responseEngine = new ResponsePredictionEngine(_openAIToken);
            var response = await responseEngine.PredictResponse(content);
            await message.Channel.SendMessageAsync(response);
            //var clue = _cognition.Understanding.Services.Analysis.AnalyzeConversation(RequestContent.Create(Hydrate(message.Content)));
            //await ReadAzure(clue, message);
            //await ReadEntities(message);
            return response;
        }

        private async Task ReadAzure(Response response, IMessage message)
        {
            using JsonDocument result = JsonDocument.Parse(response.ContentStream);
            JsonElement conversationalTaskResult = result.RootElement;
            JsonElement conversationPrediction = conversationalTaskResult.GetProperty("result").GetProperty("prediction");

            await message.Channel.SendMessageAsync($"Top intent: {conversationPrediction.GetProperty("topIntent").GetString()}");

            await message.Channel.SendMessageAsync("Intents:");
            foreach (JsonElement intent in conversationPrediction.GetProperty("intents").EnumerateArray())
            {
                await message.Channel.SendMessageAsync($"Category: {intent.GetProperty("category").GetString()}");
                await message.Channel.SendMessageAsync($"Confidence: {intent.GetProperty("confidenceScore").GetSingle()}");
                await message.Channel.SendMessageAsync(Environment.NewLine);
            }

            await message.Channel.SendMessageAsync("Entities:");
            foreach (JsonElement entity in conversationPrediction.GetProperty("entities").EnumerateArray())
            {
                await message.Channel.SendMessageAsync($"Category: {entity.GetProperty("category").GetString()}");
                await message.Channel.SendMessageAsync($"Text: {entity.GetProperty("text").GetString()}");
                await message.Channel.SendMessageAsync($"Offset: {entity.GetProperty("offset").GetInt32()}");
                await message.Channel.SendMessageAsync($"Length: {entity.GetProperty("length").GetInt32()}");
                await message.Channel.SendMessageAsync($"Confidence: {entity.GetProperty("confidenceScore").GetSingle()}");
                await message.Channel.SendMessageAsync(Environment.NewLine);

                if (entity.TryGetProperty("resolutions", out JsonElement resolutions))
                {
                    foreach (JsonElement resolution in resolutions.EnumerateArray())
                    {
                        if (resolution.GetProperty("resolutionKind").GetString() == "DateTimeResolution")
                        {
                            await message.Channel.SendMessageAsync($"Datetime Sub Kind: {resolution.GetProperty("dateTimeSubKind").GetString()}");
                            await message.Channel.SendMessageAsync($"Timex: {resolution.GetProperty("timex").GetString()}");
                            await message.Channel.SendMessageAsync($"Value: {resolution.GetProperty("value").GetString()}");
                            await message.Channel.SendMessageAsync(Environment.NewLine);
                        }
                    }
                }
            }
        }

        private async Task ReadEntities(IMessage message)
        {
            Response<CategorizedEntityCollection> response = _cognition.Understanding.Services.TextAnalyzer.RecognizeEntities(message.Content);
            CategorizedEntityCollection entitiesInDocument = response.Value;

            Console.WriteLine($"Recognized {entitiesInDocument.Count} entities:");
            foreach (CategorizedEntity entity in entitiesInDocument)
            {
                await message.Channel.SendMessageAsync($"  Text: {entity.Text}");
                await message.Channel.SendMessageAsync($"  Offset: {entity.Offset}");
                await message.Channel.SendMessageAsync($"  Length: {entity.Length}");
                await message.Channel.SendMessageAsync($"  Category: {entity.Category}");
                if (!string.IsNullOrEmpty(entity.SubCategory))
                    await message.Channel.SendMessageAsync($"  SubCategory: {entity.SubCategory}");
                await message.Channel.SendMessageAsync($"  Confidence score: {entity.ConfidenceScore}");
                //await message.Channel.SendMessageAsync(Environment.NewLine);
            }
        }

        private object Hydrate(string message)
        {
            return new
            {
                analysisInput = new
                {
                    conversationItem = new
                    {
                        text = message,
                        id = "1",
                        participantId = "1",
                    }
                },
                parameters = new
                {
                    projectName = ProjectName,
                    deploymentName = DeploymentName,
                    // Use Utf16CodeUnit for strings in .NET.
                    stringIndexType = "Utf16CodeUnit",
                },
                kind = "Conversation",
            };
        }
    }
}
