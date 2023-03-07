# About
This bot integrates GPT-3 Davinci, GPT-3 Ada embedding, and Chat-GPT 3.5 to create a bot with short and long term memories which can respond to a variety of prompts, including user-defined prompts.

Code exists inside for integrating Microsoft CLU as well, but is not currently enabled or used while I focus on Chat GPT.

# Requirements

PostSharp is required by default, but you can easily disable it by deleting GlobalAssembly.cs and removing the nuget packages from the `Realization` project. I highly recommend just using a PostSharp Trial if you want to experiment with my bot though. The logs are helpful.

[PostSharp](https://www.postsharp.net/) is not required to use the compiled version of the bot however. I will have a compiled version of it up on the Releases page for use by 3/9/2023

# Setup

Step 1. Create a new Discord Developer application.

Step 2. Set the bot's name to Realization, and copy the bot's token to `appsettings.json` as `"RealizeBotApiKey"`.

Step 3. Save your OpenAI API key to `key.openAI` in the folder where the bot is running. If debug then this will be `/bin/..`

Step 4. Right click your test server and copy the server ID to `"TestGuild"`

    {
      "RealizeBotApiKey": "ex-yourKey",
      "TestGuild": yourGuildId
    }
    
Step 5. Invite the bot to your server with the scopes:

    -bot
      -Manage Server
      -Send Messages
      -Create Public Threads
      -Create Private Threads
      -Send Messages In Threads
      -Manage Messages
      -Manage Threads
      -Read Message History
      -Add Reactions
      -Use Slash Commands
      
Step 6. type "move Realization here" in the channel you want it to listen.

Step 7. try using /converse to select a conversational prompt for Chat-GPT, /settings to change the default prompt, and /realize to call GPT-3.
