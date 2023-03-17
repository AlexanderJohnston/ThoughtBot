using Memory.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory
{
    // Tracks embedded memories in memory and calls a file manager to persist them to disk.
    public class EmbeddingMemory
    {
        public List<EmbeddedMemory> Global { get; set; }
        public List<EmbeddedMemory> Memories { get; set; }

        public List<EmbeddedMemory> Thread { get; set; }
        public PersistentLongTermMemorySystem LongTermMemorySystem { get; set; }

        public EmbeddingMemory(string storagePath)
        {
            Global = new List<EmbeddedMemory>();
            Memories = new List<EmbeddedMemory>();
            LongTermMemorySystem = new PersistentLongTermMemorySystem(storagePath);
        }

        public void TransferToGlobalMemory(List<EmbeddedMemory> memories)
        {
            foreach (EmbeddedMemory memory in memories)
            {
                LongTermMemorySystem.AddGlobalMemory(memory);
            }

        }

        public void AddMemory(EmbeddedMemory memory, ulong userId)
        {
            LongTermMemorySystem.AddMemory(memory, userId);
        }

        public void AddMemories(List<EmbeddedMemory> memories, ulong userId)
        {
            Memories.AddRange(memories);
            foreach (var memory in memories)
            {
                LongTermMemorySystem.AddMemory(memory, userId);
            }
        }

        public void RemoveMemory(EmbeddedMemory memory, ulong userId)
        {
            Memories.Remove(memory);
            LongTermMemorySystem.RemoveMemory(memory, userId);
        }

        public void RemoveAllMemories()
        {
            Memories.Clear();
            LongTermMemorySystem.RemoveAllMemories();
        }

        public void RemoveMemoriesForModel(string model, ulong userId)
        {
            Memories.RemoveAll(memory => memory.Embedding.Model == model);
            LongTermMemorySystem.RemoveMemoriesForModel(model, userId);
        }

        public void RemoveMemoriesFor(string model, ulong userId)
        {
            Memories.RemoveAll(memory => memory.Embedding.Model == model);
            LongTermMemorySystem.RemoveMemoriesForModel(model, userId);
        }


        public void SaveMemories(ulong userId)
        {
            LongTermMemorySystem.SaveUserMemoriesToDisk(userId);
        }

        public void SaveGlobal()
        {
            LongTermMemorySystem.SaveGlobalMemoriesToDisk();
        }

        public void LoadMemories(ulong userId)
        {
            Global = LongTermMemorySystem.GetGlobalMemories();
            LongTermMemorySystem.LoadUserMemoriesFromDisk(userId);
            Memories = LongTermMemorySystem.GetUserMemories(userId);
        }
    }

}
