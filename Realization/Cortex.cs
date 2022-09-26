using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Realization.Intent;
using Realization.Learning;
using Realization.Services;
using Realization.Skill;
using ThotLibrary;

namespace Realization
{
    public class Cortex
    {
        public List<Intention> Intentions;

        public Cortex(List<Intention> intentions)
        {
            Intentions = intentions;
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
            return response;
        }
    }
}
