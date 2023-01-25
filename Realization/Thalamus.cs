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
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using PostSharp.Extensibility;
using System.Threading.Channels;
using Azure;
using Memory.Converse;
using static Totem.Timeline.FlowCall;
using System;
using Totem;
using Serilog.Events;
using Discord.Interactions;

namespace Realization
{
    public class Thalamus
    {
        //public VentralStream Auditory = new(); // TODO remove any vestiges of single-threaded system
        DiskEmbedder _disk = new DiskEmbedder("longTerm-Memory.json"); // TODO deprecate this
        ShortTermMemory<string> _memory = new ShortTermMemory<string>(); // TODO make a service to handle this separately
        GetAClue Cognition;
        ReticularSystem Attention;
        MultiTasking Auditory = new(); // Supports multiple threads and channels
        Cortex Cortex;
        AttentionSpan Focus = new(); // TODO put to use
        EmbeddingMemory GlobalLongMemory = new("longTerm-Memory.json"); // TODO swap to using this instead of DiskEmbedder
        LimbicSystem Limbic = new(); // TODO put to use
        SocketUser _self;
        bool _sleeping = true;
        bool _quiet = true;
        bool _imagine = false;
        bool _tokenize = false;
        bool _showMemory = false;
        string _promptTemplate = "{0}";
        string _openAIKey = File.ReadAllText(Environment.CurrentDirectory + "\\key.openAI");
        string _name = "Vertex Intelligence";
        string AllowedChannel = "bottest";
        readonly DiscordSocketClient _client;
        readonly CommandService _commands;
        IServiceProvider _services;


        public Thalamus(DiscordSocketClient client, CommandService commands, GetAClue cognition, IServiceProvider services)
        {
            _commands = commands;
            _client = client;
            Cognition = cognition;
            var plasticIntent = new PlasticIntentions();
            //_lastIntentions = plasticIntent.DownloadIntentions();
            Cortex = new Cortex(new List<Intention> { new None() }, Cognition, _openAIKey);
            Attention = new ReticularSystem(_memory);
            _services = services;
            _client.MessageReceived += HandleMessageAsync;
        }

        public async Task HandleMessageAsync(SocketMessage messageParam)
        {
            // Check if the message is from a user, otherwise disregard it.
            SocketUserMessage message;
            ITextChannel channel;
            if (messageParam is SocketUserMessage)
            {
                message = messageParam as SocketUserMessage;
                channel = message.Channel as ITextChannel;
            }
            else
            {
                // Message is from the system or something else.
                return;
            }
            // Check if the message is from the bot or not. We don't want to listen to our own messages because they can be tracked internally.
            if (Attention.NotInterested(messageParam))
            {
                CheckSelf(message);
                return;
            }
            // Decide what type of message this is.
            var channelType = CheckChannelType(messageParam);
            bool handled;
            if (channelType == typeof(IThreadChannel))
                handled = await HandleThreadChannel(messageParam, message);
            else if (channelType == typeof(ITextChannel))
                handled = await HandleTextChannel(messageParam, message);
            else
                handled = false;
            if (!handled)
            {
                // If the message was not handled, then it is a command and we should log the channel id, content, and user id
                Log.Verbose("Command was issued from {0} in channel {1} with content {2}", message.Author.Id, message.Channel.Id, message.Content);
            }
        }

        private void CheckSelf(SocketUserMessage message)
        {
            if (message.Author.Username == _name)
            {
                _self = message.Author;
            }
        }

        private Type CheckChannelType(SocketMessage messageParam)
        {
            Type messageType;
            if (messageParam.Channel is IGuildChannel)
            {
                // This is a Guild message. Decide which type.
                if (messageParam.Channel is ITextChannel)
                {
                    // This is a text channel, decide if thread or not.
                    if (messageParam.Channel is IThreadChannel)
                    {
                        messageType = typeof(IThreadChannel);
                    }
                    else
                    {
                        // This is a regular guild channel
                        messageType = typeof(ITextChannel);
                    }
                }
                else
                {
                    // This is a DM message or voice channel.
                    messageType = typeof(IGuildChannel);
                }
            }
            else
            {
                // Only other option is private channel.
                messageType = typeof(IPrivateChannel);
            }
            return messageType;
        }

