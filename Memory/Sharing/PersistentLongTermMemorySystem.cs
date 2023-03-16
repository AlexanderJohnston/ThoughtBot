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
            diskEmbedder.WriteMemories(GetGlobalMemories(), basePath);
        }

        public void SaveUserMemoriesToDisk(ulong userId)
        {
            var userMemoriesPath = GetUserMemoriesPath(userId);
            diskEmbedder.WriteMemories(GetUserMemories(userId), userMemoriesPath);
        }

        public void LoadUserMemoriesFromDisk(ulong userId)
        {
            var userMemoriesPath = GetUserMemoriesPath(userId);
            List<EmbeddedMemory> userMemories = diskEmbedder.ReadMemories(userMemoriesPath);

            foreach (EmbeddedMemory memory in userMemories)
            {
                AddMemory(memory, userId);
            }
        }

        private void LoadGlobalMemoriesFromDisk()
        {
            List<EmbeddedMemory> globalMemories = diskEmbedder.ReadMemories(basePath);

            foreach (EmbeddedMemory memory in globalMemories)
            {
                AddGlobalMemory(memory);
            }
        }

        private string GetUserMemoriesPath(ulong userId)
        {
            return Path.Combine(basePath, $"user_{userId}");
        }
    }

}
