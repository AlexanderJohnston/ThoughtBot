using Azure.AI.Language.Conversations.Authoring;
using Azure.AI.Language.Conversations;
using Azure.AI.TextAnalytics;
using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureLUIS
{
    public class GetAClue
    {
        public CognitiveLanguageUnderstanding Understanding;

        public GetAClue() => Understanding = new (CallCLU());

        private CognitiveServices CallCLU()
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
    }
}