        private async Task<bool> HandleTextChannel(SocketMessage messageParam, SocketUserMessage message)
        {
            // Check the current message to compare it against existing memories.
            if (_showMemory)
            {
                // Used thread id if available otherwise channel id
                //var contextId = message.
                //var embeddings = new EmbeddingMemory()
                var comparison = new MemoryComparison(_disk);
                var memories = _disk.ReadMemories();
                var prompt = await comparison.ConstructPrompt(message.Content, memories);
                await messageParam.Channel.SendMessageAsync(prompt);
                var response = await Cortex.PredictResponse(message, prompt);
                await messageParam.Channel.SendMessageAsync(response);
                _showMemory = false;
                return true;
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
                return true;
            }
            // Store a new prompt for usage in GPT-3 response prediction.
            if (_imagine)
            {
                _promptTemplate = message.Content;
                _imagine = false;
                return true;
            }
            // Handle deterministic commands like putting the bot to sleep or checking memory.
            var happened = await BasicCommands(message);
            if (happened || _sleeping)
                return true;
            return false;
        }

        private async Task<bool> HandleThreadChannel(SocketMessage messageParam, SocketUserMessage message)
        {
            // Listen to incoming messages and respond to them.
            if (Listening())
            {
                await ReactToUserMessage(messageParam, message);
                return true;
            }
            return false;
        }

        private async Task ReactToUserMessage(SocketMessage messageParam, SocketUserMessage? message)
        {            
            // Track the message with short term memory.
            Guid memId = ShortMemory(message);
            // Decide if the topic has shifted and needs to be updated.
            var currentTopic = RecallTopic(message);
            var topicShift = await DidTopicShift(messageParam, message, currentTopic);
            // Predict the intention of the user's message and then remember it.
            Intention prediction = await PredictIntentWithGpt3(message, memId, currentTopic, topicShift.Topic, topicShift.Decision);
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
        private async Task<Intention> PredictIntentWithGpt3(SocketUserMessage? message, Guid memId, string currentTopic, string topicShift, string decision)
        {
            var prediction = await PredictIntent(message, currentTopic, topicShift, decision);
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
                await RememberSelf(currentTopic, prediction, botMemId, response, message.Channel.Id);
            }
        }

        private async Task RememberSelf(string currentTopic, Intention prediction, Guid botMemId, string response, ulong channelId)
        {
            var incomingSignal = new AuditorySignal() { Context = currentTopic, MemoryId = botMemId, Source = _self.Id, Topic = prediction.Name , Text = response, Channel = channelId };
            Auditory.ListenChannel(incomingSignal);
        }

        private async Task RememberOther(SocketUserMessage? message, Guid memId, string currentTopic, Intention prediction)
        {
            var userSignal = new AuditorySignal() { Context = currentTopic, MemoryId = memId, Source = message.Author.Id, Topic = prediction.Name, Text = message.Content, Channel = message.Channel.Id };
            Auditory.ListenChannel(userSignal);
            if (currentTopic != prediction.Name)
            {
                await SaveConversation(message, userSignal, currentTopic); // TODO Currently fails to find the first topic unknown and then carries it onward.
            }
        }

        private async Task<Intention> PredictIntent(SocketMessage message, string currentTopic, string topicShift, string decision)
        {
            string gpt3Intent;
            if (decision.Contains("Yes"))
            {
                //gpt3Intent = await Cortex.PredictGptIntent(message);
                gpt3Intent = topicShift;
            }
            else
            {
                gpt3Intent = currentTopic;
            }
            Intention intent = new None(gpt3Intent);
            return intent;
        }
        
        private async Task<TopicShiftAnswer> DidTopicShift(SocketMessage messageParam, SocketUserMessage? message, string currentTopic)
        {
            var wovenTopicRequest = WeavePromptBeforeResponse(messageParam.Channel.Id);
            var topicShift = await Cortex.PredictTopicShift(message, currentTopic, wovenTopicRequest);
            return topicShift;
        }

