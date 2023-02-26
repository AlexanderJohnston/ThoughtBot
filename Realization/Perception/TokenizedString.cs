using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realization.Perception
{
    public class TokenizedString
    {
        public string Original { get; set; }
        public int Tokens { get; set; }
        public TokenizedString(string original, int tokens)
        {
            Original = original;
            Tokens = tokens;
        }
    }
}
