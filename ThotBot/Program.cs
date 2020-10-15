using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using PostSharp.Patterns.Diagnostics;
using PostSharp.Patterns.Diagnostics.Backends.Serilog;
using Serilog;
using System;
using System.Threading.Tasks;

namespace ThotBot
{
    class Program
    {
		public static void Main(string[] args)
			=> new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			SetupLogging();
			SetupConfiguration();

			_client = new DiscordSocketClient();
			LoadCommandsFor(_client);
			await StartAsync(_client);
			await LoadCortex(_client, _commandService);

			// Block this task until the program is closed.
			await Task.Delay(-1);
		}

		public DiscordSocketClient LoadCommandsFor(DiscordSocketClient client)
        {
			var commands = new Commands();
			_commandService = new CommandService();
			client.Log += LogMessage;
			client.MessageReceived += commands.MessageReceived;
			return client;
		}

		public async Task StartAsync(DiscordSocketClient client)
        {
			await client.LoginAsync(TokenType.Bot, _botApiKey);
			await client.StartAsync();
		}

		public async Task LoadCortex(DiscordSocketClient client, CommandService commands)
        {
			var skills = new Cortex(client, commands);
			await skills.ConfigureBotServices();
		}

		private string _botApiKey;
		private ILogger _log;
		private DiscordSocketClient _client;
		private CommandService _commandService;

		private void SetupLogging()
		{
			var blueVerboseLogger = LogExtensions.VerboseLogger();
			Log.Logger = blueVerboseLogger;
			_log = Log.ForContext<Program>();
			LoggingServices.DefaultBackend =
					new SerilogLoggingBackend(_log.ForContext("RuntimeContext", "PostSharp"));
		}

		private void SetupConfiguration()
		{
			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", false)
				.Build();
			_botApiKey = configuration.GetSection("BotApiKey").Value.ToString();
		}

		private Task LogMessage(LogMessage msg)
		{
			_log.Information(msg.ToString());
			return Task.CompletedTask;
		}

	}
}
