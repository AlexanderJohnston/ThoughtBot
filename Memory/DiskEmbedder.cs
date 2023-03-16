using Newtonsoft.Json;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Memory
{
    // Stores EmbeddedMemories to disk and retrieves them.
    public class DiskEmbedder
    {
        public string BasePath { get; set; }

        public DiskEmbedder(string basePath)
        {
            BasePath = basePath;
        }

        public void WriteMemories(List<EmbeddedMemory> memories, string path)
        {
            string fullPath = System.IO.Path.Combine(BasePath, path);

            if (File.Exists(fullPath))
            {
                try
                {
                    File.WriteAllText(fullPath, JsonConvert.SerializeObject(memories));
                }
                catch (Exception ex) { throw ex; }
            }
            else
            {
                File.WriteAllText(fullPath, JsonConvert.SerializeObject(memories));
            }
        }

        public List<EmbeddedMemory> ReadMemories(string path)
        {
            string fullPath = System.IO.Path.Combine(BasePath, path);

            try
            {
                if (File.Exists(fullPath) && new FileInfo(fullPath).Length != 0)
                {
                    return JsonConvert.DeserializeObject<List<EmbeddedMemory>>(File.ReadAllText(fullPath));
                }
                else
                {
                    return new List<EmbeddedMemory>();
                }
            }
            catch (FileNotFoundException _fex)
            {
                return new List<EmbeddedMemory>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
