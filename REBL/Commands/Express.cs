using Dialogue;
using REBL.Utilities;
using System.Drawing;

namespace REBL.Commands
{
    public class Express : Command, SetupCommand<Express>
    {
        public Express(REBLConsole console) : base(console)
        {
            Name = "express";
            Act = () => GetExpression();
        }
        public Express(string name, string expression, REBLConsole console) : base(console)
        {
            Name = "express";
            Expression = new Expression(name, expression);
            Act = () => GetExpression();
        }

        public Expression Expression { get; set; }

        public Express Create(string input, REBLConsole console)
        {
            // Find index of first space, then separate out the preceding string as name, and the following as expression
            string name, expression;
            int index = input.IndexOf(' ');
            if (index > 0)
            {
                name = input.Substring(0, index);
                expression = input.Substring(index + 1);
            }
            else
            {
                name = input;
                expression = "";
            }

            return new Express(name, expression, console);
        }

        public void GetExpression()
        {
            AddExpressionToBuffer();
            Colors.WriteLine(Expression.Write(), Color.Green);
        }

        public void AddExpressionToBuffer()
        {
            Rebel.Dynamics[Expression.Name] = Expression;
        }
    }





}