using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory
{
    ///// <summary>
    ///// This is the main class for the Memory library.
    ///// It is responsible for managing the memory of the bot.
    ///// It holds a reference to the <see cref="DiskEmbedder"/> <see cref="DiskEmbedder.ReadMemories">to read</see> and <see cref="DiskEmbedder.WriteMemories">to write</see> memories.
    ///// When this class is constructed, it initializes DiskEmbedder with a given path.
    ///// Loaded memories of type EmbeddedMemory are stored in this class with methods to access them.
    ///// </summary>
    //public class Memories
    //{
    //    private DiskEmbedder _diskEmbedder;
    //    private List<EmbeddedMemory> _memories;
    //    public Memories(string path)
    //    {
    //        _diskEmbedder = new DiskEmbedder(path);
    //        _memories = _diskEmbedder.ReadMemories();
    //    }
    //    public void AddMemory(EmbeddedMemory memory)
    //    {
    //        _memories.Add(memory);
    //    }
    //    public void AddMemories(List<EmbeddedMemory> memories)
    //    {
    //        _memories.AddRange(memories);
    //    }
    //    public void RemoveMemory(EmbeddedMemory memory)
    //    {
    //        _memories.Remove(memory);
    //    }
    //    public void RemoveMemories(List<EmbeddedMemory> memories)
    //    {
    //        foreach (var memory in memories)
    //        {
    //            _memories.Remove(memory);
    //        }
    //    }
    //    public List<EmbeddedMemory> GetMemories()
    //    {
    //        return _memories;
    //    }
    //    public void SaveMemories()
    //    {
    //        _diskEmbedder.WriteMemories(_memories);
    //    }
    //}

}
