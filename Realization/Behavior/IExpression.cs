using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realization.Behavior
{
    public interface IExpression
    {
        public string GetName();
        public void SetFormat(string format);
        public string ExpressFormat(string input);
    }

    public class Conversation : IExpression
    {
        private string _format { get; set; }
        private string _name { get; }
        public string ExpressFormat(string input) => string.Format(_format, input);
        public string GetName() => _name;
        public void SetFormat(string format) => _format = format;
        public Conversation(string name, string format)
        {
            _name = name;
            _format = format;
        }
    }

    public class Memories : IExpression
    {
        private string _format { get; set; }
        private string _name { get; }
        public string ExpressFormat(string input) => string.Format(_format, input);
        public string GetName() => _name;
        public void SetFormat(string format) => _format = format;
        public Memories(string name, string format)
        {
            _name = name;
            _format = format;
        }
    }

    public class Prompt : IExpression
    {
        private string _format { get; set; }
        private string _name { get; }
        public string ExpressFormat(string input) => string.Format(_format, input);
        public string GetName() => _name;
        public void SetFormat(string format) => _format = format;
        public Prompt(string format, string name)
        {
            _format = format;
            _name = name;
        }
    }
}
