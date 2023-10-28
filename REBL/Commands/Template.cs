using System.Drawing;
using REBL.Utilities;

namespace REBL.Commands
{
    public class Template : Command, SetupCommand<Template>
    {
        public Template(REBLConsole console) : base(console)
        {
            Name = "template";
            Act = () => { };
        }
        public Template(string name, string format, REBLConsole console) : base(console)
        {
            Name = "expression";
            Structure = new Dialogue.Template(name, format);
            Act = () => GetExpression();
        }

        public Dialogue.Template Structure { get; set; }

        public Template Create(string input, REBLConsole console)
        {
            // Same as the Expression command
            string name, format;
            int index = input.IndexOf(' ');
            if (index > 0)
            {
                name = input.Substring(0, index);
                format = input.Substring(index + 1);
            }
            else
            {
                name = input;
                format = "";
            }
            return new Template(name, format, console);
        }

        public void GetExpression()
        {
            AddTemplateToBuffer();
            Colors.WriteLine(Structure.Write(), Color.Green);
        }

        // Add template to the buffer
        public void AddTemplateToBuffer()
        {
            Rebel.Dynamics[Structure.Name] = Structure;
        }
    }





}