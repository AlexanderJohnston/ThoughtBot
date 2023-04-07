namespace Weaviation
{
    public class EmbeddedMessage
    {
        public string Text { get; set; }
        public ulong ThreadId { get; set; }
        public ulong UserId { get; set; }
        public ulong ContextId { get; set; }

        public EmbeddedMessage(string text, ulong threadId, ulong userId, ulong contextId)
        {
            Text = text;
            ThreadId = threadId;
            UserId = userId;
            ContextId = contextId;
        }
    }
  
}