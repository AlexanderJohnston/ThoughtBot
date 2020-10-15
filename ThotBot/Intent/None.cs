using System;
using System.Collections.Generic;
using System.Text;

namespace ThotBot.Intent
{
    public class None : Intention
    {
        public float Threshold { get; set; } = 0.50F;

        public string Name { get; set; } = "None";
    }
}
