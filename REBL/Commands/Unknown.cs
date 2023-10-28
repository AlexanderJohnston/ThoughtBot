using System.Drawing;
using REBL.Utilities;

namespace REBL.Commands
{
    public class Unknown : Command, SetupCommand<Unknown>
    {
        public Unknown(REBLConsole console) : base(console)
        {
            Name = "unknown";
            Act = () => Message("");
        }
        public Unknown(string command, REBLConsole console) : base(console)
        {
            Name = "unknown";
            Act = () => Message(command);
        }

        public Unknown Create(string input, REBLConsole console) => new Unknown(input, console);

        public void Message(string command)
        {
            Colors.Write("Unknown command: ", Color.Red);
            Colors.Write(command, Color.White);
            Console.Write(Environment.NewLine);
        }
    }





}