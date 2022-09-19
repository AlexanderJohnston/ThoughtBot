using System;
using System.Collections.Generic;
using System.Text;
using ThotLibrary;

namespace Realization.Intent
{
    public class LinkResource : Intention
    {
        public float Threshold { get; set; } = 0.80F;

        public string Name { get; set; } = "LinkResource";
    }
}
