using System;
using System.Collections.Generic;
using System.Text;

namespace ThotLibrary
{
    public interface Intention
    {
        string Name { get; set; }
        float Threshold { get; set; }
    }
}
