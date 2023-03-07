#region using
global using Serilog;
using AzureLUIS;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostSharp.Patterns.Diagnostics;
using PostSharp.Patterns.Diagnostics.Backends.Serilog;
using Realization;
using System.Reflection;
#endregion

await Think();
await StayAlive();

async Task Think()
{
    // PostSharp automatically logs all functions to the console.
    // You can disable this by removing GlobalAssembly.cs and the PostSharp NuGet package if it causes compiler errors.
    // Don't forget to come back and either add your own logger or drop this line.
    AutomaticLogs();

    // Reads the following keys from `appsettings.json`
    // "RealizeBotApiKey": "sk-long-key-string"    <---- Your Discord API key
    // "TestGuild": 1077789543006208072            <---- Your Test Server ID
    var config = Authorization();
    var key = config.GetSection("RealizeBotApiKey").Value;

    // This builds your service collection and then injects the current assembly modules as commands for Discord.NET
    // You can add more modules by adding them to the `Realization` namespace and adding them to the `AddModulesAsync` call.
    var services = Initialize(config);
    var commands = services.GetRequiredService<CommandService>();
    commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: services);

    // Set up the interaction handler for interactive commands, modals, and menus.
    var interactions = services.GetRequiredService<InteractionHandler>();
    await interactions.InitializeAsync();

    // Set up the client and connect to Discord.
    var discord = services.GetRequiredService<DiscordSocketClient>();
    TrackMessages(ref discord);
    await JoinDiscord(discord, key);

    // Loop forever and respond to users.
    ExecutiveFunction(discord, commands, services);
}

async Task StayAlive() => await Task.Delay(-1);

void AutomaticLogs()
{
    var blueVerboseLogger = LogExtensions.VerboseLogger();
    Log.Logger = blueVerboseLogger;
    LoggingServices.DefaultBackend =
            new SerilogLoggingBackend(Log.ForContext("RuntimeContext", "PostSharp"));
}

// Calls Microsoft Cognitive Language Understanding. Not currently supported on this release of Realization bot.
GetAClue CallCognitiveServices() => new();

// Create a new service collection, add the IConfiguration to it, the DiscordSocketClient, and an InteractionService to handle commands.
// You can add additional services here for dependency injection w/ Discord.NET
IServiceProvider Initialize(IConfiguration config)
{
    var services = new ServiceCollection()
        .AddSingleton(config)
        .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All,
            LogLevel = LogSeverity.Verbose,
            MessageCacheSize = 1000,
        }))
        .AddSingleton(new CommandService(new CommandServiceConfig
        {
            LogLevel = LogSeverity.Verbose,
            DefaultRunMode = Discord.Commands.RunMode.Async,
            CaseSensitiveCommands = false,
        }))
        .AddSingleton(x => new InteractionService(
            x.GetRequiredService<DiscordSocketClient>(), 
            new InteractionServiceConfig
        {
            LogLevel = LogSeverity.Verbose
        }))
        .AddSingleton<InteractionHandler>()
        .BuildServiceProvider();
    return services;
}

IConfiguration Authorization()
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false)
        .Build();
    return configuration;
}

// Loop the Discord logger into the Postsharp logger or whatever you prefer.
void TrackMessages(ref DiscordSocketClient client)
{
    var commands = new Commands();
    client.Log += commands.LogMessage;
    client.MessageReceived += commands.MessageReceived;
    return;
}

async Task JoinDiscord(DiscordSocketClient client, string key)
{
    await client.LoginAsync(TokenType.Bot, key);
    await client.StartAsync();
}

// Integrates MS CLU awareness with OpenAI GPT responses. Currently this is redundant as no calls are made to CLU in this build.
Thalamus ExecutiveFunction(DiscordSocketClient client, CommandService commands, IServiceProvider services) => Awareness(client, commands, services);

Thalamus Awareness(DiscordSocketClient client, CommandService commands, IServiceProvider services) => new Thalamus(client, commands, CallCognitiveServices(), services);