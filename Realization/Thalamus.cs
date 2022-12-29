using AzureLUIS;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Realization.Skill;
using System.Reflection;
using System.Text;
using ThotLibrary;
using Memory;
using Memory.Intent;

namespace Realization
{
    public class Thalamus
    {
        public AttentionSpan Focus = new();
        public EmbeddingMemory LongMemory = new();
        public LimbicSystem Limbic = new();
        public VentralStream Auditory = new();
        public Cortex Cortex;
        public ReticularSystem Attention;
        private ShortTermMemory<string> _memory = new ShortTermMemory<string>();
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private ServiceProvider _provider;
        private string AllowedChannel = "auto-intelligence";
        private bool _sleeping = true;
        private bool _quiet = true;
        private bool _imagine = false;
        private List<Intention> _lastIntentions;
        private SocketUser _self;
        private GetAClue _cognition;

        public Thalamus(DiscordSocketClient client, CommandService commands, GetAClue cognition)
        {
            _commands = commands;
            _client = client;
            _cognition = cognition;
            var plasticIntent = new PlasticIntentions();
            _lastIntentions = plasticIntent.DownloadIntentions();
            Cortex = new Cortex(_lastIntentions, _cognition);
            Attention = new ReticularSystem(_memory);
        }

        public async Task ConfigureBotServices()
        {
            _client.MessageReceived += HandleMessageAsync;
            var provider = new ServiceCollection();
            //provider.AddSingleton<SchedulerService>();
            _provider = provider.BuildServiceProvider();
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: _provider);
        }

        public async Task HandleMessageAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (Attention.NotInterested(messageParam))
            {
                if (message.Author.Username == "Vertex Intelligence")
                {
                    _self = message.Author;
                }
                return;
            }

            // Handle deterministic commands like putting the bot to sleep or checking memory.
            var happened = await BasicCommands(message);
            if (happened || _sleeping)
                return;

            if (_imagine)
            {

            }

