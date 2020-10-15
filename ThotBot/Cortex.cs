using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
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

            // Pass the message to the conversation handler for the first pass of hard-coded responses.
            int argPos = 0;
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
            {
                HandleConversation(messageParam);
            }

            // Predict the user's intention and then send it to short term memory.
            var intention = await PredictIntention(messageParam);
            _memory.Remember(message.Content, message.Author, MemoryType.Command, message.Channel.Id, intention);
        }

        private async Task<Intention> PredictIntention(SocketMessage message)
        {
            var intentions = new List<Intention>() { new Confirm(), new Greeting(), new JoinMeeting(), new JoinStatic(), new LinkResource(), new News(), new None(), new Praise(), new Recall(), new Remember(), new ScheduleEvent(), new Sleep(), new Wake() };
            var responseEngine = new ResponsePredictionEngine(intentions);
            var intent = await responseEngine.PredictAsync(message.Content);
            await message.Channel.SendMessageAsync(string.Format("I predict your intention is: {0}.", intent.Name));
            return intent;
        }

        private async Task HandleConversation(SocketMessage messageParam)
        {
            var message = messageParam.Content.ToLower();
            if (message.Contains("stack overflow"))
            {
                await messageParam.Channel.SendMessageAsync(@"https://stackoverflow.com/c/dealeron-dev/");
                return;
            }
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
                await messageParam.Channel.SendMessageAsync("Going to sleep.");
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
