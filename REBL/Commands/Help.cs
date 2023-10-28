using System.Drawing;
using REBL.Utilities;

namespace REBL.Commands
{
    public class Help : Command, SetupCommand<Help>
    {
        public Help(REBLConsole console) : base(console)
        {
            Name = "help";
            Act = () => GetHelp();
        }

        public Help Create(string input, REBLConsole console) => new Help(console);

        public void GetHelp()
        {
            var actions = ActionBuilder.GetInstances<Command>();
            Colors.WriteLine("Command List:", Color.White);
            for (int i = 0; i < actions.Count; i++)
            {
                Colors.WriteLine(actions[i].Name, Color.Green);
            }
        }
    }





}