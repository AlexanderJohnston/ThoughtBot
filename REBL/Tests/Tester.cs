using REBL.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REBL.Tests
{
    public class Tester
    {
        public Tester(REBLConsole console)
        {
            _console = console;
        }
        REBLConsole _console { get; set; }

        public void RunTest()
        {
            var hello = new Express("greeting", "hello", _console);
            var echo = new Template("echo", "{0}", _console);
            var makeBuffer = new Commands.Buffer("sayHello", _console);
            var addTemplate = new AddTemplate("sayHello", "echo", _console);
            var addExpression = new AddExpression("sayHello", "greeting", _console);

            _console.IsReady = false;
            _console.Run(hello);
            _console.Run(echo);
            _console.Run(makeBuffer);
            _console.Run(addTemplate);
            _console.Run(addExpression);
            _console.IsReady = true;
        }
    }
}
