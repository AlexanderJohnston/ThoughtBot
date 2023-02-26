using Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realization.Skill
{
    public class TemporalMemory
    {
        public TemporalMemory(List<EmbeddedMemory> memories)
        {
            Memories = memories;
        }
        public List<EmbeddedMemory> Memories { get; set; } = new();
    }
    /// <summary>
    ///  This is a rewrite of the <see cref="VentralStream"/> for <see cref="Memory.EmbeddedMemory"/> rather than <see cref="Memory.Converse.Conversation"/>.
    ///  This allows us to track the embedded memories for each user on Discord by their ulong.
    /// </summary>
    public class TemporalLobe
    {
        public Dictionary<People, EmbeddedMemory[]> Memories { get; set; }

        public void Correlate(ulong channel, ulong user, EmbeddedMemory memory)
        {
            
        }
    }
}
