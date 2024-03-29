﻿using Memory.Chat;
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
        private List<RoleMessage> conversation;

        /// <summary>
        /// Creates a new prompt with the given format.
        /// </summary>
        /// <param name="format">The format of the prompt.</param>
        public Prompt(string format)
        {
            Format = format;
            variables = new Dictionary<string, string>();
            memories = new();
            conversation = new();
        }

        /// <summary>
        /// Adds a variable to the prompt.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value of the variable.</param>
        public void AddVariable(string name, string value)
        {
            if (variables.ContainsKey(name))
            {
                variables[name] = value;
            }
            else
            {
                variables.Add(name, value);
            }
        }

        /// <summary>
        /// Adds a conversation to the prompt which may consist of several messages.
        /// </summary>
        /// <param name="memory">The memory to be stored.</param>
        public void AddConversation(string memory, string role)
        {
            var message = new RoleMessage(memory, role);
            conversation.Add(message);
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

        public void ResetMemory() => memories = new();
        public void ResetConversation() => conversation = new();
        

        public List<GptChatMessage> GenerateChat()
        {
            List<GptChatMessage> messages = new();
            var instructions = variables.ContainsKey("instructions") ? variables["instructions"] : null;
            if (instructions != null)
            {
                messages.Add(new GptChatMessage("system", instructions));
            }
            if (memories.Count > 0)
            {
                foreach (var memory in memories)
                {
                    messages.Add(new GptChatMessage("system", memory));
                }
            }
            if (conversation.Count > 0)
            {
                foreach (var msg in conversation)
                {
                    messages.Add(new GptChatMessage(msg.Role, msg.Message));
                }
            }
            return messages;
        }

        /// <summary>
        /// Generates the final prompt by passing all variables in.
        /// </summary>
        /// <returns>The final prompt.</returns>
        public string GeneratePrompt()
        {
            string prompt = Format;

            // Check if the variables conversation or instruction are present. If they are, then we need a stringbuilder to format them and to remove them from the dictionary.
            //var conversation = variables.ContainsKey("conversation") ? variables["conversation"] : null;
            if (this.conversation.Count > 0)
            {
                var sb = new StringBuilder();
                sb.Append("Conversation:\n");
                foreach (RoleMessage msg in this.conversation)
                {
                    sb.Append("    - ");
                    sb.Append(msg.Role);
                    sb.Append(": ");
                    sb.Append(msg.Message);
                    sb.Append("\n");
                }
                prompt = prompt.Replace("{conversation}", sb.ToString());
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append("Conversation:\n");
                sb.Append("\n");
            }
            var instructions = variables.ContainsKey("instructions") ? variables["instructions"] : null;
            if (instructions != null)
            {
                var instSb = new StringBuilder();

                instSb.Append("Instructions:\n");
                instSb.Append("    - ");
                instSb.Append(instructions);
                instSb.Append('\n');
                variables.Remove("instructions");
            }
            else
            {
                var instSb = new StringBuilder();
                instSb.Append("Instructions:\n");
                instSb.Append('\n');
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
            else
            {
                var sb = new StringBuilder();
                sb.Append("Memories:\n");
                sb.Append("\n");
            }
            // Write the remaining variables to the prompt.
            foreach (KeyValuePair<string, string> variable in variables)
            {
                prompt = prompt.Replace("{" + variable.Key + "}", variable.Value);
            }
            return prompt;
        }

        private class RoleMessage
        {
            public string Message { get; set; }
            public string Role { get; set; }

            public RoleMessage(string message, string role)
            {
                Message = message;
                Role = role;
            }
        }
    }
}
        