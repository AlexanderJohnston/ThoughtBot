using Memory.Conversation;

namespace Realization.Converse
{
    /// <summary>
    /// Conversation is a class which receives a <see cref="AuditorySignal"/> and categorizes
    /// the signal into discrete conversations. It is a part of the ventral stream.
    /// The method of determining a conversational boundary is determined by a change in <see cref="AuditorySignal.Topic"/>.
    /// Any given conversational <see cref="AuditorySignal.Topic"/> can be associated with only one <see cref="AuditorySignal.Context"/>.
    /// If the <see cref="AuditorySignal.Topic"/> is the same, but the <see cref="AuditorySignal.Context"/> is different, then 
    /// the context should be added to a list.
    /// </summary>
    public class Conversation
    {
        public string Topic { get; set; }
        public ulong Author { get; set; }
        public List<string> Context { get; set; }
        public List<ulong> Speakers { get; set; }
        public List<Message> Memories { get; set; }
        
        public Conversation(AuditorySignal signal)
        {
            Topic = signal.Topic;
            Author = signal.Source;
            Speakers = new List<ulong>() { signal.Source };
            Context = new List<string>() { signal.Context };
            Memories = new List<Message>() { new Message(signal.Source, signal.MemoryId) };
        }
        //TODO
        // Add a new signal to the topic and determine if changes are necessary.
        public void Update(AuditorySignal signal)
        {
            if (!Context.Contains(signal.Context))
            {
                Context.Add(signal.Context);
            }
            if (!Speakers.Contains(signal.Source))
            {
                Speakers.Add(signal.Source);
            }
            Memories.Add(new Message(signal.Source, signal.MemoryId));
            //if (signal.Topic == Topic)
            //{
            //    if (!Context.Contains(signal.Context))
            //    {
            //        Context.Add(signal.Context);
            //    }
            //    if (!Speakers.Contains(signal.Source))
            //    {
            //        Speakers.Add(signal.Source);
            //    }
            //}
            //else
            //{
            //    Topic = signal.Topic;
            //    Context = new List<string>() { signal.Context };
            //}
        }
    }

    
}
