using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Prompter;
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

        [SlashCommand("character", "Set up a character for Realai to act.")]
        public async Task CharacterModal()
        {
            var mb = new ModalBuilder()
                .WithTitle("Character Actor")
                .WithCustomId("character-menu")
                .AddTextInput("Character:", "character", TextInputStyle.Paragraph, placeholder: "Describe a character.", required: false);

            await RespondWithModalAsync(mb.Build());
        }

        [SlashCommand("settings", "Configure Realai to your liking.")]
        public async Task SettingsModal()
        {
            var mb = new ModalBuilder()
                .WithTitle("Thalamus Settings")
                .WithCustomId("settings-menu")
                .AddTextInput("Temperature: 0.0 - 2.0 how random bot behaves", "temperature", TextInputStyle.Short, placeholder: "0.7", maxLength: 3, minLength: 3)
                .AddTextInput("Default Prompt:", "prompt", TextInputStyle.Paragraph, placeholder: "vars:{instruction} {conversation} {memories}", required: false);

            await RespondWithModalAsync(mb.Build());
        }

        [SlashCommand("converse", "Create a new conversation with Realai.")]
        public async Task Converse()
        {
            var menuBuilder = new SelectMenuBuilder()
                .WithPlaceholder("What type of conversation would you like to have?")
                .WithCustomId("conversation-type-menu")
                .WithMinValues(1)
                .WithMaxValues(1)
                .AddOption("Chat", "opt-chat", "Talk with Realai using the latest Chat GPT!")
                .AddOption("Davinci", "opt-davinci", "Talk with Realai using GPT-3 Davinci!")
                .AddOption("Characterize", "opt-character", "Personify Realai and have a conversation with them!")
                .AddOption("Teach", "opt-teach", "Teach Realai something new!")
                .AddOption("Recall", "opt-memory", "Recall memories with Realai!");

            var builder = new ComponentBuilder()
                .WithSelectMenu(menuBuilder);

            await RespondAsync("What type of conversation would you like to have?", components: builder.Build());
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
