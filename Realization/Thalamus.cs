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
using Memory.Conversation;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using PostSharp.Extensibility;

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
        private string AllowedChannel = "bottest";
        private bool _sleeping = true;
        private bool _quiet = true;
        private bool _imagine = false;
        private bool _tokenize = false;
        private List<Intention> _lastIntentions;
        private SocketUser _self;
        private GetAClue _cognition;
        private string _promptTemplate = "{0}";
        private DiskEmbedder _disk = new DiskEmbedder("longTerm-Memory-{0}.json");

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
            // Check if the message is from a user, otherwise disregard it.
            SocketUserMessage message;
            if (messageParam is SocketUserMessage)
            {
                message = (SocketUserMessage)messageParam;
            }
            else
            {
                return;
            }
            // Check if the message is from the bot or not. We don't want to listen to our own messages because they can be tracked internally.
            if (Attention.NotInterested(messageParam))
            {
                if (message.Author.Username == "Vertex Intelligence")
                {
                    _self = message.Author;
                }
                return;
            }
            // Tokenize the user's message and return to them the list of tokens using the Tokenizer from Memory
            if (_tokenize)
            {
                var tokenizer = new Tokenizer();
                var tokens = tokenizer.Tokenize(message.Content);
                var output = new StringBuilder();
                output.Append('[');
                foreach (var token in tokens)
                {
                    output.Append(token.ToString() + ',');
                }
                // remove the last comma
                output.Remove(output.Length - 1, 1);
                output.Append(']');
                await messageParam.Channel.SendMessageAsync(output.ToString());
                _tokenize = false;
                return;
            }
            // Store a new prompt for usage in GPT-3 response prediction.
            if (_imagine)
            {
                _promptTemplate = message.Content;
                _imagine = false;
                return;
            }
            // Handle deterministic commands like putting the bot to sleep or checking memory.
            var happened = await BasicCommands(message);
            if (happened || _sleeping)
                return;
            // Listen to incoming messages and respond to them.
            if (Listening())
            {
                await ReactToUserMessage(messageParam, message);
            }
        }

        private async Task ReactToUserMessage(SocketMessage messageParam, SocketUserMessage? message)
        {
            // Track the message with short term memory.
            Guid memId = ShortMemory(message);
            // Decide if the topic has shifted and needs to be updated.
            var currentTopic = RecallTopic(message);
            var topicShift = await DidTopicShift(messageParam, message, currentTopic);
            // Predict the intention of the user's message and then remember it.
            Intention prediction = await PredictIntentWithGpt3(message, memId, currentTopic, topicShift);
            // Prepare a prompt for GPT-3 to predict a response to the user's message, then attempt it.
            await TryRespondWithGpt3(messageParam, message, currentTopic, prediction);
        }

        /// <summary>
        ///   Predict the intention of the user's message and then remember it.
        /// </summary>
        /// <param name="message">message to analyze</param>
        /// <param name="memId">memory ID from short term memory</param>
        /// <param name="currentTopic">the current topic as a string</param>
        /// <param name="topicShift">the topic we're shifting to as a string</param>
        /// <returns></returns>
        private async Task<Intention> PredictIntentWithGpt3(SocketUserMessage? message, Guid memId, string currentTopic, string topicShift)
        {
            var prediction = await PredictIntent(message, topicShift, currentTopic);
            await RememberOther(message, memId, currentTopic, prediction);
            return prediction;
        }

        private async Task TryRespondWithGpt3(SocketMessage messageParam, SocketUserMessage? message, string currentTopic, Intention prediction)
        {
            var promptForGpt = WeavePromptToTemplate(messageParam.Channel.Id, _promptTemplate);
            await Respond(message, currentTopic, prediction, promptForGpt);
        }

        private bool Listening()
        {
            return !_quiet;
        }

        private async Task Respond(SocketUserMessage? message, string currentTopic, Intention prediction, string wovenRequest)
        {
            // Don't bother responding if a self isn't defined for sake of memory.
            if (_self != null)
            {
                var response = await Cortex.PredictResponse(message, wovenRequest);
                var botMemId = _memory.Remember(response, _self, MemoryType.FormulatedIntent, message.Channel.Id, prediction);
                await RememberSelf(currentTopic, prediction, botMemId);
            }
        }

        private async Task RememberSelf(string currentTopic, Intention prediction, Guid botMemId)
        {
            var incomingSignal = new AuditorySignal() { Context = currentTopic, MemoryId = botMemId, Source = _self.Id, Topic = prediction.Name };
            Auditory.Listen(incomingSignal);
        }

        private async Task RememberOther(SocketUserMessage? message, Guid memId, string currentTopic, Intention prediction)
        {
            var userSignal = new AuditorySignal() { Context = currentTopic, MemoryId = memId, Source = message.Author.Id, Topic = prediction.Name };
            Auditory.Listen(userSignal);
            if (currentTopic != prediction.Name)
            {
                await SaveConversation(message, userSignal);
            }
        }

        private async Task<Intention> PredictIntent(SocketMessage message, string topicShift, string currentTopic)
        {
            string gpt3Intent;
            if (topicShift.Contains("Yes"))
            {
                gpt3Intent = await Cortex.PredictGptIntent(message);
            }
            else
            {
                gpt3Intent = currentTopic;
            }
            Intention intent = new None(gpt3Intent);
            return intent;
        }

        private async Task<string> DidTopicShift(SocketMessage messageParam, SocketUserMessage? message, string currentTopic)
        {
            string topicShift;
            var wovenTopicRequest = WeavePromptBeforeResponse(messageParam.Channel.Id);
            topicShift = await Cortex.PredictTopicShift(message, currentTopic, wovenTopicRequest);
            return topicShift;
        }

        private string RecallTopic(SocketMessage messageParam)
        {
            var currentTopic = "None";
            if (Auditory.TopicExists(messageParam.Author.Id))
            {
                currentTopic = Auditory.TopicLookup.First(person => person.Key.Id == messageParam.Author.Id).Value;
            }
            return currentTopic;
        }
        private Guid ShortMemory(SocketUserMessage? message)
        {
            return _memory.Remember(message.Content, message.Author, MemoryType.LearnedSkill, message.Channel.Id, new None());
        }

        private async Task SaveConversation(SocketUserMessage message, AuditorySignal userSignal)
        {
            var conversation = Auditory.Dialogue.First(dialogue => Auditory.Conversation(dialogue).Topic == userSignal.Context).Value;
            var dynamicMemory = new
            {
                Conversation = conversation,
                WeavePrompt = WeavePromptBeforeResponse(message.Channel.Id)
            };
            var json = JsonConvert.SerializeObject(dynamicMemory);
            var topic = conversation.Topic;
            // Concatenate the list of contexts from the conversation.
            var context = string.Join(";", conversation.Context);
            var embed = await Cortex.EmbedMemory(json, message);
            embed.Topic = topic;
            embed.Context = context;
            var memoryOnDisk = _disk;
            var pastMemory = memoryOnDisk.ReadMemories();
            pastMemory.Add(embed);
            memoryOnDisk.WriteMemories(pastMemory);
            Log.Debug("Memory embedded. Model: {0} | Usage: {1}", embed.Embedding.Model, embed.Embedding.Usage);
        }

        public async Task<bool> BasicCommands(SocketUserMessage message)
        {
            // if message contains tokenize my next message and set the _tokenize flag to true
            if (message.Content.Contains("tokenize my next message"))
            {
                _tokenize = true;
                return true;
            }
            if (message.Content.Contains("Realize new prompt"))
            {
                _imagine = true;
                await message.Channel.SendMessageAsync("I will use the next message you send as the new template.");
                return true;
            }
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
                var json = JsonConvert.SerializeObject(WeavePromptBeforeResponse(message.Channel.Id));
                var embed = await Cortex.EmbedMemory(json, message);
                var memoryOnDisk = _disk;
                var pastMemory = memoryOnDisk.ReadMemories();
                //var pastMemory = new List<EmbeddedMemory>();
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

        public string WeavePromptToTemplate(ulong channelId, string template = "{0}")
        {
            var weaveDefaultPrompt = WeavePrompt(channelId);
            var customTemplate = string.Format(template, weaveDefaultPrompt);
            return customTemplate;
        }

        public string WeavePrompt(ulong channelId)
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
        public string WeavePromptBeforeResponse(ulong channelId)
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
            return sb.ToString();
        }
    }

}
