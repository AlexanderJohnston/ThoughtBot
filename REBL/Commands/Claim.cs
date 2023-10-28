using System.Drawing;
using REBL.Utilities;

namespace REBL.Commands
{
    public class Claim : Command, SetupCommand<Claim>
    {
        public Claim(REBLConsole console) : base(console)
        {
            Name = "claim";
            Act = () => GetClaims();
        }
        public Claim(Dialogue.Template template, Dialogue.Expression[] expressions, REBLConsole console) : base(console)
        {
            Name = "claim";
            Claims = new Dialogue.Claim(template, expressions);
            Act = () => GetClaims();
        }

        public Dialogue.Claim Claims { get; set; }

        public Claim Create(string key, REBLConsole console)
        {
            // Use the key to lookup the buffer containing a tuple of the Template and the Expression array
            var buffer = console.Dynamics[key];
            var unpackedBuffer = (List<object>)buffer;
            var selectedValue = unpackedBuffer[0];
            var tuple = (Tuple<Dialogue.Template, Dialogue.Expression[]>)selectedValue;
            return new Claim(tuple.Item1, tuple.Item2, console);
        }

        public void GetClaims()
        {
            Colors.WriteLine(Claims.WriteSafe(), Color.White);
        }
    }





}