namespace Memory.Converse
{
    public class AuditorySignal
    {
        public string Text { get; set; }
        public ulong Source { get; set; }
        public Guid MemoryId { get; set; }
        public string Context { get; set; }
        public string Topic { get; set; }
        public ulong Channel { get; set; }
        public ulong Thread { get; set; }
        public AuditorySignal() { }
        public AuditorySignal(ulong source, Guid memoryId, string context, string topic, ulong channel, ulong thread, string text)
        {
            Source = source;
            MemoryId = memoryId;
            Context = context;
            Topic = topic;
            Channel = channel;
            Thread = thread;
            Text = text;
        }
    }
}
