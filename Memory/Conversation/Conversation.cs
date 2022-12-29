namespace Realization.Converse
{
    public class AuditorySignal
    {
        public ulong Source { get; set; }
        public Guid MemoryId { get; set; }
        public string Context { get; set; }
        public string Topic { get; set; }
    }

    /// <summary>
    /// Conversation is a class which receives a <see cref="AuditorySignal"/> and categorizes
    /// the signal into discrete conversations. It is a part of the ventral stream.
    /// The method of determining a conversational boundary is determined by a change in <see cref="AuditorySignal.Topic"/>.
    /// Any given conversational <see cref="AuditorySignal.Topic"/> can be associated with only one <see cref="AuditorySignal.Context"/>.
    /// If the <see cref="AuditorySignal.Topic"/> is the same, but the <see cref="AuditorySignal.Context"/> is different, then 
    /// </summary>
    public class Conversation
    {

    }

    
}
