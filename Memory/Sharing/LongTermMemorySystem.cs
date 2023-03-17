using System.Collections.Generic;
using Memory;
using Newtonsoft.Json;

namespace Memory.Sharing
{

    public class LongTermMemorySystem
    {
        internal Dictionary<ulong, List<EmbeddedMemory>> userMemories;
        private List<EmbeddedMemory> globalMemories;

        public LongTermMemorySystem()
        {
            userMemories = new Dictionary<ulong, List<EmbeddedMemory>>();
            globalMemories = new List<EmbeddedMemory>();
        }

        public void AddMemory(EmbeddedMemory memory, ulong userId)
        {
            if (!userMemories.ContainsKey(userId))
            {
                userMemories[userId] = new List<EmbeddedMemory>();
            }

            userMemories[userId].Add(memory);
        }

        public void LoadMemories(List<EmbeddedMemory> memory, ulong userId)
        {

            userMemories[userId] = memory;
        }

        public void LoadMemory(List<EmbeddedMemory> memory) => globalMemories = memory;

        public void AddGlobalMemory(EmbeddedMemory memory)
        {
            globalMemories.Add(memory);
        }

        public List<EmbeddedMemory> GetUserMemories(ulong userId)
        {
            if (userMemories.ContainsKey(userId))
            {
                return userMemories[userId];
            }
            else
            {
                return new List<EmbeddedMemory>();

            }
        }

        public List<EmbeddedMemory> GetGlobalMemories() => globalMemories;
        
        public void TransferToGlobal(ulong userId, EmbeddedMemory memory)
        {
            if (userMemories.ContainsKey(userId) && userMemories[userId].Contains(memory))
            {
                RemoveMemory(memory, userId);
                globalMemories.Add(memory);
            }
        }
        
        public void RemoveMemory(EmbeddedMemory memory, ulong userId)
        {
            if (userMemories.ContainsKey(userId))
            {
                userMemories[userId].Remove(memory);
            }
        }
        
        public void RemoveAllMemories()
        {
            userMemories.Clear();
            globalMemories.Clear();
        }

        public void RemoveMemoriesForModel(string model, ulong userId)
        {
            if (userMemories.ContainsKey(userId))
            {
                userMemories[userId].RemoveAll(memory => memory.Embedding.Model == model);
            }
        }
    }
}