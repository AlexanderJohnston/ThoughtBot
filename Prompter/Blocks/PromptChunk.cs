using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prompter.Blocks
{
    public class PromptChunk
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Format { get; set; }
        public PromptChunk(string name, string description, string format)
        {
            Name = name;
            Description = description;
            Format = format;
        }
    }
}
