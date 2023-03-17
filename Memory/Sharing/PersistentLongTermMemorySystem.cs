using Memory.Converse;
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
        private string basePath;

        public PersistentLongTermMemorySystem(string storagePath) : base()
        {
            basePath = storagePath;
            diskEmbedder = new DiskEmbedder(basePath);
            LoadGlobalMemoriesFromDisk();
        }

        public void SaveGlobalMemoriesToDisk()
        {
            diskEmbedder.WriteMemories(GetGlobalMemories());
        }

        public void SaveUserMemoriesToDisk(ulong userId)
        {
            var memories = GetUserMemories(userId);
            diskEmbedder.WriteMemories(memories, userId.ToString());
        }

        public void LoadUserMemoriesFromDisk(ulong userId)
        {
            List<EmbeddedMemory> userMemories = diskEmbedder.ReadMemories(userId.ToString());
            LoadMemories(userMemories, userId);
        }

        private void LoadGlobalMemoriesFromDisk()
        {
            List<EmbeddedMemory> globalMemories = diskEmbedder.ReadMemories();
            LoadMemory(globalMemories);
        }
    }

}
