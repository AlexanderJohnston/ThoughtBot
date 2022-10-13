using Azure;
using Azure.AI.TextAnalytics;
using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AzureLUIS
{

    public class AnalyticsRequest
    {
        public List<string> Documents { get; }
        public TextAnalyticsActions Actions { get; }
        public AnalyticsRequest(List<string> documents, TextAnalyticsActions actions)
        {
            Documents = documents;
            Actions = actions;
        }
        public async Task HandleRequestAsync(TextAnalyticsClient client)
        {
            AnalyzeActionsOperation operation = await client.StartAnalyzeActionsAsync(Documents, Actions);
            await operation.WaitForCompletionAsync();

            Console.WriteLine($"Status: {operation.Status}");
            Console.WriteLine($"Created On: {operation.CreatedOn}");
            Console.WriteLine($"Expires On: {operation.ExpiresOn}");
            Console.WriteLine($"Last modified: {operation.LastModified}");
            if (!string.IsNullOrEmpty(operation.DisplayName))
                Console.WriteLine($"Display name: {operation.DisplayName}");
            Console.WriteLine($"Total actions: {operation.ActionsTotal}");
            Console.WriteLine($"  Succeeded actions: {operation.ActionsSucceeded}");
            Console.WriteLine($"  Failed actions: {operation.ActionsFailed}");
            Console.WriteLine($"  In progress actions: {operation.ActionsInProgress}");
            await foreach (AnalyzeActionsResult documentsInPage in operation.Value)
            {
                IReadOnlyCollection<ExtractKeyPhrasesActionResult> keyPhrasesResults = documentsInPage.ExtractKeyPhrasesResults;
                IReadOnlyCollection<RecognizeEntitiesActionResult> entitiesResults = documentsInPage.RecognizeEntitiesResults;
                IReadOnlyCollection<AnalyzeSentimentActionResult> sentimentResults = documentsInPage.AnalyzeSentimentResults;

                Console.WriteLine("Recognized Entities");
                int docNumber = 1;
                foreach (RecognizeEntitiesActionResult entitiesActionResults in entitiesResults)
                {
                    Console.WriteLine($" Action name: {entitiesActionResults.ActionName}");
                    foreach (RecognizeEntitiesResult documentResults in entitiesActionResults.DocumentsResults)
                    {
                        Console.WriteLine($" Document #{docNumber++}");
                        Console.WriteLine($"  Recognized the following {documentResults.Entities.Count} entities:");

                        foreach (CategorizedEntity entity in documentResults.Entities)
                        {
                            Console.WriteLine($"  Entity: {entity.Text}");
                            Console.WriteLine($"  Category: {entity.Category}");
                            Console.WriteLine($"  Offset: {entity.Offset}");
                            Console.WriteLine($"  Length: {entity.Length}");
                            Console.WriteLine($"  ConfidenceScore: {entity.ConfidenceScore}");
                            Console.WriteLine($"  SubCategory: {entity.SubCategory}");
                        }
                        Console.WriteLine("");
                    }
                }

                Console.WriteLine("Key Phrases");
                docNumber = 1;
                foreach (ExtractKeyPhrasesActionResult keyPhrasesActionResult in keyPhrasesResults)
                {
                    foreach (ExtractKeyPhrasesResult documentResults in keyPhrasesActionResult.DocumentsResults)
                    {
                        Console.WriteLine($" Document #{docNumber++}");
                        Console.WriteLine($"  Recognized the following {documentResults.KeyPhrases.Count} Keyphrases:");

                        foreach (string keyphrase in documentResults.KeyPhrases)
                        {
                            Console.WriteLine($"  {keyphrase}");
                        }
                        Console.WriteLine("");
                    }
                }

                Dictionary<string, int> complaints = GetComplaints(sentimentResults);

                var negativeAspect = complaints.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                Console.WriteLine($"Alert! major complaint is *{negativeAspect}*");
                Console.WriteLine();
                Console.WriteLine("---All complaints:");
                foreach (KeyValuePair<string, int> complaint in complaints)
                {
                    Console.WriteLine($"   {complaint.Key}, {complaint.Value}");
                }
            }
        }
        private Dictionary<string, int> GetComplaints(IReadOnlyCollection<AnalyzeSentimentActionResult> documents)
        {
            var complaints = new Dictionary<string, int>();
            foreach (var document in documents)
            {
                foreach (AnalyzeSentimentResult review in document.DocumentsResults)
                {
                    foreach (SentenceSentiment sentence in review.DocumentSentiment.Sentences)
                    {
                        foreach (SentenceOpinion opinion in sentence.Opinions)
                        {
                            if (opinion.Target.Sentiment == TextSentiment.Negative)
                            {
                                complaints.TryGetValue(opinion.Target.Text, out var value);
                                complaints[opinion.Target.Text] = value + 1;
                            }
                        }
                    }
                }
            }
            return complaints;
        }
    }

    public enum CognitiveAction
    {
        RecognizeEntities,
        ExtractPhrases,
        MineOpinions
    }

    public class DocumentBatch
    {
        public List<string> Documents;
        public List<CognitiveAction> Actions;
    }

    public class CognitiveLanguageUnderstanding
    {
        public CognitiveServices Services;
        public List<DocumentBatch> Batches = new();


        public CognitiveLanguageUnderstanding(CognitiveServices services) => Services = services;

        //public void 

        public void Batch(List<string> documents, List<CognitiveAction> actions) => 
            Batches.Add(new() { Documents = documents, Actions = actions });

        public List<AnalyticsRequest> CondenseBatch()
        {
            var documents = new List<string>();
            var intents = new List<CognitiveAction>();
            var requests = new List<AnalyticsRequest>();
            foreach (var batch in Batches)
            {
                TextAnalyticsActions actions = new TextAnalyticsActions();
                foreach (var action in batch.Actions)
                {
                    switch (action)
                    {
                        case CognitiveAction.MineOpinions:
                            actions.AnalyzeSentimentActions = new List<AnalyzeSentimentAction>() { new() { IncludeOpinionMining = true } };
                            break;
                        case CognitiveAction.ExtractPhrases:
                            actions.ExtractKeyPhrasesActions = new List<ExtractKeyPhrasesAction>() { new ExtractKeyPhrasesAction() };
                            break;
                        case CognitiveAction.RecognizeEntities:
                            actions.RecognizeEntitiesActions = new List<RecognizeEntitiesAction>() { new RecognizeEntitiesAction() };
                            break;
                        default:
                            throw new NotImplementedException();
                    };
                }
                var allActs = string.Join(",", batch.Actions.Select(act => act.ToString()));
                actions.DisplayName = string.Format("Realized[{0}][{1}]", allActs, DateTime.Now.ToShortDateString());
                requests.Add(BuildRequest(batch.Documents, actions));
            }
            return requests;
        }
        private AnalyticsRequest BuildRequest(List<string> documents, TextAnalyticsActions actions)
        {
            return new AnalyticsRequest(documents, actions);
        }
    }

    internal class SampleConversation
    {
        public string ProjectName = "thoughtbot";
        public string DeploymentName = "production";
        public CognitiveServices Cognitive;
        public SampleConversation(CognitiveServices services)
        {
            Cognitive = services;
        }

        public async Task AsyncBatch()
        {
            var language = new CognitiveLanguageUnderstanding(Cognitive);
            var complaints = HydrateComplaints();
            var actions = new List<CognitiveAction>() { 
                CognitiveAction.MineOpinions, CognitiveAction.RecognizeEntities, CognitiveAction.ExtractPhrases };
            language.Batch(complaints, actions);
            var requests = language.CondenseBatch();
            foreach (var request in requests)
            {
                await request.HandleRequestAsync(language.Services.TextAnalyzer);
            }
        }

        public string HydrateEntities()
        {
            return  @"We love this trail and make the trip every year. The views are breathtaking and well
                    worth the hike! Yesterday was foggy though, so we missed the spectacular views.
                    We tried again today and it was amazing. Everyone in my family liked the trail although
                    it was too challenging for the less athletic among us.
                    Not necessarily recommended for small children.
                    A hotel close to the trail offers services for childcare in case you want that.";

        }
        public List<string> HydrateComplaints()
        {
            string reviewA = @"The food and service were unacceptable, but the concierge were nice.
                 After talking to them about the quality of the food and the process
                 to get room service they refunded the money we spent at the restaurant
                 and gave us a voucher for nearby restaurants.";

            string reviewB = @"The rooms were beautiful. The AC was good and quiet, which was key for
                us as outside it was 100F and our baby was getting uncomfortable because of the heat.
                The breakfast was good too with good options and good servicing times.
                The thing we didn't like was that the toilet in our bathroom was smelly.
                It could have been that the toilet was not cleaned before we arrived.
                Either way it was very uncomfortable.
                Once we notified the staff, they came and cleaned it and left candles.";

            string reviewC = @"Nice rooms! I had a great unobstructed view of the Microsoft campus
                but bathrooms were old and the toilet was dirty when we arrived. 
                It was close to bus stops and groceries stores. If you want to be close to
                campus I will recommend it, otherwise, might be better to stay in a cleaner one.";

            var documents = new List<string>
            {
                reviewA,
                reviewB,
                reviewC
            };
            return documents;
        }

        public string HydrateEmail()
        {
            return @"My cat might need to see a veterinarian. It has been sneezing more than normal, and although my
                    little sister thinks it is funny, I am worried it has the cold that I got last week.
                    We are going to call tomorrow and try to schedule an appointment for this week. Hopefully it
                    will be covered by the cat's insurance.
                    It might be good to not let it sleep in my room for a while.";
        }

        public object Hydrate()
        {
            return new
            {
                analysisInput = new
                {
                    conversationItem = new
                    {
                        text = "Send an email to Carol about tomorrow's demo",
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
        public void RecognizeEntities(string document)
        {
            try
            {
                Response<CategorizedEntityCollection> response = Cognitive.TextAnalyzer.RecognizeEntities(document);
                CategorizedEntityCollection entitiesInDocument = response.Value;

                Console.WriteLine($"Recognized {entitiesInDocument.Count} entities:");
                foreach (CategorizedEntity entity in entitiesInDocument)
                {
                    Console.WriteLine($"  Text: {entity.Text}");
                    Console.WriteLine($"  Offset: {entity.Offset}");
                    Console.WriteLine($"  Length: {entity.Length}");
                    Console.WriteLine($"  Category: {entity.Category}");
                    if (!string.IsNullOrEmpty(entity.SubCategory))
                        Console.WriteLine($"  SubCategory: {entity.SubCategory}");
                    Console.WriteLine($"  Confidence score: {entity.ConfidenceScore}");
                    Console.WriteLine("");
                }
            }
            catch (RequestFailedException exception)
            {
                Console.WriteLine($"Error Code: {exception.ErrorCode}");
                Console.WriteLine($"Message: {exception.Message}");
            }
        }
        public void RespondComplaints(List<string> documents)
        {
            var options = new AnalyzeSentimentOptions() { IncludeOpinionMining = true };
            Response<AnalyzeSentimentResultCollection> response = Cognitive.TextAnalyzer.AnalyzeSentimentBatch(documents, options: options);
            AnalyzeSentimentResultCollection reviews = response.Value;

            Dictionary<string, int> complaints = GetComplaints(reviews);

            var negativeAspect = complaints.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            Console.WriteLine($"Alert! major complaint is *{negativeAspect}*");
            Console.WriteLine();
            Console.WriteLine("---All complaints:");
            foreach (KeyValuePair<string, int> complaint in complaints)
            {
                Console.WriteLine($"   {complaint.Key}, {complaint.Value}");
            }
        }

        private Dictionary<string, int> GetComplaints(AnalyzeSentimentResultCollection reviews)
        {
            var complaints = new Dictionary<string, int>();
            foreach (AnalyzeSentimentResult review in reviews)
            {
                foreach (SentenceSentiment sentence in review.DocumentSentiment.Sentences)
                {
                    foreach (SentenceOpinion opinion in sentence.Opinions)
                    {
                        if (opinion.Target.Sentiment == TextSentiment.Negative)
                        {
                            complaints.TryGetValue(opinion.Target.Text, out var value);
                            complaints[opinion.Target.Text] = value + 1;
                        }
                    }
                }
            }
            return complaints;
        }

        public void RespondEmail(string data)
        {
            try
            {
                Response<KeyPhraseCollection> response = Cognitive.TextAnalyzer.ExtractKeyPhrases(data);
                KeyPhraseCollection keyPhrases = response.Value;

                Console.WriteLine($"Extracted {keyPhrases.Count} key phrases:");
                foreach (string keyPhrase in keyPhrases)
                {
                    Console.WriteLine($"  {keyPhrase}");
                }
            }
            catch (RequestFailedException exception)
            {
                Console.WriteLine($"Error Code: {exception.ErrorCode}");
                Console.WriteLine($"Message: {exception.Message}");
            }
        }
        public void Respond(object data)
        {
            Response response = Cognitive.Analysis.AnalyzeConversation(RequestContent.Create(data));

            using JsonDocument result = JsonDocument.Parse(response.ContentStream);
            JsonElement conversationalTaskResult = result.RootElement;
            JsonElement conversationPrediction = conversationalTaskResult.GetProperty("result").GetProperty("prediction");

            Console.WriteLine($"Top intent: {conversationPrediction.GetProperty("topIntent").GetString()}");

            Console.WriteLine("Intents:");
            foreach (JsonElement intent in conversationPrediction.GetProperty("intents").EnumerateArray())
            {
                Console.WriteLine($"Category: {intent.GetProperty("category").GetString()}");
                Console.WriteLine($"Confidence: {intent.GetProperty("confidenceScore").GetSingle()}");
                Console.WriteLine();
            }

            Console.WriteLine("Entities:");
            foreach (JsonElement entity in conversationPrediction.GetProperty("entities").EnumerateArray())
            {
                Console.WriteLine($"Category: {entity.GetProperty("category").GetString()}");
                Console.WriteLine($"Text: {entity.GetProperty("text").GetString()}");
                Console.WriteLine($"Offset: {entity.GetProperty("offset").GetInt32()}");
                Console.WriteLine($"Length: {entity.GetProperty("length").GetInt32()}");
                Console.WriteLine($"Confidence: {entity.GetProperty("confidenceScore").GetSingle()}");
                Console.WriteLine();

                if (entity.TryGetProperty("resolutions", out JsonElement resolutions))
                {
                    foreach (JsonElement resolution in resolutions.EnumerateArray())
                    {
                        if (resolution.GetProperty("resolutionKind").GetString() == "DateTimeResolution")
                        {
                            Console.WriteLine($"Datetime Sub Kind: {resolution.GetProperty("dateTimeSubKind").GetString()}");
                            Console.WriteLine($"Timex: {resolution.GetProperty("timex").GetString()}");
                            Console.WriteLine($"Value: {resolution.GetProperty("value").GetString()}");
                            Console.WriteLine();
                        }
                    }
                }
            }
        }
    }
}
