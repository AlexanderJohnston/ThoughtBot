using AI.Dev.OpenAI.GPT;

namespace Prompter
{


    /// <summary>
    /// This class helps to limit the length of strings by returning a substring of the string which is less than or equal to the allowed length.
    /// It has a limit for each type of prompt variable, conversation, embeddings, instructions, and a 20 token buffer of empty space.
    /// The class should track the amount of tokens used in total as well as the amount of tokens used for each type of prompt variable.
    /// If the maximum is reached for a prompt variable, then we should stop adding the string into memory.
    /// </summary>
    public class Tokenizer : IEnforceTokenizer, IDisposable
    {
        // The maximum amount of tokens allowed for each conversation.
        private const int MAX_CONVERSATION_TOKENS = 1700;
        // The maximum amount of tokens allowed for each embedding.
        private const int MAX_EMBEDDING_TOKENS = 1200;
        // The maximum amount of tokens allowed for each instruction.
        private const int MAX_INSTRUCTION_TOKENS = 1000;
        // The maximum amount of tokens allowed for each prompt.
        private const int MAX_TOTAL_TOKENS = 4000;
        // The maximum amount of tokens allowed for each prompt.
        private const int MAX_BUFFER_TOKENS = 20;
        // The total amount of tokens used.
        private int totalTokens;
        // The amount of tokens used for each conversation.
        private int conversationTokensUsed;
        // The amount of tokens used for each embedding.
        private int embeddingTokensUsed;
        // The amount of tokens used for each instruction.
        private int instructionTokensUsed;

        /// <summary>
        /// Creates a new tokenizer.
        /// </summary>
        public Tokenizer()
        {
            totalTokens = 0;
            conversationTokensUsed = 0;
            embeddingTokensUsed = 0;
            instructionTokensUsed = 0;
        }

        /// <summary>
        /// Tokenizes the given string.
        /// </summary>
        /// <param name="input">The string to tokenize.</param>
        /// <returns>The tokens of the string.</returns>
        public List<int> Tokenize(string input)
        {
            return GPT3Tokenizer.Encode(input);
        }

        /// <summary>
        /// Counts the number of tokens in the given array.
        /// </summary>
        /// <param name="tokens">The array of tokens.</param>
        /// <returns>The number of tokens in the array.</returns>
        public int CountTokens(List<int> tokens)
        {
            return tokens.Count;
        }

        public bool AddConversation(string conversation)
        {
            if (conversationTokensUsed + CountTokens(Tokenize(conversation)) > MAX_CONVERSATION_TOKENS)
            {
                return false;
            }
            conversationTokensUsed += CountTokens(Tokenize(conversation));
            return true;
        }

        public bool AddConversation(IEnumerable<string> conversations)
        {
            foreach (string conversation in conversations)
            {
                if (!AddConversation(conversation))
                {
                    return false;
                }
            }
            return true;
        }

        public bool AddEmbedding(string embedding)
        {
            if (embeddingTokensUsed + CountTokens(Tokenize(embedding)) > MAX_EMBEDDING_TOKENS)
            {
                return false;
            }
            embeddingTokensUsed += CountTokens(Tokenize(embedding));
            return true;
        }

        public bool AddEmbedding(IEnumerable<string> embedding)
        {
            foreach (string embed in embedding)
            {
                if (!AddEmbedding(embed))
                {
                    return false;
                }
            }
            return true;
        }

        public bool AddInstruction(string instruction)
        {
            if (instructionTokensUsed + CountTokens(Tokenize(instruction)) > MAX_INSTRUCTION_TOKENS)
            {
                return false;
            }
            instructionTokensUsed += CountTokens(Tokenize(instruction));
            return true;
        }

        public bool AddInstruction(IEnumerable<string> instruction)
        {
            foreach (string instr in instruction)
            {
                if (!AddInstruction(instr))
                {
                    return false;
                }
            }
            return true;
        }

        public int TotalTokens()
        {
            return totalTokens;
        }

        public void ResetMemoryTokens() => embeddingTokensUsed = 0;
        public void ResetConversationTokens() => conversationTokensUsed = 0;


        public void Dispose()
        {
            totalTokens = 0;
            conversationTokensUsed = 0;
            embeddingTokensUsed = 0;
            instructionTokensUsed = 0;
        }
    }
}
        