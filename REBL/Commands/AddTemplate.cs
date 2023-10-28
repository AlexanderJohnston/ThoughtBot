using REBL.Utilities;

namespace REBL.Commands
{
    //Does the same as AddExpression but this one adds Templates to a buffer with the "templates." prefix
    public class AddTemplate : Command, SetupCommand<AddTemplate>
    {
        public AddTemplate(REBLConsole console) : base(console)
        {
            Name = "addtemplate";
            Act = () => AddTemplateToBuffer();
        }
        public AddTemplate(string key, string templateName, REBLConsole console) : base(console)
        {
            Name = "addtemplate";
            Key = key;
            TemplateName = templateName;
            Act = () => AddTemplateToBuffer();
        }

        public string Key { get; set; }
        public string TemplateName { get; set; }

        public AddTemplate Create(string input, REBLConsole console)
        {
            // Get the key and template name from the string
            string key, templateName;
            int index = input.IndexOf(' ');
            if (index > 0)
            {
                key = input.Substring(0, index);
                templateName = input.Substring(index + 1);
            }
            else
            {
                key = input;
                templateName = "";
            }
            return new AddTemplate(key, templateName, console);
        }

        private void AddTemplateToBuffer()
        {
            // Get the template
            var prefixed = string.Format("{0}", TemplateName);
            var newTemplate = (Dialogue.Template)Rebel.Dynamics[prefixed];
            // Unpack the buffer
            var packedBuffer = Rebel.Dynamics[Key];
            var bufferExists = Buffers.Is<List<object>>(packedBuffer);
            Tuple<Dialogue.Template, Dialogue.Expression[]> unpack;
            bool newItem;
            if (!bufferExists || packedBuffer.Count == 0)
            {

                unpack = new Tuple<Dialogue.Template, Dialogue.Expression[]>
                    (default(Dialogue.Template), new Dialogue.Expression[0]);
            }
            else
            {
                unpack = (Tuple<Dialogue.Template, Dialogue.Expression[]>)packedBuffer;
            }
            var expressions = unpack.Item2;
            var newTuple = new Tuple<Dialogue.Template, Dialogue.Expression[]>(newTemplate, expressions);
            Rebel.Dynamic(Key, newTuple);
        }
    }





}