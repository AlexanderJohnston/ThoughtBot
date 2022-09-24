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


        //public bool Learning(Skill.Memory<string> memory) => memory.Context.Intention.Name == "Teach";

        //public async Task RequestTraining(SocketUserMessage message)
        //{
        //    _memory.SaveFocusedMemory(message.Author, message.Content);
        //    _remember = null;
        //    await message.Channel.SendMessageAsync($"You want me to learn {message.Content}?");
        //    return;
        //}



        public async Task<PredictedIntent> PredictIntention(IMessage message)
        {
            var responseEngine = new ResponsePredictionEngine(Intentions);
            var intent = await responseEngine.PredictAsync(message.Content);
            var formulatedJson = JsonConvert.SerializeObject(intent);
            if (intent.Predicted.Name == "None")
            {
                return new PredictedIntent(new None(), formulatedJson);
            }
            //var entities = JsonConvert.SerializeObject(intent.Entities);
            //await message.Channel.SendMessageAsync(string.Format("I predict your intention is to {0} an entity list {1}.", intent.Predicted.Name, entities));
            //await message.Channel.SendMessageAsync(string.Format("I predict your intention is {0}", intent.Predicted.Name));
            
            return new PredictedIntent(intent.Predicted, formulatedJson);
        }

        public async Task<string> PredictResponse(IMessage message, string content)
        {
            var responseEngine = new ResponsePredictionEngine(Intentions);
            var response = await responseEngine.PredictResponse(content);
            await message.Channel.SendMessageAsync(response);
            return response;
        }

        

        private void ToBeImplemented()
        {
            // Create a WebSocket-based command context based on the message
            //var context = new SocketCommandContext(_client, message);
            //var result = await _commands.ExecuteAsync(
            //    context: context,
            //    argPos: argPos,
            //    services: _provider);

            //if (!result.IsSuccess)
            //    await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
