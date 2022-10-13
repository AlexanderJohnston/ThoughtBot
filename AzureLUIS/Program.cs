using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Authoring = Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using AuthorCredentials = Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.ApiKeyServiceClientCredentials;
using Realization.Services;
using Azure;
using Azure.AI.Language.Conversations;
using Azure.AI.Language.Conversations.Authoring;
using AzureLUIS;
using Azure.AI.TextAnalytics;

LUISAuthoringClient Client(string endpoint, AuthorCredentials credentials) =>
    new LUISAuthoringClient(credentials) { Endpoint = endpoint };
AuthorCredentials AuthorCredentials(string key) => new AuthorCredentials(key);
LUISAuthoringClient LUIS(string key, string endpoint) => 
    Client(endpoint, AuthorCredentials(key));

CognitiveServices CallToCLU()
{
    var key = File.ReadAllText(Environment.CurrentDirectory + "\\key");
    Uri endpoint = new Uri("https://realize.cognitiveservices.azure.com");
    AzureKeyCredential credential = new AzureKeyCredential(key);

    ConversationAnalysisClient analyzer = new ConversationAnalysisClient(endpoint, credential);
    ConversationAuthoringClient author = new ConversationAuthoringClient(endpoint, credential);
    TextAnalyticsClient textAnalytics = new TextAnalyticsClient(endpoint, credential);
    return new CognitiveServices(analyzer, author, textAnalytics);
    // https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/cognitivelanguage/Azure.AI.Language.Conversations/README.md
}
void LegacyLUIS()
{
    var key = "PASTE_YOUR_LUIS_AUTHORING_SUBSCRIPTION_KEY_HERE";
    var authoringEndpoint = "PASTE_YOUR_LUIS_AUTHORING_ENDPOINT_HERE";
    var predictionEndpoint = "PASTE_YOUR_LUIS_PREDICTION_ENDPOINT_HERE";
    var appName = "Realization";
    var versionId = "0.0.1"; // Released.Major.Minor or Published.Trained.Composed
    var intentName = "TeachIntent"; // The most basic intent requried here for now
    var http = new HttpClient();
    var client = new LuisClient(http);
}


#region Cognitive Language Understanding
// Get the clients, subscription, and resource
var cognition = CallToCLU();

// Analyze Language
await TestBatch(cognition);
Console.ReadLine();
//DetectLanguage(cognition);

async Task TestBatch(CognitiveServices cognition)
{
    var convo = new SampleConversation(cognition);
    await convo.AsyncBatch();
}

void DetectLanguage(CognitiveServices cognition)
{
    var convo = new SampleConversation(cognition);
    convo.Respond(convo.Hydrate());
    convo.RespondEmail(convo.HydrateEmail());
    convo.RespondComplaints(convo.HydrateComplaints());
    convo.RecognizeEntities(convo.HydrateEntities());
}

// Analyze Sentiment
GaugeSentiment(cognition);

void GaugeSentiment(CognitiveServices cognition)
{
    throw new NotImplementedException();
}

// Extract Key Phrases
ExtractPhrases(cognition);

void ExtractPhrases(CognitiveServices cognition)
{
    throw new NotImplementedException();
}

// Extract Named Entities
ExtractEntities(cognition);

void ExtractEntities(CognitiveServices cognition)
{
    throw new NotImplementedException();
}

// Find Linked Entities
FindLinks(cognition);

void FindLinks(CognitiveServices cognition)
{
    throw new NotImplementedException();
}
public class CognitiveServices
{
    public ConversationAnalysisClient Analysis;
    public ConversationAuthoringClient Author;
    public TextAnalyticsClient TextAnalyzer;

    public CognitiveServices(ConversationAnalysisClient analysis, ConversationAuthoringClient author, TextAnalyticsClient textClient)
    {
        Analysis = analysis;
        Author = author;
        TextAnalyzer = textClient;
    }
}
#endregion







