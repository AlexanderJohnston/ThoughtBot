using System.Text;

namespace Prompter
{
    /// <summary>
    /// This class represents a prompt to be sent to an AI model.
    /// It is used to store the prompt's format as well as expose
    /// methods to add and remove variables from the prompt.
    /// it should also expose methods to generate the final prompt
    /// by passing all variables in.
    /// </summary>
    public class Prompt
    {
        private const string INST_SEPARATOR = "\n-";
        private const string CONV_SEPARATOR = "\n-";
        private const string MEM_SEPARATOR = "\n-";
        // The format of the prompt.
        private string Format;
        // The variables that will be passed into the prompt.
        private Dictionary<string, string> variables;
        private List<string> memories;

        /// <summary>
        /// Creates a new prompt with the given format.
        /// </summary>
        /// <param name="format">The format of the prompt.</param>
        public Prompt(string format)
        {
            Format = format;
            variables = new Dictionary<string, string>();
            memories = new List<string>();
        }

        /// <summary>
        /// Adds a variable to the prompt.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value of the variable.</param>
        public void AddVariable(string name, string value)
        {
            variables.Add(name, value);
        }

        /// <summary>
        /// Adds a memory to the prompt which may consist of several memories.
        /// </summary>
        /// <param name="memory">The memory to be stored.</param>
        public void AddMemory(string memory)
        {
            memories.Add(memory);
        }

        /// <summary>
        /// Removes a variable from the prompt.
        /// </summary>
        /// <param name="name">The name of the variable to remove.</param>
        public void RemoveVariable(string name)
        {
            variables.Remove(name);
        }

        /// <summary>
        /// Generates the final prompt by passing all variables in.
        /// </summary>
        /// <returns>The final prompt.</returns>
        public string GeneratePrompt()
        {
            string prompt = Format;

            // Check if the variables conversation or instruction are present. If they are, then we need a stringbuilder to format them and to remove them from the dictionary.
            var conversation = variables.ContainsKey("conversation") ? variables["conversation"] : null;
            if (conversation != null)
            {
                var convoSb = new StringBuilder();

                convoSb.Append("Conversation:\n");
                convoSb.Append(conversation);
                variables.Remove("conversation");
            }
            var instructions = variables.ContainsKey("instructions") ? variables["instructions"] : null;
            if (instructions != null)
            {
                var instSb = new StringBuilder();

                instSb.Append("Instructions:\n");
                instSb.Append(instructions);
                variables.Remove("instructions");
            }
            if (memories.Count > 0)
            {
                var sb = new StringBuilder();
                sb.Append("Memories:\n");
                foreach (string memory in memories)
                {
                    sb.Append("    - ");
                    sb.Append(memory);
                    sb.Append("\n");
                }
                prompt = prompt.Replace("{memories}", sb.ToString());
            }
            // Write the remaining variables to the prompt.
            foreach (KeyValuePair<string, string> variable in variables)
            {
                prompt = prompt.Replace("{" + variable.Key + "}", variable.Value);
            }
            return prompt;
        }
    }
}
        