﻿using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory
{
    // Stores EmbeddedMemories to disk and retrieves them.
    public class DiskEmbedder
    {
        public string Path { get; set; }
        public DiskEmbedder(string template)
        {
            Path = string.Format(template, Guid.NewGuid());
        }

        // Check if file already exists at path.
        // If it does, overwrite it with the list of memories.
        // If it doesn't, create it and write list of memories to it.
        public void WriteMemories(List<EmbeddedMemory> memories)
        {

            if (File.Exists(Path))
            {
                string path = Path;
                try
                {
                    File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(memories));
                }
                catch (System.IO.IOException)
                {
                    path = Path + new Random().Next(0,999999);
                    File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(memories));
                }
                catch (Exception ex) { throw ex;  }
            }
            else
            {
                File.WriteAllText(Path, System.Text.Json.JsonSerializer.Serialize(memories));
            }
        }
        // Check if file already exists at path and that it is not empty.
        // If it does, read all memories from it and return them.
        // If it does not, return an empty set of memories.
        public List<EmbeddedMemory> ReadMemories()
        {
            if (File.Exists(Path) && new FileInfo(Path).Length > 0)
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<EmbeddedMemory>>(File.ReadAllText(Path));
            }
            else
            {
                return new List<EmbeddedMemory>();
            }
        }
    }
}
