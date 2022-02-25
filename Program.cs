using System;
using System.Threading.Tasks;
using dotenv.net;
using dotenv.net.Utilities;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using CollectibleBot.Modules;

namespace CollectibleBot
{
	class Program
	{
		private DiscordSocketClient _client;
		private CommandService _commands;
		private IServiceProvider _services;

		private CommandHandler _handler;

		static Task Main(string[] args) => new Program().MainAsync();

		public async Task MainAsync()
		{
			Startup();

			AssignEvents();

			var token = EnvReader.GetStringValue("TOKEN");

			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();

			await Task.Delay(-1);
		}

		private void Startup()
		{
			Initialize init = new();

			init.LoadEnv();
			_services = init.BuildServiceProvider();

			_client = _services.GetService<DiscordSocketClient>();
			_commands = _services.GetService<CommandService>();
		}

		private void AssignEvents()
		{
			_client.Log += Log;
			_client.Ready += ClientReady;
		}

		private async Task ClientReady()
		{

			_handler = new CommandHandler(_services);

			await _handler.InstallCommandsAsync();


			Console.WriteLine("[STARTUP] CollectibleBot Online!");
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
	}
}
