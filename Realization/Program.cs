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
    AutomaticLogs();
    var config = Authorization();
    var key = config.GetSection("RealizeBotApiKey").Value;
    var services = Initialize(config);
    var commands = services.GetRequiredService<CommandService>();
    commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: services);
    var interactions = services.GetRequiredService<InteractionHandler>();
    await interactions.InitializeAsync();
    //var discord = BasicDiscord();
    var discord = services.GetRequiredService<DiscordSocketClient>();
    var clue = CallCognitiveServices();
    //var commands = CommandService();
    TrackMessages(ref discord);
    await JoinDiscord(discord, key);
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

GetAClue CallCognitiveServices() => new();

// Create a new service collection, add an IConfiguration to it, the DiscordSocketClient, and an InteractionService.
// Then build the service provider.
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

IConfiguration Configure()
{
    return new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .Build();
}

IConfiguration Authorization()
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false)
        .Build();
    return configuration;
}

DiscordSocketConfig Declarations() => new DiscordSocketConfig()
{
    GatewayIntents = GatewayIntents.All
};

DiscordSocketClient BasicDiscord() => new(Declarations());

CommandService CommandService() => new();

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

Thalamus ExecutiveFunction(DiscordSocketClient client, CommandService commands, IServiceProvider services) => Awareness(client, commands, services);

Thalamus Awareness(DiscordSocketClient client, CommandService commands, IServiceProvider services) => new Thalamus(client, commands, CallCognitiveServices(), services);