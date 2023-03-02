using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Realization.Global;
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

        [SlashCommand("button-test", "Testing button interaction components.")]
        public async Task Spawn()
        {
            var builder = new ComponentBuilder().WithButton("Realization button test", "test-version-0.0");
            await ReplyAsync("Here is a button!", components: builder.Build());
        }

        [SlashCommand("modal-test", "Test modal.")]
        public async Task ModalTest()
        {
            var mb = new ModalBuilder()
    .WithTitle("Fav Food")
    .WithCustomId("food_menu")
    .AddTextInput("What??", "food_name", placeholder: "Pizza")
    .AddTextInput("Why??", "food_reason", TextInputStyle.Paragraph, "Kus it's so tasty");

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }

        [SlashCommand("converse", "Create a new conversation with the bot.")]
        public async Task Converse()
        {
            var menuBuilder = new SelectMenuBuilder()
                .WithPlaceholder("What type of conversation would you like to have?")
                .WithCustomId("conversation-type-menu")
                .WithMinValues(1)
                .WithMaxValues(1)
                .AddOption("Chat", "opt-chat", "Talk with Vertai!")
                .AddOption("Characterize", "opt-character", "Personify Vertai and have a conversation with them!")
                .AddOption("Teach", "opt-teach", "Teach Vertai something new!")
                .AddOption("Recall", "opt-memory", "Recall memories with Vertai!");

            var builder = new ComponentBuilder()
                .WithSelectMenu(menuBuilder);

            await ReplyAsync("Type of conversation selected", components: builder.Build());
        }

        [SlashCommand("realize", "Creates a new thread for the bot to respond to.")]
        public async Task RealizeCommand([Discord.Interactions.Summary("instructions", "Describe how you want the bot to behave. There is no limit to imagination.")] string input) /*, [ChannelTypes(ChannelType.Text)] IChannel channel*/
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
            Instructor.AddInstruction(thread.Id, input);

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