            // Predict the user's intention and then send it to short term memory.
            if (!_quiet)
            {
                var memId = _memory.Remember(message.Content, message.Author, MemoryType.LearnedSkill, message.Channel.Id, new None());
                // Decide if the topic has shifted and needs to be updated.
                string currentTopic;
                string topicShift;
                var wovenTopicRequest = Wove(messageParam.Channel.Id);
                if (Auditory.TopicExists(message.Author.Id))
                {
                    currentTopic = Auditory.TopicLookup.First(person => person.Key.Id == message.Author.Id).Value;
                    topicShift = await Cortex.PredictTopicShift(message, currentTopic, wovenTopicRequest);
                }
                else
                {
                    currentTopic = "";
                    topicShift = await Cortex.PredictTopicShift(message, currentTopic, wovenTopicRequest);
                }
                string gpt3Intent;
                Intention prediction;
                AuditorySignal userSignal;
                if (topicShift.Contains("Yes"))
                {
                    gpt3Intent = await Cortex.PredictGptIntent(message);
                    prediction = new None(gpt3Intent);
                    userSignal = new AuditorySignal() { Context = currentTopic, MemoryId = memId, Source = message.Author.Id, Topic = prediction.Name };
                }
                else
                {
                    gpt3Intent = currentTopic;
                    prediction = new None(gpt3Intent);
                    userSignal = new AuditorySignal() { Context = string.Empty, MemoryId = memId, Source = message.Author.Id, Topic = prediction.Name };
                }
                Auditory.Listen(userSignal);
                var wovenRequest = Wove(messageParam.Channel.Id);
                //var wovenRequest = Weave(messageParam.Channel.Id, messageParam.Author.Username, message.Content);
                var response = await Cortex.PredictResponse(message, wovenRequest);
                if (_self != null)
                {
                    var botMemId = _memory.Remember(response, _self, MemoryType.FormulatedIntent, message.Channel.Id, prediction);
                    var incomingSignal = new AuditorySignal() { Context = gpt3Intent, MemoryId = botMemId, Source = _self.Id, Topic = prediction.Name };
                    Auditory.Listen(incomingSignal);
                }



                // ----- unfinished layer ------

                //if (_memory.Remembers(message.Author.DiscriminatorValue))
                //{
                //    lastMemory = _memory.MemoryBeforeLast(message.Author);
                //}
                //else
                //{
                //    lastMemory = new Skill.Memory<string>() { Value = string.Empty, Context = default(MemoryContext) };
                //}
                //if (intention.Name == "Teach")
                //{
                //    _remember = message.Author;
                //    await message.Channel.SendMessageAsync("Tell me the name of this new command.");
                //}
                //if (intention.Name == "Confirm")
                //{
                //    var focusedMemory = _memory.GetFocusedMemory(message.Author);
                //    //var lastMemory = _memory.MemoryBeforeLast(message.Author);
                //    if (lastMemory.Context.Intention.Name == "Teach")
                //    {
                //        var rnd = new Random();
                //        string[] acknowledge = new[] { "Understood!", "Okay,", "Cool~", "Got it," };
                //        var acknowledgement = acknowledge[rnd.Next(0, 4)];
                //        await message.Channel.SendMessageAsync($"{acknowledgement} I will remember {focusedMemory}.");
                //    }

                //}
                //if (intention.Name == "Sing")
                //{
                //    var responseEngine = new ResponsePredictionEngine(_intentions);
                //    var index = message.Content.IndexOf(intention.Name);
                //    var song = message.Content.Substring(index + 5, message.Content.Length - 5);
                //    var emotion = await responseEngine.PredictAsync(song);
                //    await message.Channel.SendMessageAsync(string.Format("Verse: {0}", emotion.Predicted.Name));
                //}
            }
        }

        public async Task<bool> BasicCommands(SocketUserMessage message)
        {
            // Dictate where the bot is allowed to operate.
            if (message.Content.Contains("move Thought here"))
            {
                AllowedChannel = message.Channel.Name;
                return true;
            }
            // Go into prototyping mode.
            if (message.Content.Contains("imagine dragons"))
            {
                _imagine = true;
                return true;
            }
            // Restrict message handling to one channel.
            if (message.Channel.Name != AllowedChannel) return true;

            // Check for wake and sleep messages
            if (message.Content.Contains("power up"))
            {
                if (_sleeping)
                {
                    await message.Channel.SendMessageAsync("Waking up...");
                    _sleeping = false;
                    return true;
                }
            }
            if (_sleeping) return false;
            if (message.Content.Contains("go to sleep"))
            {
                if (!_sleeping)
                {
                    await message.Channel.SendMessageAsync("Going into low power mode...");
                    _sleeping = true;
                    return true;
                }
            }

            if (message.Content.Contains("quiet mode"))
            {
                _quiet = true;
                return true;
            }
            if (message.Content.Contains("verbose mode"))
            {
                _quiet = false;
                return true;
            }
            if (message.Content.Contains("reset the thalamus"))
            {
                Limbic = new();
                Auditory = new();
                _memory = new ShortTermMemory<string>();
                _sleeping = true;
                _quiet = true;
                Cortex = new Cortex(_lastIntentions, _cognition);
                Attention = new ReticularSystem(_memory);
            }
            if (message.Content.Contains("save this conversation"))
            {
                var json = JsonConvert.SerializeObject(_memory.AllMemories(message.Channel.Id));
                var embed = await Cortex.EmbedMemory(message);
                var memoryOnDisk = new DiskEmbedder("longTerm-Memory2.json");
                //var pastMemory = memoryOnDisk.ReadMemories();
                var pastMemory = new List<EmbeddedMemory>();
                pastMemory.Add(embed);
                memoryOnDisk.WriteMemories(pastMemory);
                Log.Debug("Memory embedded. Model: {0} | Usage: {1}", embed.Embedding.Model, embed.Embedding.Usage);
            }

            // Pass the message to the conversation handler for the first pass of hard-coded responses.
            int argPos = 0;
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
            {
                var happened = await DeterministicResponse(message);
                return happened;
            }
            return false;
        }
        public async Task<bool> DeterministicResponse(IMessage messageParam)
        {
            var message = messageParam.Content.ToLower();
            if (message.Contains("last memory"))
            {
                var memory = _memory.LastMemory(messageParam.Author);
                await messageParam.Channel.SendMessageAsync(memory);
                return true;
            }
            if (message.Contains("all dialogue"))
            {
                var dialogue = JsonConvert.SerializeObject(Auditory.Dialogue);
                await messageParam.Channel.SendMessageAsync(dialogue);

                return true;
            }
            if (message.Contains("everything you remember"))
            {
                var memories = _memory.AllMemories(messageParam.Channel.Id);
                var sb = new StringBuilder();
                foreach (var memory in memories)
                {
                    sb.Append((string)memory.Context.Author);
                    sb.Append(": ");
                    sb.Append((string)(memory.Value.Trim()));
                    sb.AppendLine();
                }
                await messageParam.Channel.SendMessageAsync(sb.ToString());
                return true;
            }
            if (message.Contains("now wake up"))
            {
                await messageParam.Channel.SendMessageAsync("Yes I am listening.");
                return true;
            }
            if (message.Contains("good bot"))
            {
                await messageParam.Channel.SendMessageAsync("Thank you.");
                return true;
            }
            if (message.Contains("everything you've learned"))
            {
                await messageParam.Channel.SendMessageAsync(JsonConvert.SerializeObject(Cortex.Intentions));
                return true;
            }
            return false;
        }
        public string Weave(ulong channelId, string author, string message)
        {
            var memories = _memory.AllMemories(channelId);
            var sb = new StringBuilder();
            foreach (var memory in memories)
            {
                sb.Append((string)memory.Context.Author);
                sb.Append(": ");
                sb.Append((string)(memory.Value.Trim()));
                sb.AppendLine();
            }
            sb.Append(author);
            sb.Append(": ");
            sb.Append((string)(message.Trim()));
            sb.AppendLine();
            return sb.ToString();
        }

        public string Wove(ulong channelId)
        {
            var memories = _memory.AllMemories(channelId);
            var sb = new StringBuilder();
            foreach (var memory in memories)
            {
                sb.Append((string)memory.Context.Author);
                sb.Append(": ");
                sb.Append((string)(memory.Value.Trim()));
                sb.AppendLine();
            }
            sb.Append(_self.Username);
            sb.Append(": ");
            return sb.ToString();
        }
    }

}
