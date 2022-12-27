using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realization.Skill
{
    // Stores EmbeddedMemories to disk and retrieves them.
    public class DiskEmbedder
    {
        public string Path { get; set; }
        public DiskEmbedder(string path)
        {
            Path = path;
        }
        // Check if file already exists at path.
        // If it does, overwrite it with the list of memories.
        // If it doesn't, create it and write list of memories to it.
        public void WriteMemories(List<EmbeddedMemory> memories)
        {
            if (System.IO.File.Exists(Path))
            {
                System.IO.File.WriteAllText(Path, System.Text.Json.JsonSerializer.Serialize(memories));
            }
            else
            {
                System.IO.File.Create(Path);
                System.IO.File.WriteAllText(Path, System.Text.Json.JsonSerializer.Serialize(memories));
            }
        }
        // Check if file already exists at path.
        // If it does, read all memories from it and return them.
        // If it does not, return an empty set of memories.
        public List<EmbeddedMemory> ReadMemories()
        {
            if (System.IO.File.Exists(Path))
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<EmbeddedMemory>>(System.IO.File.ReadAllText(Path));
            }
            else
            {
                return new List<EmbeddedMemory>();
            }
        }
    }
}
