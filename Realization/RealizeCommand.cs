using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realization
{
    // Discord interaction module for creating new threads for the bot to respond to.
    public class RealizationModule : InteractionModuleBase<SocketInteractionContext>
    {
        private InteractionHandler _handler;

        public RealizationModule(InteractionHandler handler)
        {
            _handler = handler;
        }

        [SlashCommand("realize", "Creates a new thread for the bot to respond to.")]
        public async Task RealizeCommand([Summary("input", "The input from the user.")] string input) /*, [ChannelTypes(ChannelType.Text)] IChannel channel*/
        {
            await RespondAsync("Creating a new thread.");
            // Check if the channel is an ITextChannel, if it isn't then we can't create a thread.
            if (!(Context.Channel is ITextChannel text))
            {
                await RespondAsync("This command can only be used in a text channel.");
                return;
            }
            // Cast the channel.
            var textChannel = Context.Channel as ITextChannel;

            // Format the name Realization- with the current DateTime as a ulong.
            string name = $"Realization-{DateTime.Now.ToBinary()}";
            
            // Create a new thread for the bot to respond to.
            var thread = await textChannel.CreateThreadAsync(name, autoArchiveDuration: ThreadArchiveDuration.OneDay);

            // Add the user to the thread after checking if they can be cast to an IGuildUser.
            if (Context.User is IGuildUser user)
            {
                await thread.AddUserAsync(user);
            }
            // Send a message in the thread.
            await thread.SendMessageAsync("Hello, world!");
        }
    }
}
