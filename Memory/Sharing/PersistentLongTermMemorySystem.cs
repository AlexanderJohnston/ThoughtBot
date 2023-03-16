using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory.Sharing
{
    public class PersistentLongTermMemorySystem : LongTermMemorySystem
    {
        private DiskEmbedder diskEmbedder;

        public PersistentLongTermMemorySystem(string storagePath) : base()
        {
            diskEmbedder = new DiskEmbedder(storagePath);
            LoadFromDisk();
        }

        public void SaveToDisk()
        {
            List<EmbeddedMemory> allMemories = new List<EmbeddedMemory>(GetGlobalMemories());

            foreach (var userMemories in GetAllUserMemories())
            {
                allMemories.AddRange(userMemories.Value);
            }

            diskEmbedder.WriteMemories(allMemories);
        }

        private void LoadFromDisk()
        {
            List<EmbeddedMemory> allMemories = diskEmbedder.ReadMemories();

            foreach (EmbeddedMemory memory in allMemories)
            {
                if (memory.Memory.UserId == 0)
                {
                    // Global memory
                    AddGlobalMemory(memory);
                }
                else
                {
                    // User memory
                    AddMemory(memory, memory.Memory.UserId);
                }
            }
        }

        private Dictionary<ulong, List<EmbeddedMemory>> GetAllUserMemories()
        {
            return userMemories;
        }
    }
}
