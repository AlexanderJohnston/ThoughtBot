using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realization.Global
{
    internal static class Instructor
    {
        private static Dictionary<ulong, string> _instructions = new();
        public static void AddInstruction(ulong id, string instruction)
        {
            _instructions.Add(id, instruction);
        }
        public static string GetInstruction(ulong id) => _instructions[id];
    }
}
