using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prompter
{
    /// <summary>
    /// This class is responsible for taking some template, the <see cref="Prompt"/> and filling it with variables.
    /// The <see cref="Tokenizer"/> is used to ensure that variables do not exceed the maximum token length when being passed to the AI model.
    /// It should have a method to add a variable to the prompt which returns a boolean indicating whether or not the variable was added based on the <see cref="Tokenizer"/>.
    /// The tokenizer can take three different types of variables, instructions, conversation, and memories. The type is used to call the correct method on the <see cref="Tokenizer"/>.
    /// Each different type of variable should have its own method to add it to the prompt.
    /// There is no need to remove variables as they can not be added if they exceed the length anyway.
    /// Before returning the result of the <see cref="Tokenizer"/>, the <see cref="Prompt"/> should be updated with the new variable.
    /// </summary>
    public class Loom
    {
        // The prompt to be filled with variables.
        private Prompt prompt;
        // The tokenizer used to ensure that variables do not exceed the maximum token length when being passed to the AI model.
        private Tokenizer tokenizer;

        /// <summary>
        /// Creates a new <see cref="Loom"/> with the given <see cref="Prompt"/> and <see cref="Tokenizer"/>.
        /// </summary>
        /// <param name="prompt">The prompt to be filled with variables.</param>
        /// <param name="tokenizer">The tokenizer used to ensure that variables do not exceed the maximum token length when being passed to the AI model.</param>
        public Loom(Prompt prompt, Tokenizer tokenizer)
        {
            this.prompt = prompt;
            this.tokenizer = tokenizer;
        }

        /// <summary>
        /// Adds an instruction variable to the prompt.
        /// </summary>
        /// <param name="instruction">The instruction to add.</param>
        /// <returns>A boolean indicating whether or not the instruction was added.</returns>
        public bool AddInstruction(string instruction)
        {
            if (tokenizer.AddInstruction(instruction))
            {
                prompt.AddVariable("instruction", instruction);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a conversation variable to the prompt. The conversation is assumed to be pre-formatted because 
        /// it is assumed that the conversation will be stored in a database.
        /// </summary>
        /// <param name="conversation">The conversation to add.</param>
        /// <returns>A boolean indicating whether or not the conversation was added.</returns>
        public bool AddConversation(string conversation)
        {
            if (tokenizer.AddConversation(conversation))
            {
                prompt.AddConversation(conversation);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a memory variable to the prompt. The memory is assumed to be a string properly formatted to be displayed in a list of other memories.
        /// </summary>
        /// <param name="memory">The memory to add.</param>
        /// <returns>A boolean indicating whether or not the memory was added.</returns>
        public bool AddMemory(string memory)
        {
            if (tokenizer.AddEmbedding(memory))
            {
                // Special case because memories are stored in a list.
                prompt.AddMemory(memory);
                return true;
            }
            return false;
        }

        public void ResetMemory()
        {
            prompt.ResetMemory();
            //prompt.ResetConversation();
            tokenizer.ResetMemoryTokens();
            //tokenizer.ResetConversationTokens();

        }

        /// <summary>
        /// Weaves the final prompt by passing all variables in. Disposes of the tokenizer afterward to clear memory.
        /// </summary>
        /// <returns>The final prompt after disposing of the tokenizer.</returns>
        public string GeneratePrompt()
        {
            string finalPrompt = prompt.GeneratePrompt();
            //tokenizer.Dispose();
            return finalPrompt;
        }

        /// <summary>
        /// Gets the total token count from the <see cref="Tokenizer"/>.
        /// </summary>
        /// <returns>The total token count.</returns>
        public int GetTokenCount()
        {
            return tokenizer.TotalTokens();
        }

        public void AddMemory(object value)
        {
            throw new NotImplementedException();
        }
    }
}
