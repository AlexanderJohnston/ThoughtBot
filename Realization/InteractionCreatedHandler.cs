using Discord;
using Discord.WebSocket;
using Realization.Global;
using Realization.Perception;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Realization
{
    public class InteractionCreatedHandler
    {
        private ThreadWeaver _weaver;

        private UserSettings _modals;

        private Cortex _cortexRef;

        public InteractionCreatedHandler(ThreadWeaver weaver, Cortex cortex)
        {
            _weaver = weaver;
            _modals = new();
            _cortexRef = cortex;
        }

        public double GetUserTemperature(ulong userId) => _modals.GetTemperature(userId);
        public string GetUserPrompt(ulong userId) => _modals.GetPrompt(userId);

        public async Task MyModalHandler(SocketModal modal)
        {
            switch (modal.Data.CustomId)
            {
                case "settings-menu":
                    await SettingsModalHandler(modal);
                    break;
                case "character-menu":
                    await CharacterModalHandler(modal);
                    break;
            }
        }
        
        private async Task CharacterModalHandler(SocketModal modal)
        {
            var components = modal.Data.Components.ToList();
            var character = components.First(field => field.CustomId == "character").Value;
            if (!string.IsNullOrEmpty(character))
            {
                _weaver.AddCharacter(modal.User.Id, character);
                await modal.RespondAsync("Character updated.");
            }
            else
            {
                await modal.RespondAsync("Character cleared.");
            }
        }

        private async Task SettingsModalHandler(SocketModal modal)
        {
            var components = modal.Data.Components.ToList();
            var temperature = components.First(field => field.CustomId == "temperature").Value;
            var prompt = components.First(field => field.CustomId == "prompt").Value;

            double inputParsed;
            var temperatureValid = Double.TryParse(temperature, out inputParsed);
            float parsedTemperature = (float)inputParsed;
            if (temperatureValid)
            {
                _modals.AddTemperature(modal.User.Id, parsedTemperature);
                _cortexRef.AddTemperature(modal.User.Id, parsedTemperature);
            }
            if (!string.IsNullOrEmpty(prompt))
            {
                _modals.AddPrompt(modal.User.Id, prompt);
                _weaver.UpdateUserSetting(modal.User.Id, prompt);
            }
            else
            {
                _modals.AddPrompt(modal.User.Id, prompt);
                _weaver.DropUserSettings(modal.User.Id);
            }

            await modal.RespondAsync("Settings updated.");
        }

        public async Task MyButtonHandler(SocketMessageComponent component)
        {
            // We can now check for our custom id
            switch (component.Data.CustomId)
            {
                case "test-version-0.0":
                    await component.RespondAsync($"{component.User.Mention} has clicked the button!");
                    break;
            }
        }

        public async Task MyMenuHandler(SocketMessageComponent component)
        {
            // We can now check for our custom id
            switch (component.Data.CustomId)
            {
                case "conversation-type-menu":
                    var option = await ConversationMenuHandler(component);
                    await NewThreadHandler(component, option);
                    //await component.RespondAsync($"{component.User.Mention} has selected ${component.Data.Values}");
                    break;
            }
            var text = string.Join(", ", component.Data.Values);
            await component.RespondAsync($"Values: {text}");
        }

        public async Task<string> ConversationMenuHandler(SocketMessageComponent component)
        {
            var value = component.Data.Values.First();
            SelectMenuComponent? selectedMenuComponent = (component.Message.Components.First().Components.First() as SelectMenuComponent);
            string placeHolder = string.Empty;
            var selectedValue = selectedMenuComponent?.Options.First(x => x.Value == value);
            if (selectedMenuComponent != null)
            {
                placeHolder = selectedMenuComponent.Placeholder;
            }
            var menuBuilder = new SelectMenuBuilder()
                .WithPlaceholder(selectedValue.Label)
                .WithCustomId("conversation-type-menu")
                .WithMinValues(1)
                .WithMaxValues(1)
                .AddOption("Chat", "opt-chat", "Talk with Vertai using the latest Chat GPT!")
                .AddOption("Davinci", "opt-davinci", "Talk with Vertai using GPT-3 Davinci!")
                .AddOption("Characterize", "opt-character", "Personify Vertai and have a conversation with them!")
                .AddOption("Teach", "opt-teach", "Teach Vertai something new!")
                .AddOption("Recall", "opt-memory", "Recall memories with Vertai!")
                .WithDisabled(true);

            var builder = new ComponentBuilder()
                .WithSelectMenu(menuBuilder);

            await component.UpdateAsync(original =>
            {
                original.Content = $"Creating a new thread for <@{component.User.Id}> to {(selectedValue?.Label ?? value).ToLower()}.";
                original.Components = new ComponentBuilder().WithSelectMenu(menuBuilder).Build();
            });
            return value;
        }

        public async Task NewThreadHandler(SocketMessageComponent component, string option)
        {
            // Check if the channel is an ITextChannel, if it isn't then we can't create a thread.
            if (!(component.Channel is ITextChannel text))
            {
                await component.RespondAsync("This command can only be used in a top-level text channel.");
                return;
            }
            // Cast the channel.
            var textChannel = component.Channel as ITextChannel;
            
            // Format the name Realization- with the current DateTime as a ulong.
            string name = $"Realization-{DateTime.Now.ToBinary()}";

            // Create a new thread for the bot to respond to.
            var thread = await textChannel.CreateThreadAsync(name, autoArchiveDuration: ThreadArchiveDuration.OneDay);
            Instructor.AddInstruction(thread.Id, option);

            // Add the user to the thread after checking if they can be cast to an IGuildUser.
            if (component.User is IGuildUser user)
            {
                await thread.AddUserAsync(user);
            }
            // Send a message in the thread.
            if (!_weaver.Exists(thread.Id))
            {
                _weaver.InitializeThread(thread.Id, option, component.User.Id);
                if (option == "opt-character")
                {
                    var character = _weaver.GetCharacter(component.User.Id);
                    _weaver.NewThread(thread.Id);
                    _weaver.ThreadInstructions(character, thread.Id);
                }
                else
                {
                    _weaver.NewThread(thread.Id);
                }
            }
            if (option == "opt-chat")
            {
                await thread.SendMessageAsync("This thread is using Chat GPT 3.5 Turbo model release March 1st 2023.");
                return;
            }
            var defaultPrompt = _weaver.GetDefaultPrompt(thread.Id);
            await thread.SendMessageAsync("Prompt used for this thread:\n---\n");
            await thread.SendMessageAsync(defaultPrompt.GeneratePrompt());
        }

    }
}
