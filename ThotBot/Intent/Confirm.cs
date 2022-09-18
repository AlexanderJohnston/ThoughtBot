using System;
using System.Collections.Generic;
using System.Text;
using ThotLibrary;

namespace ThotBot.Intent
{
    public class Compliment : Intention
    {
        public float Threshold { get; set; } = 0.80F;

        public string Name { get; set; } = "Compliment";
    }
}