        /// <summary>
        /// The RecallTopic method appears to be checking if a "ventral stream" object exists for the channel where the message was sent. 
        /// A "ventral stream" is a type of memory system that tracks the topics of conversation in a channel. If a ventral stream object 
        /// exists for the channel, the method checks if the topic for the user who sent the message is stored in the ventral stream's 
        /// "topic lookup" dictionary. If it is, the method sets the currentTopic variable to the stored topic. If not, the currentTopic 
        /// variable is set to a default value of "Topic is not yet known." The method returns the currentTopic variable.
        /// </summary>
        /// <param name="messageParam"></param>
        /// <returns></returns>
        private string RecallTopic(SocketMessage messageParam)
        { // TODO this is breaking topics
            var currentTopic = "Topic is not yet known.";
            VentralStream ventral;
            if (Auditory.Channels.ContainsKey(messageParam.Channel.Id))
            {
                ventral = Auditory.Channels[messageParam.Channel.Id];
                if (ventral.TopicExists(messageParam.Author.Id))
                {
                    currentTopic = ventral.TopicLookup.First(person => person.Key.Id == messageParam.Author.Id).Value;
                }
            }
            return currentTopic;
        }
        
        /// <summary>
        /// The first step in the ReactToUserMessage method is to track the message using short term memory by storing information
        /// about it, including its content, author, location in a channel, type, intention, and related context, in a memory tracking
        /// system.This system creates a new memory object for the message and associates it with the user and channel it came from.
        /// The memory object is given a globally unique identifier(Guid) which can be used to identify it in the memory system.The
        /// method returns this Guid so that it can be used later in the process.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private Guid ShortMemory(SocketUserMessage? message)
        {
            return _memory.Remember(message.Content, message.Author, MemoryType.LearnedSkill, message.Channel.Id, new None());
        }

        private async Task SaveConversation(SocketUserMessage message, AuditorySignal userSignal, string currentTopic)
        {
            VentralStream ventral;
            Conversation conversation;
            try
            {
                if (Auditory.Channels.ContainsKey(message.Channel.Id))
                {
                    ventral = Auditory.Channels[message.Channel.Id];
                    conversation = ventral.GetConversation(currentTopic);
                }
                else
                {
                    Log.Error("Failed to find a conversation for {0}", currentTopic);
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to find a conversation for {0}", currentTopic);
                return;
            }
            var memories = _memory.AllMemories(message.Channel.Id).ToList();
            //var dynamicMemory = memories.Select(x => $"{x.Context.Author}: {x.Value}").ToList(); 
            //var json = JsonConvert.SerializeObject(conversation);
            var topic = userSignal.Topic;
            // Concatenate the list of contexts from the conversation.
            var context = string.Join("\n*", userSignal.Context);
            var embed = await Cortex.EmbedMemory(conversation, message, topic, context);
            embed.Topic = topic;
            embed.Context = context;
            var memoryOnDisk = _disk;
            var pastMemory = memoryOnDisk.ReadMemories();
            pastMemory.Add(embed);
            memoryOnDisk.WriteMemories(pastMemory);
            _memory.WipeAll(message.Channel.Id);
            Log.Debug("Memory embedded. Model: {0} | Usage: {1}", embed.Embedding.Model, embed.Embedding.Usage);
        }

        public async Task<bool> BasicCommands(SocketUserMessage message)
        {
            // provide most relevant memories
            if (message.Content.Contains("what do you remember about my next message"))
            {
                _showMemory = true;
                return true;
            }
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
                Cortex = new Cortex(new List<Intention> { new None() }, Cognition, _openAIKey);
                Attention = new ReticularSystem(_memory);
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
                var dialogue = JsonConvert.SerializeObject(Auditory.Channels.FirstOrDefault(chan => chan.Key == messageParam.Channel.Id).Value.Dialogue);
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
