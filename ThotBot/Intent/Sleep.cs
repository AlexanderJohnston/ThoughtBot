using System;
using System.Collections.Generic;
using System.Text;

namespace ThotBot.Intent
{
    public class Sleep : Intention
    {
        public float Threshold { get; set; } = 0.80F;

        public string Name { get; set; } = "Sleep";
    }
}
