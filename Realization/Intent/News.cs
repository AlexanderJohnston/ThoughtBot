using System;
using System.Collections.Generic;
using System.Text;
using ThotLibrary;

namespace Realization.Intent
{
    public class News : Intention
    {
        public float Threshold { get; set; } = 0.50F;

        public string Name { get; set; } = "News";
    }
}
