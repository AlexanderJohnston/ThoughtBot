using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realization
{
    public class InteractionCreatedHandler
    {
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

        public async Task MyMenuHandler(SocketMessageComponent arg)
        {
            var text = string.Join(", ", arg.Data.Values);
            await arg.RespondAsync($"You have selected {text}");
        }
    }
}
