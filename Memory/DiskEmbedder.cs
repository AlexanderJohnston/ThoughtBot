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
        public string Prefix { get; set; }
        public string FileType { get; set; }

        public DiskEmbedder(string prefix = "longTermMemory", string fileType = ".json")
        {
            Prefix = prefix;
            FileType = fileType;
        }

        private string BuildPath(string extraPath = "")
        {
            return $"{Prefix}{(string.IsNullOrEmpty(extraPath) ? "" : $"-{extraPath}")}{FileType}";
        }

        public void WriteMemories(List<EmbeddedMemory> memories, string extraPath = "")
        {
            string fullPath = BuildPath(extraPath);

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

        public List<EmbeddedMemory> ReadMemories(string extraPath = "")
        {
            string fullPath = BuildPath(extraPath);

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
