using BlingFire;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory
{
    public static class TokenizerExtensions
    {
        public static int[] Tokenize(this string input)
        {
            // load XLM Roberta tokenization model
            var h = BlingFireUtils.LoadModel("./gpt2.bin");
            // get its UTF8 representation
            byte[] inBytes = System.Text.Encoding.UTF8.GetBytes(input);
            // allocate space for ids and offsets
            int[] Ids = new int[inBytes.Length];

            // tokenize with loaded XLM Roberta tokenization and output ids and start and end offsets
            var outputCount = BlingFireUtils.TextToIds(h, inBytes, inBytes.Length, Ids, Ids.Length, 0);
            Log.Verbose(string.Format("Gpt2.bin tokenizer return length: {0}", outputCount));
            // join the tokenized ids into a comma separated string
            var tokenized = string.Format("[{0}]", string.Join(",", Ids));
            Log.Verbose(tokenized);
            return Ids;
        }
    }
}
