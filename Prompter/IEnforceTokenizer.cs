namespace Prompter
{

    public interface IEnforceTokens
    {
        // Check if a single input can be added to the conversation.
        public bool AllowConversation(string conversation);
        // Check if many inputs can be added to the conversation.
        public bool AllowConversation(IEnumerable<string> conversations);
        // Check if an input can be added to the embedding.
        public bool AllowEmbedding(string embedding);
        // Check if multiple inputs can be added to the embedding.
        public bool AllowEmbedding(IEnumerable<string> embedding);
        // Check if an input can be added to the system instruction.
        public bool AllowInstruction(string instruction);
        // Check if many inputs can be added to the system instruction.
        public bool AllowInstruction(IEnumerable<string> instruction);
    }

    public interface ITrackTokens
    {
        // Basic tokenization of an input using gpt2 tokenizer.
        List<int> Tokenize(string input);
        // How many tokens does an input contain.
        int TokenCount(string input);

        int GetTotalLimit();
        int GetIntructionLimit();
        int GetConversationLimit();
        int GetEmbeddingLimit();
        int SetTotalLimit();
        int AddInstruction(int count);
        int DropInstruction(int count);
    }

    public interface IEnforceTokenizer
    {
        List<int> Tokenize(string input);
        int CountTokens(List<int> tokens);
        public bool AddConversation(string conversation);
        public bool AddConversation(IEnumerable<string> conversations);
        public bool AddEmbedding(string embedding);
        public bool AddEmbedding(IEnumerable<string> embedding);
        public bool AddInstruction(string instruction);
        public bool AddInstruction(IEnumerable<string> instruction);
        public int TotalTokens();
    }
}
        