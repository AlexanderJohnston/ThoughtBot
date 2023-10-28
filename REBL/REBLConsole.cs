using System;
using System.Drawing;
using REBL.Commands;
using REBL.Tests;
using REBL.Utilities;

namespace REBL
{
    // Create a simple REPL for a C# console
    public class REBLConsole
    {
        public REBLConsole()
        {
            Name = "Realization.REBL";
            Actions = new Command[2]; 
            Actions[0] = new Clear(this);
            Actions[1] = new Help(this);
            GetActions();
            var tester = new Tester(this);
            tester.RunTest();
        }

        public bool IsReady = true;
        string Name;
        string? CurrentCommand;
        Command[] Actions;
        Command[] Commands;
        public Dictionary<string, dynamic> Dynamics = new();

        public void Dynamic(string key, dynamic thing) => Dynamics[key] = thing;

        public void Run(Command? command = null)
        {
            ClearAct();
            if (command != null)
            {
                Report(command);
            }
            if (IsReady)
            {
                Ready();
                ReadInput();
            }
        }

        public void ClearAct()
        {
            Actions[0].Act();
        }

        public void GetActions()
        {
            var actions = ActionBuilder.GetInstances<Command>();
            Actions = new Command[actions.Count];
            //Colors.Write("Command List:", Color.White);
            for (int i = 0; i < actions.Count; i++)
            {
                Command instance = Create<Command>(actions[i]);
                Actions[i] = instance;
                //Colors.WriteLine(actions[i].Name, Color.Green);
            }
        }

        public T Create<T>(Type selected)
        {
            if (selected.IsSubclassOf(typeof(T)))
            {
                // Create instance and pass this as in this class as the parameter

                T instance = (T)Activator.CreateInstance(selected, this);
                return instance;
            }
            else return default(T);
        }

        public void Report(Command? command = null)
        {
            if (command != null)
            {
                command.Act();
            }
        }

        public void Ready()
        {
            string prefix = string.Format("{0}> ", Name);
            Colors.Write(prefix, Color.Red);
        }

        public void ReadInput()
        {
            var command = Console.ReadLine();
            CurrentCommand = command;
            var commandIndex = command.IndexOf(' ');
            if (commandIndex < 0)
            {
                commandIndex = command.Length;
            }
            var commandName = command.Substring(0, commandIndex).ToLower();
            string parsedCommand;
            if (HasParameters(command))
            {
                parsedCommand = command.Substring(commandIndex + 1);
            }
            else
            {
                parsedCommand = command;
            }
            if (commandName == "exit")
            {
                Colors.WriteLine("Goodbye!", Color.Red);
                return;
            }
            else if (Actions.Any(a => string.Equals(a.Name, commandName, StringComparison.OrdinalIgnoreCase)))
            {
                var action = Actions.First(a => string.Equals(a.Name.ToLower(), commandName));
                var type = action.GetType();
                var method = type.GetMethod("Create");
                var instance = method.Invoke(action, new object[] { parsedCommand, this });
                Run((Command)instance);
            }   
            else
            {
                // Check if any of the dynamics have a key matching the command
                if (Dynamics.Any(entry => string.Equals(entry.Key, command, StringComparison.OrdinalIgnoreCase)))
                {
                    Claim(command);
                    Run();
                }
                // Get the Unknown action from Actions
                var action = Actions.First(a => string.Equals(a.Name.ToLower(), "unknown"));
                var type = typeof(Unknown);
                var method = type.GetMethod("Create");
                var instance = method.Invoke(action, new object[] { parsedCommand, this });
                Run((Command)instance);
            }
        }

        private bool HasParameters(string command)
        {
            var trimmed = command.Trim();
            var index = trimmed.IndexOf(' ');
            if (index > 0)
            {
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Reads a dynamic from the Dynamics dictionary as a List of Tuple<Dialogue.Template, Dialogue.Expression[]>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private void Claim(string key)
        {
            var unpackedBuffer = (Tuple<Dialogue.Template, Dialogue.Expression[]>)Dynamics[key];
            var template = unpackedBuffer.Item1;
            var expressions = unpackedBuffer.Item2;
            var claim = new Dialogue.Claim(template, expressions);
            var makeClaim = claim.WriteSafe();
            Colors.WriteLine($"Claim: {makeClaim}", Color.Red);
        }
    }
}