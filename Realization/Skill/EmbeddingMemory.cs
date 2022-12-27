using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realization.Skill
{
    // Tracks embedded memories in memory and calls a file manager to persist them to disk.
    public class EmbeddingMemory
    {
        public List<EmbeddedMemory> Memories { get; set; }
        public DiskEmbedder DiskEmbedder { get; set; }
        public EmbeddingMemory()
        {
            Memories = new List<EmbeddedMemory>();
            DiskEmbedder = new DiskEmbedder("memories.json");
        }
        // Add a new memory to the list of memories.
        public void AddMemory(EmbeddedMemory memory)
        {
            Memories.Add(memory);
        }
        // Add a range memory to the list of memories.
        public void AddMemories(List<EmbeddedMemory> memories)
        {
            Memories.AddRange(memories);
        }
        // Remove a memory from the list of memories.
        public void RemoveMemory(EmbeddedMemory memory)
        {
            Memories.Remove(memory);
        }
        // Remove all memories.
        public void RemoveMemories(List<EmbeddedMemory> memories)
        {
            Memories.RemoveAll(memories.Contains);
        }
        // Remove memories for a specific model
        public void RemoveMemories(string model)
        {
            Memories.RemoveAll(memory => memory.Model == model);
        }
        // Saves all memories to disk.
        public void SaveMemories()
        {
            DiskEmbedder.WriteMemories(Memories);
        }
        // Loads all memories from disk.
        public void LoadMemories()
        {
            Memories = DiskEmbedder.ReadMemories();
        }
    }
}
