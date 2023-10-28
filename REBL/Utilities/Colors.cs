using System.Drawing;
using Console = Colorful.Console;

namespace REBL.Utilities
{
    public static class Colors
    {
        public static void Write(string text, Color color)
        {
            Console.Write(text, color);
        }

        internal static void WriteLine(string text, Color color)
        {
            Console.WriteLine(text, color);
        }
    }
}
