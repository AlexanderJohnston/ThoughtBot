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

namespace Realization
{
    public class Cortex
    {
        public string ProjectName = "Realize";
        public string DeploymentName = "FirstRealization";
        public List<Intention> Intentions;
        private GetAClue _cognition;

        public Cortex(List<Intention> intentions, GetAClue cognition)
        {
            Intentions = intentions;
            _cognition = cognition;
        }

        public async Task<EmbeddedMemory> EmbedMemory(IMessage message)
        {
            var embedEngine = new EmbeddingEngine();
            var embed = await embedEngine.GetEmbedding(message);
            var model = JsonConvert.SerializeObject(embed.Embedding.Model);
            var usage = JsonConvert.SerializeObject(embed.Embedding.Usage);
            var obj = JsonConvert.SerializeObject(embed.Embedding.Object);
            var data = JsonConvert.SerializeObject(embed.Embedding.Data[0].Embedding.Take(10));
            var template = @"model: {0}
usage: {1},
object: {2},
data: {3}";
            var output = string.Format(template, model, usage, obj, data);
            await message.Channel.SendMessageAsync(output);
            return embed;
        }

        public async Task<PredictedIntent> PredictIntention(IMessage message)
        {
            var responseEngine = new ResponsePredictionEngine(Intentions);
            var intent = await responseEngine.PredictAsync(message.Content);
            var formulatedJson = JsonConvert.SerializeObject(intent);
            if (intent.Predicted.Name == "None")
            {
                return new PredictedIntent(new None(), formulatedJson);
            }
            return new PredictedIntent(intent.Predicted, formulatedJson);
        }

        public async Task<string> PredictTopicShift(IMessage message, string currentTopic, string customMessage = "")
        {
            if (customMessage == string.Empty)
            {
                var responseEngine = new ResponsePredictionEngine(Intentions);
                var response = await responseEngine.PredictTopicShift(message.Content, currentTopic);
                return response;
            }
            else
            {
                var responseEngine = new ResponsePredictionEngine(Intentions);
                var response = await responseEngine.PredictTopicShift(customMessage, currentTopic);
                return response;
            }
        }

        public async Task<string> PredictGptIntent(IMessage message)
        {
            var responseEngine = new ResponsePredictionEngine(Intentions);
            var response = await responseEngine.LearnIntent(message.Content);
            return response;
        }

        public async Task<string> PredictResponse(IMessage message, string content)
        {
            var responseEngine = new ResponsePredictionEngine(Intentions);
            var response = await responseEngine.PredictResponse(content);
            await message.Channel.SendMessageAsync(response);
            var clue = _cognition.Understanding.Services.Analysis.AnalyzeConversation(RequestContent.Create(Hydrate(message.Content)));
            await ReadAzure(clue, message);
            await ReadEntities(message);
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
