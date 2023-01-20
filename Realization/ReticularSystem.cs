using Discord;
using Discord.WebSocket;
using Memory.Converse;
using Realization.Skill;

namespace Realization
{
    public class ReticularSystem
    {
        ShortTermMemory<string> _memory;
        public SocketUser Remember = null;


        public ReticularSystem(ShortTermMemory<string> memory)
        {
            _memory = memory;
        }

        internal bool NotInterested(SocketMessage messageParam)
        {
            // Skip  bots
            if (messageParam.Author.IsBot)
            {
                return true;
            }
            // Skip system messages
            if (messageParam is IUserMessage)
                return false;
            else
                return true;
        }

        public bool FamiliarConversation(SocketUserMessage message)
        {
            if (KnownAuthor(message))
                return true;
            return false;
        }

        public bool KnownAuthor(SocketUserMessage message) => Remember != null && Remember == message.Author;

    }
}