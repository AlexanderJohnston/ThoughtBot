using Memory.Converse;

namespace Realization.Skill
{
    // The following class, MultiTasking, is a class which is used to categorize conversations by their channel
    // It is a part of the ventral stream.
    public class MultiTasking
    {
        public Dictionary<ulong, VentralStream> Channels = new();
        public Dictionary<ulong, VentralStream> Threads = new();

        public MultiTasking() {}
        public MultiTasking(Dictionary<ulong, VentralStream> channels, Dictionary<ulong, VentralStream> threads)
        {
            Channels = channels;
            Threads = threads;
        }
        // This method is used to look up a thread by id.
        public VentralStream? GetThread(ulong id)
        {
            if (Threads.ContainsKey(id))
            {
                return Threads[id];
            }
            return null;
        }
        // This method is used to look up a channel by id.
        public VentralStream? GetChannel(ulong id)
        {
            if (Channels.ContainsKey(id))
            {
                return Channels[id];
            }
            return null;
        }
        // This method is used to listen to a signal and then categorize it by its thread.
        // It allows for multiple ventral streams to be used at once based on id.
        public void ListenThread(AuditorySignal signal)
        {
            
            if (signal.Thread != default)
            {
                // Check if the signal matches a thread.
                if (Threads.ContainsKey(signal.Thread))
                {
                    Threads[signal.Channel].Listen(signal);
                    return;
                }
                else
                {
                    var ventralStream = new VentralStream();
                    ventralStream.Listen(signal);
                    Threads.Add(signal.Thread, ventralStream);
                    return;
                }
            }
        }
        // This method is used to listen to a signal and then categorize it by its channel.
        // It allows for multiple ventral streams to be used at once based on id.
        public void ListenChannel(AuditorySignal signal)
        {
            if (signal.Channel != default)
            {
                // Check if the signal matches a channel.
                if (Channels.ContainsKey(signal.Channel))
                {
                    Channels[signal.Channel].Listen(signal);
                    return;
                }
                else
                {
                    var ventralStream = new VentralStream();
                    ventralStream.Listen(signal);
                    Channels.Add(signal.Channel, ventralStream);
                    return;
                }
            }
        }
    }
}
