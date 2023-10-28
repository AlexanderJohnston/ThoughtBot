using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REBL.Utilities
{
    /// <summary>
    /// Provides helper functions to avoid throwing exceptions when using the buffer <see cref=REBLConsole.Dynamics/>
    /// </summary>
    public class Buffers
    {
        // Method to check if the value has been instantiated with the expected type
        public static bool Is<T>(dynamic value)
        {
            return value is T;
        }
    }
}
