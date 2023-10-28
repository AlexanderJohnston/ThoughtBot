//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace REBL.Commands
//{
//    /// <summary>
//    /// Replaces an expression in a specific claim by looking it up based on a key
//    /// Should be a <see cref="Command/> and should implement <see cref="SetupCommand{T}/>
//    /// </summary>
//    public class ReplaceExpression : Command, SetupCommand<ReplaceExpression>
//    {
//        public ReplaceExpression(REBLConsole console) : base(console) { }
//        public ReplaceExpression(string name, string key, REBLConsole console) : base(console)
//        {
//            Name = name;
//            Key = key;
//            Console = console;
//        }

//        public string Name { get; set; }
//        public string Key { get; set; }
//        public string Claim { get; set; }
//        public REBLConsole Console { get; set; }

//        public void Setup()
//        {
//            // Lookup a claim from Dynamics
//            var claim = (Dialogue.Claim)Console.Dynamics[Key];
//            // Find the Expression matching the name provided
//            var expression = claim.Expressions.Find(x => x.Name == Name);
            
//        }

//        public override void Act()
//        {
//            Setup();
//        }
//    }
//}
