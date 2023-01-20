using Discord;
using Discord.WebSocket;
using System.Threading.Channels;
using ThotLibrary;

namespace Memory.Converse
{
    public enum MemoryType
    {
        Command,
        Question,
        FormulatedIntent,
        LearnedSkill,
        Unknown
    }
    public class MemoryContext
    {
        public string ContextId;
        public string RelatedToContextId;
        public string Author;
        public MemoryType Type;
        public ulong Channel;
        public Intention Intention;
        public Guid Id;

        public MemoryContext(string userName, MemoryType type, ulong channel, Intention intention, Guid id, string relatedContext = "")
        {
            Author = userName;
            Type = type;
            Channel = channel;
            RelatedToContextId = relatedContext;
            ContextId = Guid.NewGuid().ToString();
            Intention = intention;
            Id = id;
        }
    }
    public class Memory<T>
    {
        public T Value;
        public MemoryContext Context;
    }
    public class ShortTermMemory<T>
    {
        private Dictionary<ulong, Memory<T>> FocusedMemory;
        private Dictionary<ulong, List<Memory<T>>> Users;
        private Dictionary<ulong, List<Memory<T>>> Channels;

        public ShortTermMemory()
        {
            FocusedMemory = new Dictionary<ulong, Memory<T>>();
            Users = new Dictionary<ulong, List<Memory<T>>>();
            Channels = new Dictionary<ulong, List<Memory<T>>>();
        }
        /// <summary>
        /// Store message information in short term memory.
        /// </summary>
        /// <param name="value">the value to be stored in memory</param>
        /// <param name="user">an object representing the user who created the message</param>
        /// <param name="type">a value specifying the type of memory being tracked</param>
        /// <param name="channel">the ID of the channel where the message was sent</param>
        /// <param name="intention">an object representing the intention behind the message</param>
        /// <param name="relatedContext">a string containing additional context related to the message (optional, default value is an empty string)</param>
        /// <returns>the id of the memory object.</returns>
        public Guid Remember(T value, IUser user, MemoryType type, ulong channel, Intention intention, string relatedContext = "")
        {
            var userId = user.DiscriminatorValue;

            if (!Remembers(userId))
            {
                Users.Add(userId, new List<Memory<T>>());
            }
            if (!RememberChannel(channel))
            {
                Channels.Add(channel, new List<Memory<T>>());
            }

            var memories = Recall(userId);
            var channelList = RecallChannel(channel);
            var memory = new Memory<T>();
            var id = Guid.NewGuid();
            memory.Value = value;
            memory.Context = new MemoryContext(user.Username, type, channel, intention, id, relatedContext);
            memories.Add(memory);
            channelList.Add(memory);
            SaveFocusedMemory(user, memory);
            return id;
        }

        public Memory<T> GetFocusedMemory(IUser user) => FocusedMemory.FirstOrDefault(memory => memory.Key == user.DiscriminatorValue).Value;

        public void SaveFocusedMemory(IUser user, Memory<T> memory) => FocusedMemory[user.DiscriminatorValue] = memory;

        public Memory<T> MemoryBeforeLast(SocketUser user)
        {
            var memory = Recall(user.DiscriminatorValue);
            return memory[memory.Count - 2];
        }

        public Memory<T> LastMemoryObject(SocketUser user) => Recall(user.DiscriminatorValue).Last();

        public string LastMemory(IUser user)
        {
            var userId = user.DiscriminatorValue;
            if (Remembers(userId))
            {
                var lastMemory = Recall(userId).Last();
                return string.Format("The last thing I remember from {0} is the {1}: {2}",
                    lastMemory.Context.Author,
                    lastMemory.Context.Type,
                    lastMemory.Value.ToString());
            }
            else return string.Format("I don't remember anything yet for {0}", user.Username);
        }

        public List<string> AllMemories(IUser user)
        {
            var explainedMemories = new List<string>();
            var userId = user.DiscriminatorValue;
            if (Remembers(userId))
            {
                var memories = Recall(userId);
                foreach (var memory in memories)
                {
                    var explained = string.Format("I remember {0} providing the intent {1} to {2}: {3}",
                        memory.Context.Author,
                        memory.Context.Intention.Name,
                        memory.Context.Type,
                        memory.Value.ToString());
                    explainedMemories.Add(explained);
                }
            }
            return explainedMemories;
        }

        public List<Memory<T>> AllMemories(ulong channel) => RecallChannel(channel);
        public void WipeAll(ulong channelId)
        {
            var channel = Channels.First(channel => channel.Key == channelId);
            var memories = channel.Value;
            memories.Clear();
        }
        public void WipeAllButLast(ulong channelId)
        {
            var channel = Channels.First(channel => channel.Key == channelId);
            var memories = channel.Value;
            var lastMemory = memories.Last();
            memories.Clear();
            memories.Add(lastMemory);
        }

        private bool Matches(Memory<T> memory, string context) => memory.Context.ContextId == context;
        public bool RememberChannel(ulong channelId) => Channels.Any(channel => channel.Key == channelId);
        public bool Remembers(ulong userId) => Users.Any(user => user.Key == userId);
        private List<Memory<T>> Recall(ulong userId) => Users.First(user => user.Key == userId).Value;
        private List<Memory<T>> RecallChannel(ulong channelId) => Channels.First(channel => channel.Key == channelId).Value;
        private Memory<T> Specific(string context, ulong userId) =>
            Recall(userId).First(memory => Matches(memory, context));
        //public Memory<T> Specific(Guid id)
        //{

        //}
    }
}
