using AzureLUIS;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Realization.Skill;
using System.Reflection;
using System.Text;
using Memory;
using Memory;
using Memory.Intent;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using PostSharp.Extensibility;
using System.Threading.Channels;
using Azure;
using Memory.Converse;
using Prompter;
using Serilog.Events;
using Discord.Interactions;
using Realization.Perception;
using Realization.Global;
using Memory.Chat;
using Weaviation;

namespace Realization
{
    // This coordinates all information and data flow through the bot. All interactions end up here before a response is given.
    // Please, excuse the mess.
    public class Thalamus
    {
        GetAClue Cognition; // Currently disabled.
        ShortTermMemory<string> _memory = new ShortTermMemory<string>(); // Stores plaintext conversational memory for the bot.
        EmbeddingMemory GlobalLongMemory = new("longTerm-Memory.json"); // Stores and retrieves embedded memories from disk as JSON.
        ReticularSystem Attention; // Decides if a message is worth responding to.
        MultiTasking Auditory = new(); // Supports multiple threads and channels.
        Cortex Cortex; // Handles all calls to the OpenAI API and Microsoft API.
        SocketUser _self; // The bot's user object.
        ThreadWeaver _weaver = new ThreadWeaver(); // Weaves together every prompt for GPT to respond to dynamically.
        InteractionCreatedHandler _interactionHandler; // Handles Discord interaction events.
        PyClient _weaviateClient; // Handles Weaviate API calls.
        bool _sleeping = true; // If true, the bot will not respond to commands in channels.
        bool _quiet = false; // If true, the bot will not respond to threads.
        bool _tokenize = false; // Returns next message's tokens for testing purposes.
        bool _showMemory = false; // Shows bot's memory for testing purposes.
        string _openAIKey = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "key.openAI")); // Key stored separetly on disk to avoid accidentally checking it into source.
        string _name = "Realization"; // Bot's name
        string AllowedChannel = "thoughtbot"; // Default channel the bot is allowed to operate in.
        readonly DiscordSocketClient _client;
        readonly CommandService _commands; // Unused
        IServiceProvider _services;        // Unused


        public Thalamus(DiscordSocketClient client, CommandService commands, GetAClue cognition, IServiceProvider services)
        {
            _commands = commands;
            _client = client;
            Cognition = cognition;
            _weaviateClient = new("http://localhost:5000");

            // Required for CLU, this is basically just a shim for now.
            var plasticIntent = new PlasticIntentions();
            Cortex = new Cortex(new List<Intention> { new None() }, Cognition, _openAIKey);
            _interactionHandler = new InteractionCreatedHandler(_weaver, Cortex);
            Attention = new ReticularSystem(_memory);
            _services = services;

            // Set up Discord response handlers
            _client.MessageReceived += HandleMessageAsync;
            _client.ButtonExecuted += _interactionHandler.MyButtonHandler;
            _client.SelectMenuExecuted += _interactionHandler.MyMenuHandler;
            _client.ModalSubmitted += _interactionHandler.MyModalHandler;
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
                handled = await HandleThreadChannel(messageParam, message, (IThreadChannel)channel);
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
            // Handle deterministic commands like putting the bot to sleep or checking memory.
            var happened = await BasicCommands(message);
            if (happened || _sleeping)
                return true;
            return false;
        }

        private async Task<bool> HandleThreadChannel(SocketMessage messageParam, SocketUserMessage message, IThreadChannel channel)
        {
            // This may be a new channel, so we need to create a new thread for it. TODO deprecate this, handled by interactionhandler
            if (!_weaver.Exists(messageParam.Channel.Id))
            {
                _weaver.NewThread(messageParam.Channel.Id);
            }

            // Listen to incoming messages and respond to them.
            if (Listening())
            {
                ulong userId = message.Author.Id;
                GlobalLongMemory.LoadMemories(channel.Id);
                await ReactToUserMessage(messageParam, message, channel.Id);
                return true;
            }
            return false;
        }

        private async Task ReactToUserMessage(SocketMessage messageParam, SocketUserMessage? message, ulong channelId)
        {
            // Track the message with short term memory.
            Guid memId = ShortMemory(message);
            // Save the message to long term memory.
            var embeddedMemory = await Cortex.EmbedMemory(messageParam);
            // Announce to channel that you are attempting to call weaviate client.
            await messageParam.Channel.SendMessageAsync("Calling Weaviate...");
            var weaviationMessage = new EmbeddedMessage(embeddedMemory.Memory.Message, channelId, messageParam.Author.Id, messageParam.Id);
            await _weaviateClient.SendMessage(weaviationMessage);
            await TryRespondWithLoom(message, memId, embeddedMemory, channelId);
        }

        /// <summary>
        /// Uses the new <see cref="Loom"/> class to weave the prompt using <see cref="Loom.GeneratePrompt"/> rather than the old <see cref="WeavePromptToTemplate"/> method.
        /// This will require the Loom to be properly spun up first by adding the variables to the <see cref="Loom"/> class by calling the appropriate child methods.
        /// This includes Conversation, Memories, and the current Instructions for System on the bot.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        private async Task TryRespondWithLoom(SocketUserMessage? message, Guid memId, EmbeddedMemory embedding, ulong channelId)
        {
            var thread = channelId;
            var instructions = Instructor.GetInstruction(thread);
            _weaver.ThreadInstructions(instructions, thread);
            _weaver.ThreadConversation(embedding.Memory.ToString(), thread, "user");
            var comparison = new MemoryComparison();
            var memories = GlobalLongMemory.Memories;
            foreach (var memory in GlobalLongMemory.Global)
            {
                if (!memories.Any(mem => mem.Context == memory.Context))
                {
                    memories.Add(memory);
                }

            }
            var context = MemoryComparison.GetContextualMemory(memories);
            var relevantMemories = await comparison.OrderMemorySectionsByQuerySimilarity(embedding, context);
            var topMemories = relevantMemories.Take(5).Select(x => x);
            _weaver.ThreadMemories(memories, thread);
            if (instructions == "opt-chat")
            {
                var chatForGpt = _weaver.GenerateChatFor(thread);
                await RememberOther(message, memId, "Memory", new None("Thread"), embedding);
                await Respond(message, chatForGpt);
            }
            else
            {
                var chatForGpt = _weaver.GenerateChatFor(thread);
                await RememberOther(message, memId, "Memory", new None("Thread"), embedding);
                await Respond(message, chatForGpt, "gpt-4-32k");
                //var promptForGpt = _weaver.GeneratePromptFor(thread);
                //await RememberOther(message, memId, "Memory", new None("Thread"), embedding);
                //await Respond(message, promptForGpt);
            }
        }

        private bool Listening()
        {
            return !_quiet;
        }

        // This handles GPT-3 responses with Davinci using the Loom weaving system.
        private async Task Respond(SocketUserMessage? message, string wovenRequest)
        {
            // Don't bother responding if a self isn't defined for sake of memory.
            if (_self != null)
            {
                var response = await Cortex.PredictResponse(message, wovenRequest);
                _weaver.ThreadConversation(response, message.Channel.Id, "assistant");
                var botMemId = _memory.Remember(response, _self, MemoryType.FormulatedIntent, message.Channel.Id);
                await RememberSelf(botMemId, response, message.Channel.Id);
            }
        }

        // This handles Chat-GPT responses using the Loom weaving system.
        private async Task Respond(SocketUserMessage? message, List<GptChatMessage> chatHistory)
        {
            // Don't bother responding if a self isn't defined for sake of memory.
            if (_self != null)
            {
                var response = await Cortex.PredictResponse(message, chatHistory);
                _weaver.ThreadConversation(response, message.Channel.Id, "assistant");
                var botMemId = _memory.Remember(response, _self, MemoryType.FormulatedIntent, message.Channel.Id);
                await RememberSelf(botMemId, response, message.Channel.Id);
            }
        }

        // This handles Chat-GPT responses using the Loom weaving system.
        private async Task Respond(SocketUserMessage? message, List<GptChatMessage> chatHistory, string model)
        {
            // Don't bother responding if a self isn't defined for sake of memory.
            if (_self != null)
            {
                var response = await Cortex.PredictResponse(message, chatHistory, model);
                _weaver.ThreadConversation(response, message.Channel.Id, "assistant");
                var botMemId = _memory.Remember(response, _self, MemoryType.FormulatedIntent, message.Channel.Id);
                await RememberSelf(botMemId, response, message.Channel.Id);
            }
        }

        private async Task RememberSelf(Guid botMemId, string response, ulong channelId)
        {
            var incomingSignal = new AuditorySignal() { Context = string.Empty, MemoryId = botMemId, Source = _self.Id, Topic = string.Empty, Text = response, Channel = channelId };
            Auditory.ListenThread(incomingSignal);
        }

        private async Task RememberOther(SocketUserMessage? message, Guid memId, string currentTopic, Intention prediction, EmbeddedMemory embeddding)
        {
            var userSignal = new AuditorySignal() { Context = currentTopic, MemoryId = memId, Source = message.Author.Id, Topic = prediction.Name, Text = message.Content, Channel = message.Channel.Id };
            Auditory.ListenThread(userSignal);
            ulong userId = message.Author.Id;
            GlobalLongMemory.AddMemory(embeddding, message.Channel.Id);
            GlobalLongMemory.SaveMemories(message.Channel.Id);
        }

        /// <summary>
        /// The first step in the ReactToUserMessage method is to track the message using short term memory by storing information
        /// about it, including its content, author, location in a channel, type, intention, and related context, in a memory tracking
        /// system. This system creates a new memory object for the message and associates it with the user and channel it came from.
        /// The memory object is given a globally unique identifier(Guid) which can be used to identify it in the memory system.The
        /// method returns this Guid so that it can be used later in the process.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private Guid ShortMemory(SocketUserMessage? message)
        {
            return _memory.Remember(message.Content, message.Author, MemoryType.LearnedSkill, message.Channel.Id);
        }

        private bool HandleMemorySharing(SocketUserMessage message)
        {
            if (!message.Content.Contains("share memories from "))
            {
                return false;
            }

            ulong threadId = ExtractThreadIdFromMessage(message.Content);

            if (threadId == 0)
            {
                SendMessageAsync(message.Channel, "Could not parse thread id. Look for a long number at the top of all threads.");
                return true;
            }

            TransferMemoriesFromThreadToGlobal(threadId);
            SendMessageAsync(message.Channel, $"Memories transferred from {threadId} to global memory.");
            return true;
        }

        private ulong ExtractThreadIdFromMessage(string content)
        {
            const string searchString = "from ";
            int startIndex = content.IndexOf(searchString) + searchString.Length;
            string restOfString = content.Substring(startIndex);

            return ulong.TryParse(restOfString, out ulong threadId) ? threadId : 0;
        }

        private void TransferMemoriesFromThreadToGlobal(ulong threadId)
        {
            GlobalLongMemory.LoadMemories(threadId);
            var memories = GlobalLongMemory.Memories;
            GlobalLongMemory.TransferToGlobalMemory(memories);
            GlobalLongMemory.SaveGlobal();
        }

        private async Task SendMessageAsync(IMessageChannel channel, string message)
        {
            await channel.SendMessageAsync(message);
        }

        public async Task<bool> BasicCommands(SocketUserMessage message)
        {
            if (HandleMemorySharing(message))
            {
                return true;
            }
            // Check current user settings
            if (message.Content.Contains("check my settings")) 
            {
                var user = message.Author;
                var temperature = _interactionHandler.GetUserTemperature(user.Id);
                var prompt = _interactionHandler.GetUserPrompt(user.Id);
                await message.Channel.SendMessageAsync($"Settings for <@{user.Id}> are... Temperature: {temperature} | Prompt: \n{prompt}");

            }
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
            // Dictate where the bot is allowed to operate.
            if (message.Content.Contains("move Realization here"))
            {
                AllowedChannel = message.Channel.Name;
                return true;
            }
            // Restrict message handling to one channel.
            if (message.Channel.Name != AllowedChannel) return true;

            // Check for wake and sleep messages
            if (message.Content.Contains("wake up"))
            {
                if (_sleeping)
                {
                    await message.Channel.SendMessageAsync("Waking up...");
                    _sleeping = false;
                    return true;
                }
                else
                {
                    await message.Channel.SendMessageAsync("I am already awake.");
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
                else
                {
                    await message.Channel.SendMessageAsync("Already sleeping.");
                    return true;
                }
            }
            if (message.Content.Contains("reset bot"))
            {
                if (!_sleeping)
                {
                    await message.Channel.SendMessageAsync("I am resetting myself.");
                    Auditory = new();
                    _memory = new ShortTermMemory<string>();
                    _quiet = false;
                    Cortex = new Cortex(new List<Intention> { new None() }, Cognition, _openAIKey);
                    Attention = new ReticularSystem(_memory);
                    _sleeping = false;
                    await message.Channel.SendMessageAsync("I'm back.");
                    return true;
                }
            } 
            if (message.Content.Contains("quiet mode"))
            {
                if (_quiet)
                {
                    await message.Channel.SendMessageAsync("I am already quiet.");
                    return true;
                }
                else
                {
                    _quiet = true;
                    await message.Channel.SendMessageAsync("Going quiet...");
                    return true;
                }
            }
            if (message.Content.Contains("verbose mode"))
            {
                if (_quiet)
                {
                    await message.Channel.SendMessageAsync("I am already verbose.");
                    return true;
                }
                else
                {
                    _quiet = false;
                    await message.Channel.SendMessageAsync("Going verbose...");
                    return true;
                }
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
                var dialogue = JsonConvert.SerializeObject(Auditory.Threads.FirstOrDefault(chan => chan.Key == messageParam.Channel.Id).Value.Dialogue);
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
    }

}
