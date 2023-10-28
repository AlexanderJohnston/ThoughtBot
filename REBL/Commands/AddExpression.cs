using Dialogue;
using REBL.Utilities;

namespace REBL.Commands
{
    //AddExpression adds an expression to the buffer, it looks up the template name from the buffer and adds the expression to it
    public class AddExpression : Command, SetupCommand<AddExpression>
    {
        public AddExpression(REBLConsole console) : base(console)
        {
            Name = "addexpression";
            Act = () => { };
        }
        public AddExpression(string key, string expression, REBLConsole console) : base(console)
        {
            Name = "addexpression";
            Key = key;
            Expression = expression;
            Act = () => AddExpressionToBuffer();
        }

        public string Key { get; set; }
        public string Expression { get; set; }

        public AddExpression Create(string input, REBLConsole console)
        {
            // Get the key and expression from the string
            string key, expression;
            int index = input.IndexOf(' ');
            if (index > 0)
            {
                key = input.Substring(0, index);
                expression = input.Substring(index + 1);
            }
            else
            {
                key = input;
                expression = "";
            }
            return new AddExpression(key, expression, console);
        }

        private void AddExpressionToBuffer()
        {
            Tuple<Dialogue.Template, Expression[]> unpack = Unpack();
            var template = unpack.Item1;
            var expressions = unpack.Item2;
            var prefixed = string.Format("{0}", Expression);
            var newExpression = (Dialogue.Expression)Rebel.Dynamics[prefixed];
            var newExpressions = new Dialogue.Expression[expressions.Length + 1];
            for (int i = 0; i < expressions.Length; i++)
            {
                newExpressions[i] = expressions[i];
            }
            newExpressions[^1] = newExpression;
            var newTuple = new Tuple<Dialogue.Template, Dialogue.Expression[]>(template, newExpressions);
            Rebel.Dynamic(Key, newTuple);
        }

        private Tuple<Dialogue.Template, Expression[]> Unpack()
        {
            // Unpack the buffer from the dynamic list
            var packedBuffer = Rebel.Dynamics[Key];
            var bufferExists = Buffers.Is<Tuple<Dialogue.Template, Dialogue.Expression[]>>(packedBuffer);

            Tuple<Dialogue.Template, Dialogue.Expression[]> unpack;
            if (!bufferExists)
            {
                var unpackedBuffer = new List<object>();
                unpack = new Tuple<Dialogue.Template, Dialogue.Expression[]>
                    (default(Dialogue.Template), new Dialogue.Expression[0]);
            }
            else
            {
                unpack = (Tuple<Dialogue.Template, Dialogue.Expression[]>)packedBuffer;
            }
            //var unpack = DynamicBuffer<Tuple<Dialogue.Template, Dialogue.Expression[]>>.Unpack(packedBuffer);
            return unpack;
        }
    }
}