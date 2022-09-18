using System;
using System.Collections.Generic;
using System.Text;
using ThotLibrary;

namespace ThotBot.Intent
{
    public class DebugEntities : Intention
    {
        public float Threshold { get; set; } = 0.30F;

        public string Name { get; set; } = "DebugEntities";
    }
}
