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
using ThotBot.Learning;
using ThotBot.Services;
using ThotBot.Skill;

namespace ThotBot
{
    public class Cortex
    {
        private bool _sleeping = true;
        private bool _quiet = true;
        private List<Intention> _intentions;
        private ShortTermMemory<string> _memory = new ShortTermMemory<string>();
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private ServiceProvider _provider;
        private string AllowedChannel = "coding-voice-text";

        private SocketUser _remember = null;

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

            if(_remember != null && _remember == message.Author)
            {
                var lastMemory = _memory.LastMemoryObject(message.Author);
                if (lastMemory.Context.Intention.Name == "Teach")
                {
                    _memory.SaveFocusedMemory(message.Author, message.Content);
                    _remember = null;
                    await message.Channel.SendMessageAsync($"You want me to learn {message.Content}?");
                    return;
                }
            }

            // Dictate where the bot is allowed to operate.
            if (message.Content.Contains("move Thot here"))
            {
                AllowedChannel = message.Channel.Name;
                return;
            }

            // Restrict message handling to one channel.
            if (message.Channel.Name != AllowedChannel) return;

            // Check for wake and sleep messages
            if (message.Content.Contains("wake"))
            {
                if (_sleeping)
                {
                    await message.Channel.SendMessageAsync("Waking up...");
                    _sleeping = false;
                    return;
                }
            }
            if (_sleeping) return;
            if (message.Content.Contains("sleep"))
            {
                if (!_sleeping)
                {
                    await message.Channel.SendMessageAsync("Going into low power mode...");
                    _sleeping = true;
                    return;
                }
            }

            if (message.Content.Contains("quiet mode"))
            {
                _quiet = true;
                return;
            }
            if (message.Content.Contains("verbose mode"))
            {
                _quiet = false;
                return;
            }

            // Pass the message to the conversation handler for the first pass of hard-coded responses.
            int argPos = 0;
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
            {
                await HandleConversation(messageParam);
            }

            // Predict the user's intention and then send it to short term memory.
            if (!_quiet)
            {
                var intention = await PredictIntention(messageParam);
                if (intention.Name == "Teach")
                {
                    _remember = message.Author;
                    await message.Channel.SendMessageAsync("Tell me the name of this new command.");
                }
                if (intention.Name == "Confirm")
                {
                    var focusedMemory = _memory.GetFocusedMemory(message.Author);
                    var lastMemory = _memory.MemoryBeforeLast(message.Author);
                    if (lastMemory.Context.Intention.Name == "Teach")
                    {
                        var rnd = new Random();
                        string[] acknowledge = new[] { "Understood!", "Okay,", "Cool~", "Got it," };
                        var acknowledgement = acknowledge[rnd.Next(0, 4)];
                        await message.Channel.SendMessageAsync($"{acknowledgement} I will remember {focusedMemory}.");
                    }
                }
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
            var formulatedJson = JsonConvert.SerializeObject(intent);
            _memory.Remember(formulatedJson, message.Author, MemoryType.FormulatedIntent, message.Channel.Id, intent.Predicted);
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
                await messageParam.Channel.SendMessageAsync("<:sleepingcat:710491923512819783>");
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
