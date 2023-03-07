using Discord;
using Discord.WebSocket;
using PostSharp.Patterns.Diagnostics;
using static PostSharp.Patterns.Diagnostics.FormattedMessageBuilder;

internal class Commands
{
    public async Task MessageReceived(SocketMessage message)
    {
        // Serves as an echo to confirm if the bot is online or not.
        if (message.Content == "!ping")
            await message.Channel.SendMessageAsync("Received!");
    }
    public Task LogMessage(LogMessage msg)
    {
        LogSource.Get().Info.Write(Formatted(msg.ToString()));
        return Task.CompletedTask; // required by Discord client
    }
}