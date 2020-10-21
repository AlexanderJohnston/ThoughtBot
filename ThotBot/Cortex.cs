using Discord.Commands;
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
using ThotBot.Intent;
using ThotBot.Services;
using ThotBot.Skill;

namespace ThotBot
{
    public class Cortex
    {
        private bool _sleeping;
        private bool _quiet;
        private List<Intention> _intentions;
        private ShortTermMemory<string> _memory = new ShortTermMemory<string>();
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private ServiceProvider _provider;
        private string AllowedChannel = "general";

        public Cortex(DiscordSocketClient client, CommandService commands)
        {
            _commands = commands;
            _client = client;
        }

        public async Task ConfigureBotServices()
        {
            _client.MessageReceived += HandleMessageAsync;
            var plasticIntent = new PlasticIntentions();
            _intentions = await plasticIntent.DownloadIntentions();

            var provider = new ServiceCollection();
            provider.AddSingleton<SchedulerService>();
            _provider = provider.BuildServiceProvider();
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: _provider );
        }

        private async Task HandleMessageAsync(SocketMessage messageParam)
        {
            // Ignore bots.
            if (messageParam.Author.IsBot) return;

            // Skip system messages
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Dictate where the bot is allowed to operate.
            if (message.Content.Contains("move Thot here")) AllowedChannel = message.Channel.Name;

            // Restrict message handling to one channel.
            if (message.Channel.Name != AllowedChannel) return;

            // Check for wake and sleep messages
            if (message.Content.Contains("wake"))
            {
                if (_sleeping)
                {
                    message.Channel.SendMessageAsync("Waking up...");
                    _sleeping = false;
                }
            }
            if (_sleeping) return;
            if (message.Content.Contains("sleep"))
            {
                if (!_sleeping)
                {
                    message.Channel.SendMessageAsync("Going into low power mode...");
                    _sleeping = true;
                }
            }

            if (message.Content.Contains("quiet mode"))
            {
                _quiet = true;
            }
            if (message.Content.Contains("verbose mode"))
            {
                _quiet = false;
            }

            // Pass the message to the conversation handler for the first pass of hard-coded responses.
            int argPos = 0;
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
            {
                HandleConversation(messageParam);
            }

            // Predict the user's intention and then send it to short term memory.
            if (!_quiet)
            {
                var intention = await PredictIntention(messageParam);
                _memory.Remember(message.Content, message.Author, MemoryType.Command, message.Channel.Id, intention);
            }
        }

        private async Task<Intention> PredictIntention(SocketMessage message)
        {
            var responseEngine = new ResponsePredictionEngine(_intentions);
            var intent = await responseEngine.PredictAsync(message.Content);
            if (intent.Predicted.Name == "None")
            {
                return intent.Predicted;
            }
            var entities = JsonConvert.SerializeObject(intent.Entities);
            await message.Channel.SendMessageAsync(string.Format("I predict your intention is to {0} an entity list {1}.", intent.Predicted.Name, entities));
            return intent.Predicted;
        }

        private async Task HandleConversation(SocketMessage messageParam)
        {
            var message = messageParam.Content.ToLower();
            if (message.Contains("last memory"))
            {
                var memory = _memory.LastMemory(messageParam.Author);
                await messageParam.Channel.SendMessageAsync(memory);
                return;
            }
            if (message.Contains("everything you remember"))
            {
                var memories = _memory.AllMemories(messageParam.Author);
                var sb = new StringBuilder();
                foreach (var memory in memories)
                    sb.AppendLine(memory);
                await messageParam.Channel.SendMessageAsync(sb.ToString());
                return;
            }
            if (message.Contains("hello"))
            {
                await messageParam.Channel.SendMessageAsync("Good morning!");
                return;
            }
            if (message.Contains("wake"))
            {
                await messageParam.Channel.SendMessageAsync("Yes I am listening.");
                return;
            }
            if (message.Contains("sleep"))
            {
                await messageParam.Channel.SendMessageAsync(":sleepingcat:");
                return;
            }
            if (message.Contains("good bot"))
            {
                await messageParam.Channel.SendMessageAsync("Thank you.");
                return;
            }
            if (message.Contains("news"))
            {
                await messageParam.Channel.SendMessageAsync("The starting time for Thursday's business meeting has been moved to 9:30am instead of the usual 10:00am time slot.");
                return;
            }
            if (message.Contains("meeting"))
            {
                await messageParam.Channel.SendMessageAsync(@"https://dealeron.zoom.us/j/686507944");
                return;
            }
            if (message.Contains("everything you've learned"))
            {
                await messageParam.Channel.SendMessageAsync(JsonConvert.SerializeObject(_intentions));
            }
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
