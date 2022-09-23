#region using
global using Serilog;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Realization;
using PostSharp.Patterns.Diagnostics;
using PostSharp.Patterns.Diagnostics.Backends.Serilog;
#endregion

await Think();
await StayAlive();

async Task Think()
{
	AutomaticLogs();
	var key = Authorization();
	var discord = Discord();
	var commands = CommandService();
	TrackMessages(ref discord);
	await JoinDiscord(discord, key);
	await ExecutiveFunction(discord, commands);
}
async Task StayAlive() => await Task.Delay(-1);

void AutomaticLogs()
{
	var blueVerboseLogger = LogExtensions.VerboseLogger();
	Log.Logger = blueVerboseLogger;
	LoggingServices.DefaultBackend =
			new SerilogLoggingBackend(Log.ForContext("RuntimeContext", "PostSharp"));
}

string Authorization()
{
	var configuration = new ConfigurationBuilder()
		.AddJsonFile("appsettings.json", false)
		.Build();
	return configuration.GetSection("BotApiKey").Value.ToString();
}

DiscordSocketConfig Declarations() => new DiscordSocketConfig()
{
    GatewayIntents = GatewayIntents.All
};

DiscordSocketClient Discord() => new DiscordSocketClient(Declarations());

CommandService CommandService() => new CommandService();

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

async Task ExecutiveFunction(DiscordSocketClient client, CommandService commands) =>
	await Awareness(client, commands).ConfigureBotServices();

Thalamus Awareness(DiscordSocketClient client, CommandService commands) => new Thalamus(client, commands);