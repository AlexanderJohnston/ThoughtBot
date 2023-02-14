namespace Prompter
{
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
        