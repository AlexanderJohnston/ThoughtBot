namespace Memory.Conversation
{
    public class AuditorySignal
    {
        public ulong Source { get; set; }
        public Guid MemoryId { get; set; }
        public string Context { get; set; }
        public string Topic { get; set; }
    }
}
